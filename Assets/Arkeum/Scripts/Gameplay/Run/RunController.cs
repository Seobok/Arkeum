using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Combat;
using Arkeum.Production.Gameplay.Interaction;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    public sealed class RunController
    {
        private readonly TurnSystem turnSystem;
        private readonly CombatSystem combatSystem;
        private readonly EnemyTurnSystem enemyTurnSystem;
        private readonly InteractionSystem interactionSystem;
        private readonly MapService mapService;
        private readonly ActorRepository actorRepository;
        private readonly TimingService timingService;

        public RunState CurrentRun { get; private set; }
        public string LastMessage { get; private set; } = string.Empty;

        public RunController(
            TurnSystem turnSystem,
            CombatSystem combatSystem,
            EnemyTurnSystem enemyTurnSystem,
            InteractionSystem interactionSystem,
            MapService mapService,
            ActorRepository actorRepository,
            TimingService timingService)
        {
            this.turnSystem = turnSystem;
            this.combatSystem = combatSystem;
            this.enemyTurnSystem = enemyTurnSystem;
            this.interactionSystem = interactionSystem;
            this.mapService = mapService;
            this.actorRepository = actorRepository;
            this.timingService = timingService;
        }

        public void Begin(RunState runState)
        {
            CurrentRun = runState;
        }

        public RunState CreateRunState(SaveProfile profile, RunStartingLoadoutDefinition startingLoadout)
        {
            return new RunState
            {
                RunIndex = (profile?.TotalReturns ?? 0) + 1,
                CurrentFloor = 1,
                TurnCount = 0,
                Gold = 0,
                BandageCount = startingLoadout != null ? startingLoadout.BandageCount : 0,
                AttackBonus = 0,
                FloorExitUsed = false,
                HasEquippedWeapon = startingLoadout?.Weapon != null,
                EquippedWeapon = startingLoadout != null ? startingLoadout.Weapon : null,
                IsTimingModeEnabled = false,
                EndReason = RunEndReason.None,
            };
        }

        public PlayerActionResultType TryHandlePlayerAction(Vector2Int direction)
        {
            if (CurrentRun?.Player == null)
            {
                return PlayerActionResultType.NotHandled;
            }

            Vector2Int targetCell = CurrentRun.Player.GridPosition + direction;
            if (TryGetPlayerAttackTarget(direction, out ActorEntity enemy, out WeaponAttackContext attackContext))
            {
                if (timingService != null && timingService.TryBegin(CurrentRun, attackContext, out TimingSession _))
                {
                    SetMessage("Time your strike.");
                    return PlayerActionResultType.TimingChallengeStarted;
                }

                ResolvePlayerAttack(enemy, attackContext);
                return PlayerActionResultType.Handled;
            }

            if (TryHandleRunInteractionAt(targetCell))
            {
                return PlayerActionResultType.Handled;
            }

            if (!mapService.IsWalkable(targetCell))
            {
                SetMessage("The path is blocked.");
                return PlayerActionResultType.NotHandled;
            }

            CurrentRun.Player.GridPosition = targetCell;
            if (!TryAutoPickupAtPlayerPosition())
            {
                SetMessage(string.Empty);
            }

            ConsumeTurn();
            return PlayerActionResultType.Handled;
        }

        public bool ResolveTimedAttack(TimingAttackResult timingResult)
        {
            TimingSession session = timingService?.CurrentSession;
            WeaponAttackContext attackContext = session?.AttackContext;
            ActorEntity enemy = attackContext?.Defender;
            if (attackContext == null || enemy == null || CurrentRun?.Player == null)
            {
                timingService?.CancelCurrent();
                return false;
            }

            timingService.CompleteCurrent(timingResult.Grade);
            attackContext.TimingResult = timingResult;
            ResolvePlayerAttack(enemy, attackContext);
            return true;
        }

        public void ToggleTimingMode()
        {
            if (CurrentRun == null)
            {
                return;
            }

            CurrentRun.IsTimingModeEnabled = !CurrentRun.IsTimingModeEnabled;
            SetMessage(CurrentRun.IsTimingModeEnabled ? "Timing enabled." : "Timing disabled.");
        }

        private void ResolvePlayerAttack(ActorEntity enemy, WeaponAttackContext attackContext)
        {
            int damage = combatSystem.ResolvePlayerAttack(CurrentRun, CurrentRun.Player, enemy, attackContext);
            string resultPrefix = FormatTimingResultPrefix(attackContext);
            SetMessage($"{resultPrefix}You strike {enemy.DisplayName} for {damage} damage.");
            if (!enemy.IsAlive)
            {
                CurrentRun.Gold += enemy.BloodReward;
                SetMessage($"{enemy.DisplayName} falls. You gain {enemy.BloodReward} blood shards.");
            }

            ConsumeTurn();
        }

        private bool TryGetPlayerAttackTarget(Vector2Int direction, out ActorEntity enemy, out WeaponAttackContext attackContext)
        {
            enemy = null;
            attackContext = null;
            Vector2Int facing = EnemyAttackPatternDefinition.NormalizeFacing(direction);
            WeaponDefinition weapon = CurrentRun.EquippedWeapon;
            if (weapon == null || weapon.AttackOffsets == null || weapon.AttackOffsets.Count == 0)
            {
                if (!actorRepository.TryGetEnemyAt(CurrentRun.Player.GridPosition + facing, out enemy))
                {
                    return false;
                }

                attackContext = BuildWeaponAttackContext(enemy, null, facing, Vector2Int.right);
                return true;
            }

            for (int i = 0; i < weapon.AttackOffsets.Count; i++)
            {
                Vector2Int weaponOffset = weapon.AttackOffsets[i];
                if (weaponOffset == Vector2Int.zero)
                {
                    continue;
                }

                Vector2Int targetOffset = weaponOffset;
                if (weapon.RotateAttackByFacing)
                {
                    targetOffset = EnemyAttackPatternDefinition.RotateOffset(weaponOffset, facing);
                }

                if (actorRepository.TryGetEnemyAt(CurrentRun.Player.GridPosition + targetOffset, out enemy))
                {
                    attackContext = BuildWeaponAttackContext(enemy, weapon, facing, weaponOffset);
                    return true;
                }
            }

            return false;
        }

        private WeaponAttackContext BuildWeaponAttackContext(
            ActorEntity defender,
            WeaponDefinition weapon,
            Vector2Int facing,
            Vector2Int weaponOffset)
        {
            return new WeaponAttackContext
            {
                RunState = CurrentRun,
                Attacker = CurrentRun.Player,
                Defender = defender,
                Weapon = weapon,
                FacingDirection = facing,
                WeaponOffset = weaponOffset,
                AttackPower = CurrentRun.Player.Stats.AttackPower,
                TimingResult = TimingAttackResult.None,
            };
        }

        public bool UseBandage()
        {
            if (CurrentRun == null)
            {
                return false;
            }

            if (CurrentRun.Player == null)
            {
                return false;
            }

            if (CurrentRun.BandageCount <= 0)
            {
                SetMessage("No bandages remain.");
                return false;
            }

            if (CurrentRun.Player.CurrentHp >= CurrentRun.Player.Stats.MaxHp)
            {
                SetMessage("You are already at full health.");
                return false;
            }

            CurrentRun.BandageCount -= 1;
            CurrentRun.Player.CurrentHp = Mathf.Min(CurrentRun.Player.Stats.MaxHp, CurrentRun.Player.CurrentHp + 4);
            SetMessage("You bind your wounds.");
            ConsumeTurn();
            return true;
        }

        public void Wait()
        {
            if (CurrentRun == null)
            {
                return;
            }

            SetMessage("You wait and listen.");
            ConsumeTurn();
        }

        public void EndRun(RunEndReason reason)
        {
            if (CurrentRun == null)
            {
                return;
            }

            CurrentRun.EndReason = reason;
        }

        private bool TryHandleRunInteractionAt(Vector2Int targetCell)
        {
            InteractionResolution resolution = interactionSystem.ResolveRunInteractionAt(
                targetCell,
                CurrentRun.Player,
                CurrentRun,
                mapService.CurrentMap);
            if (!resolution.Handled)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(resolution.Message))
            {
                SetMessage(resolution.Message);
            }

            if (resolution.EndReason != RunEndReason.None)
            {
                EndRun(resolution.EndReason);
            }

            if (resolution.ConsumesTurn)
            {
                ConsumeTurn();
            }

            return true;
        }

        private void ConsumeTurn()
        {
            turnSystem.ConsumePlayerAction(CurrentRun);
            enemyTurnSystem.ResolveEnemyTurn(CurrentRun, actorRepository.GetAliveEnemies(), mapService, actorRepository);
            CurrentRun.Player.CurrentHp = CurrentRun.Player.CurrentHp;
            if (CurrentRun.Player.CurrentHp <= 0 && CurrentRun.EndReason == RunEndReason.None)
            {
                EndRun(RunEndReason.Death);
                SetMessage("Death is not the end, only the start of reckoning.");
            }
        }

        private bool TryAutoPickupAtPlayerPosition()
        {
            if (mapService.TryGetWeaponSpawn(CurrentRun.Player.GridPosition, out WeaponSpawnDefinition weaponSpawn))
            {
                CurrentRun.HasEquippedWeapon = true;
                CurrentRun.EquippedWeapon = weaponSpawn.Weapon;
                CurrentRun.Player.Stats.AttackPower = CurrentRun.EffectiveAttack;
                SetMessage(BuildWeaponPickupMessage(weaponSpawn.Weapon));
                return true;
            }

            return false;
        }

        private static string BuildWeaponPickupMessage(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return "You pick up a weapon.";
            }

            return $"You pick up {weapon.DisplayName}. Attack rises by {weapon.AttackBonus} for this run.";
        }

        private void SetMessage(string message)
        {
            LastMessage = message;
        }

        private static string FormatTimingResultPrefix(WeaponAttackContext attackContext)
        {
            if (attackContext == null || !attackContext.TimingResult.Attempted)
            {
                return string.Empty;
            }

            switch (attackContext.TimingResult.Grade)
            {
                case TimingResultGrade.Perfect:
                    return "Perfect timing. ";
                case TimingResultGrade.Good:
                    return "Good timing. ";
                case TimingResultGrade.Failed:
                    return "Mistimed. ";
                default:
                    return string.Empty;
            }
        }
    }
}

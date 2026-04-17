using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Combat;
using Arkeum.Production.Gameplay.Interaction;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
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

        public RunState CurrentRun { get; private set; }
        public string LastMessage { get; private set; } = string.Empty;

        public RunController(
            TurnSystem turnSystem,
            CombatSystem combatSystem,
            EnemyTurnSystem enemyTurnSystem,
            InteractionSystem interactionSystem,
            MapService mapService,
            ActorRepository actorRepository)
        {
            this.turnSystem = turnSystem;
            this.combatSystem = combatSystem;
            this.enemyTurnSystem = enemyTurnSystem;
            this.interactionSystem = interactionSystem;
            this.mapService = mapService;
            this.actorRepository = actorRepository;
        }

        public void Begin(RunState runState)
        {
            CurrentRun = runState;
        }

        public RunState CreateRunState(SaveProfile profile)
        {
            int startingBandage = profile != null && profile.StartingBandageUnlocked ? 1 : 0;
            return new RunState
            {
                RunIndex = (profile?.TotalReturns ?? 0) + 1,
                TurnCount = 0,
                DepthReached = 1,
                BloodShards = 0,
                BandageCount = startingBandage,
                DraughtCount = 0,
                AttackBonus = 0,
                TemporaryWeaponEquipped = false,
                ReliquaryClaimed = false,
                TemporaryWeaponCollected = false,
                DraughtStock = 2,
                EndReason = RunEndReason.None,
            };
        }

        public bool TryHandlePlayerAction(Vector2Int direction)
        {
            if (CurrentRun?.Player == null)
            {
                return false;
            }

            Vector2Int targetCell = CurrentRun.Player.GridPosition + direction;
            if (actorRepository.TryGetEnemyAt(targetCell, out ActorEntity enemy))
            {
                int damage = combatSystem.ResolvePlayerAttack(CurrentRun, CurrentRun.Player, enemy);
                SetMessage($"You strike {enemy.DisplayName} for {damage} damage.");
                if (!enemy.IsAlive)
                {
                    CurrentRun.BloodShards += enemy.BloodReward;
                    SetMessage($"{enemy.DisplayName} falls. You gain {enemy.BloodReward} blood shards.");
                }

                ConsumeTurn();
                return true;
            }

            if (TryHandleRunInteractionAt(targetCell))
            {
                return true;
            }

            if (!mapService.IsWalkable(targetCell))
            {
                SetMessage("The path is blocked.");
                return false;
            }

            CurrentRun.Player.GridPosition = targetCell;
            CurrentRun.DepthReached = Mathf.Max(CurrentRun.DepthReached, mapService.GetDepth(targetCell));
            TryAutoPickupAtPlayerPosition();
            SetMessage($"You advance into {GetDepthName(CurrentRun.DepthReached)}.");
            ConsumeTurn();
            return true;
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

            if (CurrentRun.Player.CurrentHp >= CurrentRun.Player.MaxHp)
            {
                SetMessage("You are already at full health.");
                return false;
            }

            CurrentRun.BandageCount -= 1;
            CurrentRun.Player.CurrentHp = Mathf.Min(CurrentRun.Player.MaxHp, CurrentRun.Player.CurrentHp + 4);
            SetMessage("You bind your wounds.");
            ConsumeTurn();
            return true;
        }

        public bool UseDraught()
        {
            if (CurrentRun == null)
            {
                return false;
            }

            if (CurrentRun.Player == null)
            {
                return false;
            }

            if (CurrentRun.DraughtCount <= 0)
            {
                SetMessage("No draughts remain.");
                return false;
            }

            if (CurrentRun.Player.CurrentHp >= CurrentRun.Player.MaxHp)
            {
                SetMessage("You are already at full health.");
                return false;
            }

            CurrentRun.DraughtCount -= 1;
            CurrentRun.Player.CurrentHp = Mathf.Min(CurrentRun.Player.MaxHp, CurrentRun.Player.CurrentHp + 6);
            SetMessage("The draught forces your wounds closed.");
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
            if (interactionSystem.TryGetInteractableAt(targetCell, out IInteractable interactable))
            {
                // TODO :: InteractionResolver로 이동해야 함
                switch (interactable.InteractableType)
                {
                    case InteractableType.Merchant:
                        return TryBuyDraught();
                    case InteractableType.Reliquary:
                        if (!CurrentRun.ReliquaryClaimed)
                        {
                            CurrentRun.ReliquaryClaimed = true;
                            SetMessage("You recover the reacting reliquary light.");
                            EndRun(RunEndReason.DepthClear);
                            return true;
                        }

                        return false;
                    default:
                        return interactionSystem.TryInteract(targetCell, CurrentRun.Player);
                }
            }

            if (mapService.CurrentMap != null && targetCell == mapService.CurrentMap.MerchantPosition)
            {
                return TryBuyDraught();
            }

            if (mapService.CurrentMap != null && targetCell == mapService.CurrentMap.ReliquaryPosition && !CurrentRun.ReliquaryClaimed)
            {
                CurrentRun.ReliquaryClaimed = true;
                SetMessage("You recover the reacting reliquary light.");
                EndRun(RunEndReason.DepthClear);
                return true;
            }

            return false;
        }

        private bool TryBuyDraught()
        {
            if (CurrentRun.BloodShards < 3)
            {
                SetMessage("You do not have enough blood shards.");
                return false;
            }

            if (CurrentRun.DraughtStock <= 0)
            {
                SetMessage("The merchant has nothing left to sell.");
                return false;
            }

            CurrentRun.BloodShards -= 3;
            CurrentRun.DraughtCount += 1;
            CurrentRun.DraughtStock -= 1;
            SetMessage("You buy a healing draught for this run.");
            ConsumeTurn();
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

        private void TryAutoPickupAtPlayerPosition()
        {
            if (CurrentRun.TemporaryWeaponCollected)
            {
                return;
            }

            if (mapService.IsTemporaryWeaponSpawn(CurrentRun.Player.GridPosition))
            {
                CurrentRun.TemporaryWeaponCollected = true;
                CurrentRun.TemporaryWeaponEquipped = true;
                CurrentRun.AttackBonus = 1;
                CurrentRun.Player.Stats.AttackPower = CurrentRun.EffectiveAttack;
                SetMessage("You pick up a worn blade. Attack rises by 1 for this run.");
            }
        }

        private string GetDepthName(int depth)
        {
            return depth <= 1 ? "Outer Corridor" : "Deep Corridor";
        }

        private void SetMessage(string message)
        {
            LastMessage = message;
        }
    }
}

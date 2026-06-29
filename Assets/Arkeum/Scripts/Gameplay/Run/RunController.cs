using System.Collections.Generic;
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
        private readonly SaveProfile activeProfile;
        
        public event System.Action WeaponPickedUp;

        public RunState CurrentRun { get; private set; }
        public string LastMessage { get; private set; } = string.Empty;
        public RunActionFeedback LastActionFeedback { get; private set; }

        public RunController(
            TurnSystem turnSystem,
            CombatSystem combatSystem,
            EnemyTurnSystem enemyTurnSystem,
            InteractionSystem interactionSystem,
            MapService mapService,
            ActorRepository actorRepository,
            TimingService timingService,
            SaveProfile activeProfile)
        {
            this.turnSystem = turnSystem;
            this.combatSystem = combatSystem;
            this.enemyTurnSystem = enemyTurnSystem;
            this.interactionSystem = interactionSystem;
            this.mapService = mapService;
            this.actorRepository = actorRepository;
            this.timingService = timingService;
            this.activeProfile = activeProfile;
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
                FloorExitUsed = false,
                BossRoomEntered = false,
                BossRoomCleared = false,
                HasEquippedWeapon = startingLoadout?.Weapon != null,
                EquippedWeapon = startingLoadout != null ? startingLoadout.Weapon : null,
                IsTimingModeEnabled = false,
                EndReason = RunEndReason.None,
            };
        }

        public PlayerActionResultType TryHandlePlayerAction(Vector2Int direction)
        {
            LastActionFeedback = RunActionFeedback.None;
            if (CurrentRun?.Player == null)
            {
                return PlayerActionResultType.NotHandled;
            }

            // 상호작용할 Cell 선정
            Vector2Int targetCell = CurrentRun.Player.GridPosition + direction;

            // 해당 셀을 기준으로 공격이 가능하다면
            if (TryGetPlayerAttackTargets(direction, out List<WeaponAttackContext> attackContexts))
            {
                WeaponAttackContext attackContext = attackContexts[0];
                CurrentRun.Player.FacingDirection = attackContext.FacingDirection;

                // 타이밍 활성화 되어있는지 확인
                if (timingService != null && timingService.TryBegin(CurrentRun, attackContexts, out TimingSession _))
                {
                    SetMessage("Time your strike.");
                    return PlayerActionResultType.TimingChallengeStarted;
                }

                // 일반 공격
                ResolvePlayerAttacks(attackContexts);
                return PlayerActionResultType.Handled;
            }

            // 해당 셀에 인터렉션이 가능한 물체가 있다면
            if (TryHandleRunInteractionAt(targetCell))
            {
                return PlayerActionResultType.Handled;
            }

            // 해당 셀에 진열대가 있다면
            if (TryHandleShopOfferAt(targetCell))
            {
                return PlayerActionResultType.Handled;
            }

            // 해당 셀이 이동 불가능한 셀이라면
            if (!mapService.IsWalkable(targetCell))
            {
                SetMessage("The path is blocked.");
                return PlayerActionResultType.NotHandled;
            }

            // 해당 셀으로 이동
            CurrentRun.Player.GridPosition = targetCell;
            LastActionFeedback |= RunActionFeedback.PlayerMoved;

            bool teleportedByShopMarker = TryTeleportByShopMarker();
            bool sealedBossRoom = !teleportedByShopMarker && TrySealBossRoomIfNeeded(); // 보스룸 입장인지 확인
            if (!sealedBossRoom &&
                !teleportedByShopMarker &&
                !TryAutoPickupAtPlayerPosition() && // 자리에 있는 아이템 픽업
                !TryDescribeAdjacentShopOffer()) // 근처에 있는 진열대 정보 표시
            {
                SetMessage(string.Empty);
            }

            ConsumeTurn();
            return PlayerActionResultType.Handled;
        }

        private bool TryTeleportByShopMarker()
        {
            MapDefinition map = mapService.CurrentMap;
            if (map == null || CurrentRun?.Player == null)
            {
                return false;
            }

            if (IsMarkerEnabled(map.ShopEntrancePosition) &&
                IsMarkerEnabled(map.ShopExitPosition) &&
                CurrentRun.Player.GridPosition == map.ShopEntrancePosition)
            {
                CurrentRun.Player.GridPosition = IsMarkerEnabled(map.ShopInteriorSpawnPosition)
                    ? map.ShopInteriorSpawnPosition
                    : map.ShopExitPosition;
                SetMessage("You step through the shop marker.");
                LastActionFeedback |= RunActionFeedback.PlayerTeleported;
                return true;
            }

            if (IsMarkerEnabled(map.ShopEntrancePosition) &&
                IsMarkerEnabled(map.ShopExitPosition) &&
                CurrentRun.Player.GridPosition == map.ShopExitPosition)
            {
                CurrentRun.Player.GridPosition = map.ShopEntrancePosition;
                SetMessage("You return to the dungeon.");
                LastActionFeedback |= RunActionFeedback.PlayerTeleported;
                return true;
            }

            return false;
        }

        public bool ResolveTimedAttack(TimingAttackResult timingResult)
        {
            LastActionFeedback = RunActionFeedback.None;
            TimingSession session = timingService?.CurrentSession;
            IReadOnlyList<WeaponAttackContext> attackContexts = session?.AttackContexts;
            if (attackContexts == null || attackContexts.Count == 0 || CurrentRun?.Player == null)
            {
                timingService?.CancelCurrent();
                return false;
            }

            timingService.CompleteCurrent(timingResult.Grade);
            for (int i = 0; i < attackContexts.Count; i++)
            {
                WeaponAttackContext attackContext = attackContexts[i];
                if (attackContext != null)
                {
                    attackContext.TimingResult = timingResult;
                }
            }

            ResolvePlayerAttacks(attackContexts);
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

        private void ResolvePlayerAttacks(IReadOnlyList<WeaponAttackContext> attackContexts)
        {
            if (attackContexts == null || attackContexts.Count == 0)
            {
                return;
            }

            LastActionFeedback |= RunActionFeedback.PlayerAttacked;
            for (int i = 0; i < attackContexts.Count; i++)
            {
                WeaponAttackContext attackContext = attackContexts[i];
                ActorEntity enemy = attackContext?.Defender;
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                // 데미지 계산
                int damage = combatSystem.ResolvePlayerAttack(CurrentRun, CurrentRun.Player, enemy, attackContext);

                // 공격한 적이 죽었을 때
                if (!enemy.IsAlive)
                {
                    activeProfile?.AddGold(enemy.Gold);
                }
            }

            // 보스방일 때 모든 적이 죽었다면 문을 개방 
            TryClearBossRoomIfNeeded();
            ConsumeTurn();
        }

        // 플레이어 공격 범위 안에 있는 모든 대상을 수집하는 함수
        private bool TryGetPlayerAttackTargets(Vector2Int direction, out List<WeaponAttackContext> attackContexts)
        {
            attackContexts = new List<WeaponAttackContext>();
            Vector2Int facing = EnemyAttackPatternDefinition.NormalizeFacing(direction);
            WeaponDefinition weapon = CurrentRun.EquippedWeapon;
            
            // 무기가 없을 때는 전방 한칸 공격
            if (weapon == null || weapon.AttackOffsets == null || weapon.AttackOffsets.Count == 0)
            {
                Vector2Int targetCell = CurrentRun.Player.GridPosition + facing;
                if (mapService.BlocksAttackBetween(CurrentRun.Player.GridPosition, targetCell) ||
                    !actorRepository.TryGetEnemyAt(targetCell, out ActorEntity enemy))
                {
                    return false;
                }

                attackContexts.Add(BuildWeaponAttackContext(enemy, null, facing, Vector2Int.right));
                return true;
            }

            // 무기가 있을 때는 무기 범위 조사
            for (int i = 0; i < weapon.AttackOffsets.Count; i++)
            {
                Vector2Int weaponOffset = weapon.AttackOffsets[i];
                if (weaponOffset == Vector2Int.zero)
                {
                    continue;
                }

                // 바라보는 방향 기준으로 회전
                Vector2Int targetOffset = weaponOffset;
                if (weapon.RotateAttackByFacing)
                {
                    targetOffset = EnemyAttackPatternDefinition.RotateOffset(weaponOffset, facing);
                }

                // 공격범위 사이에 가로막는 벽이 있는지 확인 (정규 격자만 확인)
                Vector2Int targetCell = CurrentRun.Player.GridPosition + targetOffset;
                if (mapService.BlocksAttackBetween(CurrentRun.Player.GridPosition, targetCell))
                {
                    continue;
                }

                // 적이 있는지 확인 및 이미 공격중인 적인지 중복검사
                if (actorRepository.TryGetEnemyAt(targetCell, out ActorEntity enemy) &&
                    !ContainsDefender(attackContexts, enemy))
                {
                    // 성공시 attackContext 생성
                    attackContexts.Add(BuildWeaponAttackContext(enemy, weapon, facing, weaponOffset));
                }
            }

            return attackContexts.Count > 0;
        }

        private static bool ContainsDefender(IReadOnlyList<WeaponAttackContext> attackContexts, ActorEntity defender)
        {
            if (attackContexts == null || defender == null)
            {
                return false;
            }

            for (int i = 0; i < attackContexts.Count; i++)
            {
                if (attackContexts[i]?.Defender == defender)
                {
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
                AttackPower = RunStatCalculator.CalculatePlayerAttack(CurrentRun),
                TimingResult = TimingAttackResult.None,
            };
        }

        public void Wait()
        {
            if (CurrentRun == null)
            {
                return;
            }

            if (!TryDescribeAdjacentShopOffer())
            {
                SetMessage("You wait and listen.");
            }

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

        private bool TryHandleShopOfferAt(Vector2Int targetCell)
        {
            if (!mapService.TryGetShopOffer(targetCell, out ShopOfferDefinition shopOffer))
            {
                return false;
            }

            if (shopOffer.Weapon == null)
            {
                SetMessage("This shelf is empty.");
                return true;
            }

            int price = Mathf.Max(0, shopOffer.Price);
            int currentGold = activeProfile != null ? activeProfile.Gold : 0;
            if (activeProfile == null || currentGold < price)
            {
                SetMessage($"Need {price} gold for {shopOffer.DisplayName}. {shopOffer.Summary}.");
                return true;
            }

            if (!mapService.TryBuyShopOffer(targetCell, out ShopOfferDefinition purchasedOffer))
            {
                SetMessage("The shelf is empty.");
                return true;
            }

            bool hadEquippedWeapon = CurrentRun.HasEquippedWeapon;
            WeaponDefinition droppedWeapon = CurrentRun.EquippedWeapon;
            activeProfile.SetGold(currentGold - price);
            CurrentRun.HasEquippedWeapon = true;
            CurrentRun.EquippedWeapon = purchasedOffer.Weapon;

            if (hadEquippedWeapon)
            {
                mapService.DropWeapon(CurrentRun.Player.GridPosition, droppedWeapon);
            }

            WeaponPickedUp?.Invoke();
            SetMessage(BuildShopPurchaseMessage(purchasedOffer, price, hadEquippedWeapon, droppedWeapon));
            ConsumeTurn();
            return true;
        }

        private bool TryDescribeAdjacentShopOffer()
        {
            if (CurrentRun?.Player == null)
            {
                return false;
            }

            Vector2Int playerCell = CurrentRun.Player.GridPosition;
            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
            };

            for (int i = 0; i < directions.Length; i++)
            {
                if (!mapService.TryGetShopOffer(playerCell + directions[i], out ShopOfferDefinition shopOffer))
                {
                    continue;
                }

                SetMessage($"{shopOffer.DisplayName}: {shopOffer.Price} gold. {shopOffer.Summary}.");
                return true;
            }

            return false;
        }

        private void ConsumeTurn()
        {
            turnSystem.ConsumePlayerAction(CurrentRun);
            enemyTurnSystem.ResolveEnemyTurn(CurrentRun, actorRepository.GetAliveEnemies(), mapService, actorRepository);
            if (CurrentRun.Player.CurrentHp <= 0 && CurrentRun.EndReason == RunEndReason.None)
            {
                EndRun(RunEndReason.Death);
                SetMessage("Death is not the end, only the start of reckoning.");
                return;
            }

            TryClearBossRoomIfNeeded();
        }

        private bool TrySealBossRoomIfNeeded()
        {
            if (CurrentRun == null ||
                CurrentRun.BossRoomEntered ||
                CurrentRun.BossRoomCleared ||
                CurrentRun.Player == null ||
                mapService.CurrentMap == null ||
                !IsBossRoomCell(CurrentRun.Player.GridPosition))
            {
                return false;
            }

            bool sealedAny = SetBossEntranceWalls(true);
            CurrentRun.BossRoomEntered = true;
            if (sealedAny)
            {
                SetMessage("The entrance seals behind you.");
            }

            return sealedAny;
        }

        private bool TryClearBossRoomIfNeeded()
        {
            if (CurrentRun == null ||
                !CurrentRun.BossRoomEntered ||
                CurrentRun.BossRoomCleared ||
                mapService.CurrentMap == null)
            {
                return false;
            }

            IReadOnlyList<ActorEntity> aliveEnemies = actorRepository.GetAliveEnemies();
            if (aliveEnemies.Count > 0)
            {
                return false;
            }

            CurrentRun.BossRoomCleared = true;
            bool openedAny = SetBossEntranceWalls(false);
            if (openedAny)
            {
                SetMessage("All monsters fall. The sealed entrance opens.");
            }

            return openedAny;
        }

        private bool SetBossEntranceWalls(bool hasWall)
        {
            List<Vector2Int> blockCells = mapService.CurrentMap.BossEntranceBlockCells;
            bool changedAny = false;
            for (int i = 0; i < blockCells.Count; i++)
            {
                changedAny |= mapService.SetRuntimeWall(blockCells[i], hasWall);
            }

            return changedAny;
        }

        private bool IsBossRoomCell(Vector2Int cell)
        {
            MapDefinition map = mapService.CurrentMap;
            if (map == null || map.BossRoomId < 0)
            {
                return false;
            }

            for (int i = 0; i < map.Rooms.Count; i++)
            {
                DungeonRoomDefinition room = map.Rooms[i];
                if (room == null || room.Id != map.BossRoomId)
                {
                    continue;
                }

                return room.Cells.Contains(cell);
            }

            return false;
        }

        private bool TryAutoPickupAtPlayerPosition()
        {
            bool hadEquippedWeapon = CurrentRun.HasEquippedWeapon;
            WeaponDefinition droppedWeapon = CurrentRun.EquippedWeapon;
            if (mapService.TryPickupWeaponAt(
                    CurrentRun.Player.GridPosition,
                    hadEquippedWeapon,
                    droppedWeapon,
                    out WeaponSpawnDefinition weaponSpawn))
            {
                CurrentRun.HasEquippedWeapon = true;
                CurrentRun.EquippedWeapon = weaponSpawn.Weapon;
                    
                WeaponPickedUp?.Invoke();
                
                SetMessage(BuildWeaponPickupMessage(weaponSpawn.Weapon, hadEquippedWeapon, droppedWeapon));
                return true;
            }

            return false;
        }

        private static string BuildWeaponPickupMessage(
            WeaponDefinition weapon,
            bool droppedPreviousWeapon,
            WeaponDefinition droppedWeapon)
        {
            string dropSuffix = droppedPreviousWeapon ? $" You drop {GetWeaponDisplayName(droppedWeapon)}." : string.Empty;
            if (weapon == null)
            {
                return $"You pick up a weapon.{dropSuffix}";
            }

            return $"You pick up {weapon.DisplayName}. Attack rises by {weapon.AttackBonus} for this run.{dropSuffix}";
        }

        private static string BuildShopPurchaseMessage(
            ShopOfferDefinition shopOffer,
            int price,
            bool droppedPreviousWeapon,
            WeaponDefinition droppedWeapon)
        {
            string dropSuffix = droppedPreviousWeapon ? $" You drop {GetWeaponDisplayName(droppedWeapon)}." : string.Empty;
            return $"You buy {shopOffer.DisplayName} for {price} gold. {shopOffer.Summary}.{dropSuffix}";
        }

        private static string GetWeaponDisplayName(WeaponDefinition weapon)
        {
            return weapon != null ? weapon.DisplayName : "a weapon";
        }

        private static bool IsMarkerEnabled(Vector2Int position)
        {
            return position != Vector2Int.zero;
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
                case TimingResultGrade.Success:
                    return "Timing success. ";
                case TimingResultGrade.Failed:
                    return "Mistimed. ";
                default:
                    return string.Empty;
            }
        }
    }
}

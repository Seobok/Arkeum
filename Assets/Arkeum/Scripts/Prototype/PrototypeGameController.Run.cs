using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Prototype
{
    public sealed partial class PrototypeGameController
    {
        private bool TryHandleDirectionalActionInRun(Keyboard keyboard)
        {
            if (!inputReader.TryGetDirectionalInput(keyboard, out Vector2Int direction))
            {
                return false;
            }

            Vector2Int target = player.Position + direction;
            if (TryGetEnemyAt(target, out ActorRuntime enemy))
            {
                AttackEnemy(enemy);
                return true;
            }

            if (TryHandleRunInteractionAt(target))
            {
                return true;
            }

            if (!layout.IsWalkable(target))
            {
                SetMessage("The path is blocked.");
                return true;
            }

            player.Position = target;
            SyncActorView(player);
            Run.DepthReached = Mathf.Max(Run.DepthReached, layout.GetDepth(target));
            TryAutoPickupAtPlayerPosition();
            SetMessage($"You advance into {GetDepthName(Run.DepthReached)}.");
            ConsumeTurn(null);
            return true;
        }

        private void StartRun()
        {
            ClearSpawnedViews();
            layout = BuildLayout();
            DrawLayout(layout);
            SpawnActors();
            reliquaryClaimed = false;
            temporaryWeaponCollected = false;
            draughtStock = 2;

            int startingBandage = Profile.unlockedStartingBandage ? 1 : 0;
            Run = new RunState
            {
                RunIndex = Profile.totalReturns + 1,
                TurnCount = 0,
                DepthReached = 1,
                Hyeolpyeon = 0,
                CurrentHp = 12,
                MaxHp = 12,
                BandageCount = startingBandage,
                DraughtCount = 0,
                AttackBonus = 0,
                TemporaryWeaponEquipped = false,
                EndReason = RunEndReason.None,
            };

            player.Hp = Run.CurrentHp;
            player.MaxHp = Run.MaxHp;
            Phase = GamePhase.InRun;
            hud.DialogueLine = string.Empty;
            SetMessage("You descend into the ash corridor. Enemies react after every action.");
            UpdateCamera();
        }

        private void EndRun(RunEndReason reason)
        {
            Run.EndReason = reason;
            int jangwangGain = progressionService.ApplyRunEnd(Profile, Run);
            saveService.SaveProfile(Profile);
            progressionService.BuildResultLines(Profile, Run, jangwangGain, lostResultLines, keptResultLines);

            Phase = GamePhase.RunResult;
            SetMessage(reason == RunEndReason.Death
                ? "Death is not the end, only the start of reckoning."
                : "The recovered light returns to the altar.");
        }

        private bool TryHandleRunInteractionAt(Vector2Int target)
        {
            if (target == layout.MerchantPosition)
            {
                TryBuyDraught();
                return true;
            }

            if (target == layout.ReliquaryPosition && !reliquaryClaimed)
            {
                reliquaryClaimed = true;
                SetMessage("You recover the reacting reliquary light.");
                EndRun(RunEndReason.DepthClear);
                return true;
            }

            return false;
        }

        private void TryBuyDraught()
        {
            if (Run.Hyeolpyeon < 3)
            {
                SetMessage("You do not have enough blood shards.");
                return;
            }

            if (draughtStock <= 0)
            {
                SetMessage("The merchant has nothing left to sell.");
                return;
            }

            Run.Hyeolpyeon -= 3;
            Run.DraughtCount += 1;
            draughtStock -= 1;
            SetMessage("You buy a healing draught for this run.");
            ConsumeTurn(null);
        }

        private void UseBandage()
        {
            if (Run.BandageCount <= 0)
            {
                SetMessage("No bandages remain.");
                return;
            }

            if (Run.CurrentHp >= Run.MaxHp)
            {
                SetMessage("You are already at full health.");
                return;
            }

            Run.BandageCount -= 1;
            Run.CurrentHp = Mathf.Min(Run.MaxHp, Run.CurrentHp + 4);
            player.Hp = Run.CurrentHp;
            SetMessage("You bind your wounds.");
            ConsumeTurn(null);
        }

        private void UseDraught()
        {
            if (Run.DraughtCount <= 0)
            {
                SetMessage("No draughts remain.");
                return;
            }

            if (Run.CurrentHp >= Run.MaxHp)
            {
                SetMessage("You are already at full health.");
                return;
            }

            Run.DraughtCount -= 1;
            Run.CurrentHp = Mathf.Min(Run.MaxHp, Run.CurrentHp + 6);
            player.Hp = Run.CurrentHp;
            SetMessage("The draught forces your wounds closed.");
            ConsumeTurn(null);
        }

        private void ConsumeTurn(string overrideMessage)
        {
            Run.TurnCount += 1;
            UpdateEnemies();
            if (!string.IsNullOrEmpty(overrideMessage))
            {
                SetMessage(overrideMessage);
            }

            if (Run.CurrentHp <= 0 && Phase == GamePhase.InRun)
            {
                EndRun(RunEndReason.Death);
                return;
            }

            UpdateCamera();
        }

        private void TryAutoPickupAtPlayerPosition()
        {
            for (int i = 0; i < layout.TemporaryWeaponSpawns.Count; i++)
            {
                if (player.Position == layout.TemporaryWeaponSpawns[i] && !temporaryWeaponCollected)
                {
                    temporaryWeaponCollected = true;
                    Run.TemporaryWeaponEquipped = true;
                    Run.AttackBonus = 1;
                    SetMessage("You pick up a worn blade. Attack rises by 1 for this run.");
                    return;
                }
            }
        }
    }
}

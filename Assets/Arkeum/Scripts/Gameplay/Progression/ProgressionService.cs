using Arkeum.Production.Gameplay.Run;
using System.Collections.Generic;

namespace Arkeum.Production.Gameplay.Progression
{
    public sealed class ProgressionService
    {
        private readonly QuestService questService;

        public ProgressionService(QuestService questService)
        {
            this.questService = questService;
        }

        // 클리어시 호출 / 클리어 데이터 관련 처리
        public void ApplyRunEnd(SaveProfile profile, RunState runState)
        {
            if (profile == null || runState == null)
            {
                return;
            }

            profile.TotalReturns += 1;
            if (profile.HighestFloor < runState.CurrentFloor)
            {
                profile.HighestFloor = runState.CurrentFloor;
            }
        }

        public void MarkRunClear(SaveProfile profile)
        {
            questService.MarkPrototypeClear(profile);
        }

        public void BuildResultLines(SaveProfile profile, RunState runState, List<string> lostLines, List<string> keptLines)
        {
            lostLines.Clear();
            keptLines.Clear();

            lostLines.Add(FormatWeaponLoss(runState));

            keptLines.Add($"Total gold: {profile.Gold}");
            keptLines.Add($"Total returns: {profile.TotalReturns}");
            keptLines.Add($"Highest floor reached: {profile.HighestFloor}");
        }

        private static string FormatWeaponLoss(RunState runState)
        {
            if (runState == null || !runState.HasEquippedWeapon)
            {
                return "No weapon was equipped.";
            }

            if (runState.EquippedWeapon == null)
            {
                return "Weapon lost at the end of the run.";
            }

            return $"{runState.EquippedWeapon.DisplayName} lost at the end of the run.";
        }
    }
}

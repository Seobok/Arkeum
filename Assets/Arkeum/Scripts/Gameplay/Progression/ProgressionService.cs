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

        public int ApplyRunEnd(SaveProfile profile, RunState runState)
        {
            if (profile == null || runState == null)
            {
                return 0;
            }

            profile.TotalReturns += 1;
            if (profile.HighestFloor < runState.CurrentFloor)
            {
                profile.HighestFloor = runState.CurrentFloor;
            }

            int gleamGain = runState.EndReason == RunEndReason.FloorClear ? 2 : 1;
            runState.GleamReward = gleamGain;
            profile.Shard += gleamGain;
            if (runState.EndReason == RunEndReason.FloorClear)
            {
                questService.MarkPrototypeClear(profile);
            }

            return gleamGain;
        }

        public void MarkRunClear(SaveProfile profile)
        {
            questService.MarkPrototypeClear(profile);
        }

        public void BuildResultLines(SaveProfile profile, RunState runState, List<string> lostLines, List<string> keptLines)
        {
            lostLines.Clear();
            keptLines.Clear();

            lostLines.Add($"Blood shards lost: {runState.Gold}");
            lostLines.Add(FormatWeaponLoss(runState));

            keptLines.Add($"Shard gained: +{runState.GleamReward}");
            keptLines.Add($"Total gleam: {profile.Shard}");
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

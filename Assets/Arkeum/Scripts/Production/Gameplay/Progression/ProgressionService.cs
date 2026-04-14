using Arkeum.Production.Gameplay.Run;
using System.Collections.Generic;

namespace Arkeum.Production.Gameplay.Progression
{
    public sealed class ProgressionService
    {
        private readonly UnlockService unlockService;
        private readonly QuestService questService;

        public ProgressionService(UnlockService unlockService, QuestService questService)
        {
            this.unlockService = unlockService;
            this.questService = questService;
        }

        public int ApplyRunEnd(SaveProfile profile, RunState runState)
        {
            if (profile == null || runState == null)
            {
                return 0;
            }

            profile.TotalReturns += 1;
            if (profile.HighestDepth < runState.DepthReached)
            {
                profile.HighestDepth = runState.DepthReached;
            }

            int gleamGain = runState.EndReason == RunEndReason.DepthClear ? 2 : 1;
            if (runState.DepthReached >= 2)
            {
                gleamGain += 1;
            }

            runState.GleamReward = gleamGain;
            profile.Gleam += gleamGain;
            if (runState.EndReason == RunEndReason.DepthClear)
            {
                questService.MarkPrototypeClear(profile);
            }

            return gleamGain;
        }

        public bool TryUnlockStartingBandage(SaveProfile profile, int cost, out string message)
        {
            if (profile == null)
            {
                message = "Profile is not available.";
                return false;
            }

            if (profile.StartingBandageUnlocked)
            {
                message = "The starting bandage is already unlocked.";
                return false;
            }

            if (!unlockService.TryUnlockStartingBandage(profile, cost))
            {
                message = "You do not have enough gleam to unlock the starting bandage.";
                return false;
            }

            message = "Starting bandage unlocked. Future runs begin with 1 bandage.";
            return true;
        }

        public void MarkRunClear(SaveProfile profile)
        {
            questService.MarkPrototypeClear(profile);
        }

        public void BuildResultLines(SaveProfile profile, RunState runState, List<string> lostLines, List<string> keptLines)
        {
            lostLines.Clear();
            keptLines.Clear();

            lostLines.Add($"Blood shards lost: {runState.BloodShards}");
            lostLines.Add($"Draughts left behind: {runState.DraughtCount}");
            lostLines.Add(runState.TemporaryWeaponEquipped
                ? "Temporary weapon lost at the end of the run."
                : "No temporary weapon was equipped.");

            keptLines.Add($"Gleam gained: +{runState.GleamReward}");
            keptLines.Add($"Total gleam: {profile.Gleam}");
            keptLines.Add($"Total returns: {profile.TotalReturns}");
            keptLines.Add($"Deepest corridor reached: {profile.HighestDepth}");
            keptLines.Add(profile.StartingBandageUnlocked
                ? "Starting bandage unlock is active."
                : "Starting bandage unlock is still locked.");
        }

        public string GetUndertakerGreeting(SaveProfile profile)
        {
            if (profile == null || profile.TotalReturns == 0)
            {
                return "The undertaker watches in silence. Step into the corridor and return if you can.";
            }

            if (profile.TotalReturns < 3)
            {
                return "You have returned more than once. The altar remembers what the corridor takes.";
            }

            return "Many returns have hardened the silence. Even now, the undertaker keeps count.";
        }

        public void SeedDialogue(Queue<string> undertakerLines)
        {
            undertakerLines.Clear();
            undertakerLines.Enqueue("The undertaker says: The corridor never forgets a hurried step.");
            undertakerLines.Enqueue("The undertaker says: What you carry out matters less than what you learn to keep.");
            undertakerLines.Enqueue("The undertaker says: Return enough times, and even failure becomes a kind of map.");
        }
    }
}

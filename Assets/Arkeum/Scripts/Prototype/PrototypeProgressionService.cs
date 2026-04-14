using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed class PrototypeProgressionService
    {
        public int ApplyRunEnd(ProfileSaveData profile, RunState run)
        {
            profile.totalReturns += 1;
            profile.highestReachedDepth = Mathf.Max(profile.highestReachedDepth, run.DepthReached);

            int jangwangGain = run.EndReason == RunEndReason.DepthClear ? 2 : 1;
            if (run.DepthReached >= 2)
            {
                jangwangGain += 1;
            }

            profile.jangwang += jangwangGain;
            if (run.EndReason == RunEndReason.DepthClear && !profile.mq01Completed)
            {
                profile.mq01Completed = true;
                if (!profile.completedQuestIds.Contains("MQ-01"))
                {
                    profile.completedQuestIds.Add("MQ-01");
                }
            }

            return jangwangGain;
        }

        public void BuildResultLines(ProfileSaveData profile, RunState run, int jangwangGain, List<string> lostResultLines, List<string> keptResultLines)
        {
            lostResultLines.Clear();
            keptResultLines.Clear();

            lostResultLines.Add($"Blood shards lost: {run.Hyeolpyeon}");
            lostResultLines.Add($"Draughts left behind: {run.DraughtCount}");
            lostResultLines.Add(run.TemporaryWeaponEquipped
                ? "Temporary weapon lost at the end of the run."
                : "No temporary weapon was equipped.");

            keptResultLines.Add($"Gleam gained: +{jangwangGain}");
            keptResultLines.Add($"Total gleam: {profile.jangwang}");
            keptResultLines.Add($"Total returns: {profile.totalReturns}");
            keptResultLines.Add($"Deepest corridor reached: {profile.highestReachedDepth}");
            keptResultLines.Add(profile.unlockedStartingBandage
                ? "Starting bandage unlock is active."
                : "Starting bandage unlock is still locked.");
        }

        public bool TryUnlockStartingBandage(ProfileSaveData profile, PrototypeSaveService saveService, int unlockCost, out string message)
        {
            if (profile.unlockedStartingBandage)
            {
                message = "The starting bandage is already unlocked.";
                return false;
            }

            if (profile.jangwang < unlockCost)
            {
                message = "You do not have enough gleam to unlock the starting bandage.";
                return false;
            }

            profile.jangwang -= unlockCost;
            profile.unlockedStartingBandage = true;
            saveService.SaveProfile(profile);
            message = "Starting bandage unlocked. Future runs begin with 1 bandage.";
            return true;
        }

        public string GetUndertakerGreeting(ProfileSaveData profile)
        {
            if (profile.totalReturns == 0)
            {
                return "The undertaker watches in silence. Step into the corridor and return if you can.";
            }

            if (profile.totalReturns < 3)
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

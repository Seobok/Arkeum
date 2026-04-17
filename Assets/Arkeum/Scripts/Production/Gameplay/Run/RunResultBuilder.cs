using System.Collections.Generic;
using Arkeum.Production.Gameplay.Progression;

namespace Arkeum.Production.Gameplay.Run
{
    public sealed class RunResultBuilder
    {
        public IReadOnlyList<string> BuildLostLines(RunState runState)
        {
            return new[]
            {
                $"Blood shards lost: {runState.BloodShards}",
                $"Draughts left behind: {runState.DraughtCount}",
                runState.TemporaryWeaponEquipped
                    ? "Temporary weapon lost at the end of the run."
                    : "No temporary weapon was equipped.",
            };
        }

        public IReadOnlyList<string> BuildKeptLines(RunState runState, SaveProfile profile)
        {
            return new[]
            {
                $"Gleam gained: +{runState.GleamReward}",
                $"Total returns: {profile.TotalReturns}",
                $"Total gleam: {profile.Gleam}",
            };
        }
    }
}

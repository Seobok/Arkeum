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
                FormatWeaponLoss(runState),
            };
        }

        public IReadOnlyList<string> BuildKeptLines(RunState runState, SaveProfile profile)
        {
            return new[]
            {
                $"Total gold: {profile.Gold}",
                $"Total returns: {profile.TotalReturns}",
            };
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

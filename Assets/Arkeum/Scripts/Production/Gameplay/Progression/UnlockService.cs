namespace Arkeum.Production.Gameplay.Progression
{
    public sealed class UnlockService
    {
        public bool TryUnlockStartingBandage(SaveProfile profile, int cost)
        {
            if (profile == null || profile.StartingBandageUnlocked || profile.Gleam < cost)
            {
                return false;
            }

            profile.Gleam -= cost;
            profile.StartingBandageUnlocked = true;
            return true;
        }
    }
}

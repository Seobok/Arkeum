using System;
using System.Collections.Generic;

namespace Arkeum.Production.Gameplay.Progression
{
    [Serializable]
    public sealed class SaveProfile
    {
        public int TotalReturns;
        public int HighestDepth;
        public int Gleam;
        public bool StartingBandageUnlocked;
        public bool Mq01Completed;
        public List<string> UnlockedFlags = new List<string>();
        public List<string> CompletedQuestIds = new List<string>();
    }
}

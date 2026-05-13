using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arkeum.Production.Gameplay.Progression
{
    [Serializable]
    public sealed class SaveProfile
    {
        public int TotalReturns;
        public int HighestFloor;
        [FormerlySerializedAs("Shard")]
        [SerializeField] private int gold;
        public bool Mq01Completed;
        public List<string> UnlockedFlags = new List<string>();
        public List<string> CompletedQuestIds = new List<string>();

        public int Gold => gold;

        public event Action GoldChanged;

        public void SetGold(int value)
        {
            int next = Math.Max(0, value);
            if (gold == next)
            {
                return;
            }

            gold = next;
            GoldChanged?.Invoke();
        }

        public void AddGold(int amount)
        {
            SetGold(gold + amount);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Prototype
{
    public enum GamePhase
    {
        Hub,
        InRun,
        RunResult,
    }

    public enum RunEndReason
    {
        None,
        Death,
        DepthClear,
    }

    public enum ActorKind
    {
        Player,
        AshCrawler,
        IronWarden,
    }

    public enum BrainType
    {
        Player,
        Chaser,
        HeavyChaser,
    }

    [Serializable]
    public sealed class ProfileSaveData
    {
        public int totalReturns;
        public int highestReachedDepth;
        public int jangwang;
        public bool unlockedStartingBandage;
        public bool mq01Completed;
        public List<string> unlockedFlags = new List<string>();
        public List<string> completedQuestIds = new List<string>();
    }

    [Serializable]
    public sealed class RunSnapshot
    {
        public int runIndex;
        public int depthReached;
        public int hyeolpyeon;
        public int turnCount;
        public int currentHp;
        public int maxHp;
        public int bandageCount;
        public int draughtCount;
        public bool temporaryWeaponEquipped;
        public string runEndReason;
    }

    public sealed class ActorRuntime
    {
        public ActorKind Kind;
        public BrainType Brain;
        public string DisplayName;
        public Vector2Int Position;
        public int MaxHp;
        public int Hp;
        public int Attack;
        public int Defense;
        public int ActionInterval;
        public int BloodReward;
        public bool IsAlive = true;
        public GameObject View;
        public bool DropsTemporaryWeapon;

        public bool IsPlayer => Kind == ActorKind.Player;
    }

    public sealed class RunState
    {
        public int RunIndex;
        public int TurnCount;
        public int DepthReached;
        public int Hyeolpyeon;
        public int CurrentHp;
        public int MaxHp;
        public int BandageCount;
        public int DraughtCount;
        public int AttackBonus;
        public bool TemporaryWeaponEquipped;
        public RunEndReason EndReason;
        public readonly List<string> ResultLines = new List<string>();

        public int EffectiveAttack => 3 + AttackBonus;
    }

    public sealed class DungeonLayout
    {
        public readonly HashSet<Vector2Int> Walkable = new HashSet<Vector2Int>();
        public readonly Dictionary<Vector2Int, int> DepthByCell = new Dictionary<Vector2Int, int>();
        public readonly List<Vector2Int> TemporaryWeaponSpawns = new List<Vector2Int>();
        public Vector2Int PlayerSpawn;
        public Vector2Int MerchantPosition;
        public Vector2Int ReliquaryPosition;

        public bool IsWalkable(Vector2Int cell)
        {
            return Walkable.Contains(cell);
        }

        public int GetDepth(Vector2Int cell)
        {
            if (DepthByCell.TryGetValue(cell, out int depth))
            {
                return depth;
            }

            return 1;
        }
    }

    public sealed class HubSceneLayout
    {
        public DungeonLayout Layout;
        public Vector2Int PlayerPosition;
        public Vector2Int StartGatePosition;
        public Vector2Int UnlockPosition;
        public Vector2Int UndertakerPosition;
    }
}

using System;
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Map;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class RunFloorDefinition
    {
        public int FloorIndex = 1;
        public RunMapGenerationMode GenerationMode = RunMapGenerationMode.CellularAutomata;
        public MapAsset MapAsset;
        public List<MapAsset> RoomAssets = new List<MapAsset>();
        public List<RunSpecialRoomDefinition> SpecialRooms = new List<RunSpecialRoomDefinition>();
        public int MinimumRoomCount = 6;
        public int RoomGap = 5;
        public int PlacementAttempts = 300;
        public CellularAutomataMapSettings CellularAutomataSettings = new CellularAutomataMapSettings();
    }

    public enum RunMapGenerationMode
    {
        RoomGraph,
        CellularAutomata,
        FixedMapAsset,
    }

    [Serializable]
    public sealed class CellularAutomataMapSettings
    {
        public int Width = 48;
        public int Height = 30;
        public int FillPercent = 45;
        public int SmoothIterations = 5;
        public int BirthLimit = 4;
        public int DeathLimit = 3;
        public int BorderThickness = 1;
        public int EnemySpawnZoneSize = 5;
        public int EnemySpawnSafeDistanceFromPlayer = 6;
    }

    public enum RunSpecialRoomType
    {
        Generic,
        FloorExit,
        Boss,
        Shop,
    }

    [Serializable]
    public sealed class RunSpecialRoomDefinition
    {
        public RunSpecialRoomType RoomType = RunSpecialRoomType.Generic;
        public MapAsset RoomAsset;
        public int Count = 1;
    }
}

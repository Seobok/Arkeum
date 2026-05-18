using System;
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Map;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class RunFloorDefinition
    {
        public int FloorIndex = 1;
        public MapAsset MapAsset;
        public List<MapAsset> RoomAssets = new List<MapAsset>();
        public List<RunSpecialRoomDefinition> SpecialRooms = new List<RunSpecialRoomDefinition>();
        public int MinimumRoomCount = 6;
        public int RoomGap = 5;
        public int PlacementAttempts = 300;
    }

    public enum RunSpecialRoomType
    {
        Generic,
        FloorExit,
    }

    [Serializable]
    public sealed class RunSpecialRoomDefinition
    {
        public RunSpecialRoomType RoomType = RunSpecialRoomType.Generic;
        public MapAsset RoomAsset;
        public int Count = 1;
    }
}

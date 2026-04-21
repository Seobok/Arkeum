using System;
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    public sealed class MapGenerator
    {
        private readonly MapAsset runMapAsset;
        private readonly MapAsset hubMapAsset;
        private readonly RunDefinition runDefinition;

        public MapGenerator(MapAsset runMapAsset, MapAsset hubMapAsset, RunDefinition runDefinition)
        {
            this.runMapAsset = runMapAsset;
            this.hubMapAsset = hubMapAsset;
            this.runDefinition = runDefinition;
        }

        public RunFloorDefinition GetRunFloor(int floorIndex)
        {
            return runDefinition != null ? runDefinition.GetFloor(floorIndex) : null;
        }

        public MapDefinition CreateRunMap(RunFloorDefinition floorDefinition, int fallbackFloor)
        {
            int floor = floorDefinition != null ? floorDefinition.FloorIndex : fallbackFloor;
            MapAsset floorMapAsset = floorDefinition != null && floorDefinition.MapAsset != null
                ? floorDefinition.MapAsset
                : runMapAsset;

            RoomTemplate roomTemplate = CreateRoomTemplate(floorMapAsset);
            DungeonGenerationSettings settings = DungeonGenerationSettings.From(floorDefinition, floor);
            MapDefinition map = CreateDungeonMap(roomTemplate, settings);
            map.RunFloor = floor;
            return map;
        }

        public MapDefinition CreateHubMap()
        {
            if (TryCreateFromAsset(hubMapAsset, out MapDefinition assetMap))
            {
                return assetMap;
            }

            MapDefinition map = new MapDefinition
            {
                RunFloor = 0,
                PlayerSpawn = new Vector2Int(0, 0),
                StartAltarPosition = new Vector2Int(-2, 0),
                UnlockAltarPosition = new Vector2Int(0, 2),
                UndertakerPosition = new Vector2Int(2, 0),
            };

            AddRoom(map, -3, -2, 3, 2, 0);
            return map;
        }

        private static MapDefinition CreateDungeonMap(RoomTemplate roomTemplate, DungeonGenerationSettings settings)
        {
            System.Random random = new System.Random(settings.RandomSeed + settings.Floor * 97);
            MapDefinition map = new MapDefinition
            {
                RunFloor = settings.Floor,
                PlayerSpawn = Vector2Int.zero,
            };

            List<PlacedRoom> rooms = new List<PlacedRoom>();
            HashSet<Vector2Int> occupiedRoomCells = new HashSet<Vector2Int>();
            HashSet<Vector2Int> occupiedGridPositions = new HashSet<Vector2Int>();
            int gridStepX = roomTemplate.Width + settings.RoomGap;
            int gridStepY = roomTemplate.Height + settings.RoomGap;

            PlacedRoom startRoom = CreatePlacedRoom(0, roomTemplate, Vector2Int.zero, Vector2Int.zero);
            rooms.Add(startRoom);
            occupiedGridPositions.Add(Vector2Int.zero);
            AddPlacedRoom(map, startRoom, occupiedRoomCells);

            int attempts = 0;
            while (rooms.Count < settings.MinimumRoomCount && attempts < settings.PlacementAttempts)
            {
                attempts++;
                PlacedRoom parent = rooms[random.Next(rooms.Count)];
                DoorDirection direction = RandomDirection(random);
                Vector2Int gridDirection = ToVector(direction);
                Vector2Int candidateGrid = parent.GridPosition + gridDirection;

                if (occupiedGridPositions.Contains(candidateGrid))
                {
                    continue;
                }

                Vector2Int candidateOrigin = new Vector2Int(candidateGrid.x * gridStepX, candidateGrid.y * gridStepY);
                PlacedRoom candidate = CreatePlacedRoom(rooms.Count, roomTemplate, candidateOrigin, candidateGrid);

                if (OverlapsAnyRoom(candidate, rooms))
                {
                    continue;
                }

                if (!TryBuildDoorConnection(parent, candidate, out DoorConnection connection))
                {
                    continue;
                }

                if (!TryBuildCorridorCells(connection.FromDoor.Position, connection.FromDoor.Direction, connection.ToDoor.Position, connection.ToDoor.Direction, out List<Vector2Int> corridorCells))
                {
                    continue;
                }

                if (!IsCorridorValid(corridorCells, connection.FromDoor.Position, connection.ToDoor.Position, occupiedRoomCells, candidate.CellSet))
                {
                    continue;
                }

                rooms.Add(candidate);
                occupiedGridPositions.Add(candidateGrid);
                AddPlacedRoom(map, candidate, occupiedRoomCells);
                AddDoor(parent.Definition, connection.FromDoor);
                AddDoor(candidate.Definition, connection.ToDoor);
                AddCorridor(map, parent.Id, candidate.Id, connection, corridorCells, settings.Floor);
            }

            if (rooms.Count < settings.MinimumRoomCount)
            {
                return CreateFallbackDungeonMap(roomTemplate, settings);
            }

            ApplyRunMarkers(map, rooms);
            return map;
        }

        private static MapDefinition CreateFallbackDungeonMap(RoomTemplate roomTemplate, DungeonGenerationSettings settings)
        {
            MapDefinition map = new MapDefinition
            {
                RunFloor = settings.Floor,
                PlayerSpawn = Vector2Int.zero,
            };

            List<PlacedRoom> rooms = new List<PlacedRoom>();
            HashSet<Vector2Int> occupiedRoomCells = new HashSet<Vector2Int>();
            int gridStepX = roomTemplate.Width + settings.RoomGap;

            for (int i = 0; i < settings.MinimumRoomCount; i++)
            {
                Vector2Int origin = new Vector2Int(i * gridStepX, 0);
                PlacedRoom room = CreatePlacedRoom(i, roomTemplate, origin, new Vector2Int(i, 0));
                rooms.Add(room);
                AddPlacedRoom(map, room, occupiedRoomCells);

                if (i == 0)
                {
                    continue;
                }

                PlacedRoom previous = rooms[i - 1];
                if (!TryBuildDoorConnection(previous, room, out DoorConnection connection))
                {
                    continue;
                }

                TryBuildCorridorCells(connection.FromDoor.Position, connection.FromDoor.Direction, connection.ToDoor.Position, connection.ToDoor.Direction, out List<Vector2Int> corridorCells);
                AddDoor(previous.Definition, connection.FromDoor);
                AddDoor(room.Definition, connection.ToDoor);
                AddCorridor(map, previous.Id, room.Id, connection, corridorCells, settings.Floor);
            }

            ApplyRunMarkers(map, rooms);
            return map;
        }

        private static RoomTemplate CreateRoomTemplate(MapAsset asset)
        {
            List<TemplateCell> cells = new List<TemplateCell>();
            List<TemplateDoor> doors = new List<TemplateDoor>();
            Vector2Int origin = asset != null ? asset.PlayerSpawn : Vector2Int.zero;

            if (asset != null)
            {
                HashSet<Vector2Int> walkablePositions = new HashSet<Vector2Int>();
                for (int i = 0; i < asset.Cells.Count; i++)
                {
                    MapCellData cell = asset.Cells[i];
                    if (cell == null || !cell.Walkable)
                    {
                        continue;
                    }

                    Vector2Int position = cell.Position - origin;
                    cells.Add(new TemplateCell(position, cell.Depth));
                    walkablePositions.Add(position);
                }

                for (int i = 0; i < asset.Doors.Count; i++)
                {
                    MapDoorData door = asset.Doors[i];
                    if (door == null)
                    {
                        continue;
                    }

                    Vector2Int position = door.Position - origin;
                    if (!walkablePositions.Contains(position))
                    {
                        continue;
                    }

                    doors.Add(new TemplateDoor(position, door.Direction));
                }
            }

            if (cells.Count == 0)
            {
                for (int x = -3; x <= 3; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        cells.Add(new TemplateCell(new Vector2Int(x, y), 1));
                    }
                }
            }

            if (doors.Count == 0)
            {
                AddDefaultDoors(cells, doors);
            }

            return new RoomTemplate(cells, doors);
        }

        private static PlacedRoom CreatePlacedRoom(int id, RoomTemplate template, Vector2Int origin, Vector2Int gridPosition)
        {
            DungeonRoomDefinition definition = new DungeonRoomDefinition
            {
                Id = id,
                Origin = origin,
                Min = template.Min + origin,
                Max = template.Max + origin,
            };

            Dictionary<Vector2Int, int> depthByCell = new Dictionary<Vector2Int, int>();
            HashSet<Vector2Int> cellSet = new HashSet<Vector2Int>();
            List<DungeonDoorDefinition> doorCandidates = new List<DungeonDoorDefinition>();
            for (int i = 0; i < template.Cells.Count; i++)
            {
                TemplateCell templateCell = template.Cells[i];
                Vector2Int cell = templateCell.Position + origin;
                definition.Cells.Add(cell);
                depthByCell[cell] = templateCell.Depth;
                cellSet.Add(cell);
            }

            for (int i = 0; i < template.Doors.Count; i++)
            {
                TemplateDoor templateDoor = template.Doors[i];
                doorCandidates.Add(new DungeonDoorDefinition
                {
                    Position = templateDoor.Position + origin,
                    Direction = templateDoor.Direction,
                });
            }

            return new PlacedRoom(id, gridPosition, definition, depthByCell, cellSet, doorCandidates);
        }

        private static bool TryBuildDoorConnection(PlacedRoom from, PlacedRoom to, out DoorConnection connection)
        {
            DoorDirection fromDirection = DirectionToward(from.Definition.Origin, to.Definition.Origin);
            DoorDirection toDirection = Opposite(fromDirection);
            if (!TryCreateDoor(from, fromDirection, to.Definition.Origin, out DungeonDoorDefinition fromDoor) ||
                !TryCreateDoor(to, toDirection, from.Definition.Origin, out DungeonDoorDefinition toDoor))
            {
                connection = default;
                return false;
            }

            connection = new DoorConnection(fromDoor, toDoor);
            return true;
        }

        private static bool TryCreateDoor(PlacedRoom room, DoorDirection direction, Vector2Int target, out DungeonDoorDefinition door)
        {
            DungeonDoorDefinition best = null;
            int bestScore = int.MaxValue;

            for (int i = 0; i < room.DoorCandidates.Count; i++)
            {
                DungeonDoorDefinition candidate = room.DoorCandidates[i];
                if (candidate.Direction != direction)
                {
                    continue;
                }

                int score = direction == DoorDirection.Right || direction == DoorDirection.Left
                    ? Mathf.Abs(candidate.Position.y - target.y)
                    : Mathf.Abs(candidate.Position.x - target.x);

                if (score < bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            if (best == null)
            {
                door = null;
                return false;
            }

            door = new DungeonDoorDefinition
            {
                Position = best.Position,
                Direction = best.Direction,
            };
            return true;
        }

        private static bool TryBuildCorridorCells(Vector2Int from, DoorDirection fromDirection, Vector2Int to, DoorDirection toDirection, out List<Vector2Int> cells)
        {
            cells = new List<Vector2Int>();
            if (fromDirection == toDirection)
            {
                return false;
            }

            List<Vector2Int> points = new List<Vector2Int> { from };
            if (IsPerpendicular(fromDirection, toDirection))
            {
                Vector2Int horizontalDoor = IsHorizontal(fromDirection) ? from : to;
                Vector2Int verticalDoor = IsVertical(fromDirection) ? from : to;
                points.Add(new Vector2Int(verticalDoor.x, horizontalDoor.y));
            }
            else if (IsHorizontal(fromDirection))
            {
                if (from.y != to.y)
                {
                    int middleX = (from.x + to.x) / 2;
                    points.Add(new Vector2Int(middleX, from.y));
                    points.Add(new Vector2Int(middleX, to.y));
                }
            }
            else if (from.x != to.x)
            {
                int middleY = (from.y + to.y) / 2;
                points.Add(new Vector2Int(from.x, middleY));
                points.Add(new Vector2Int(to.x, middleY));
            }

            points.Add(to);
            AddSegments(points, cells);
            return cells.Count > 0;
        }

        private static void AddSegments(IReadOnlyList<Vector2Int> points, List<Vector2Int> cells)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2Int current = points[i];
                Vector2Int target = points[i + 1];
                Vector2Int step = new Vector2Int(Math.Sign(target.x - current.x), Math.Sign(target.y - current.y));

                while (current != target)
                {
                    AddUnique(cells, current);
                    current += step;
                }
            }

            AddUnique(cells, points[points.Count - 1]);
        }

        private static bool IsCorridorValid(
            List<Vector2Int> corridorCells,
            Vector2Int fromDoor,
            Vector2Int toDoor,
            HashSet<Vector2Int> existingRoomCells,
            HashSet<Vector2Int> candidateRoomCells)
        {
            HashSet<Vector2Int> allRoomCells = new HashSet<Vector2Int>(existingRoomCells);
            foreach (Vector2Int roomCell in candidateRoomCells)
            {
                allRoomCells.Add(roomCell);
            }

            for (int i = 0; i < corridorCells.Count; i++)
            {
                Vector2Int cell = corridorCells[i];
                bool isDoor = cell == fromDoor || cell == toDoor;
                if (allRoomCells.Contains(cell) && !isDoor)
                {
                    return false;
                }

                if (isDoor)
                {
                    continue;
                }

                if (TouchesRoomAwayFromDoor(cell, fromDoor, toDoor, allRoomCells))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TouchesRoomAwayFromDoor(Vector2Int cell, Vector2Int fromDoor, Vector2Int toDoor, HashSet<Vector2Int> roomCells)
        {
            for (int i = 0; i < CardinalOffsets.Length; i++)
            {
                Vector2Int neighbor = cell + CardinalOffsets[i];
                if (neighbor != fromDoor && neighbor != toDoor && roomCells.Contains(neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddPlacedRoom(MapDefinition map, PlacedRoom room, HashSet<Vector2Int> occupiedRoomCells)
        {
            map.Rooms.Add(room.Definition);
            foreach (KeyValuePair<Vector2Int, int> pair in room.DepthByCell)
            {
                AddWalkableCell(map, pair.Key, pair.Value);
                occupiedRoomCells.Add(pair.Key);
            }
        }

        private static void AddCorridor(MapDefinition map, int fromRoomId, int toRoomId, DoorConnection connection, List<Vector2Int> corridorCells, int floor)
        {
            DungeonCorridorDefinition corridor = new DungeonCorridorDefinition
            {
                FromRoomId = fromRoomId,
                ToRoomId = toRoomId,
                FromDoor = connection.FromDoor.Position,
                FromDirection = connection.FromDoor.Direction,
                ToDoor = connection.ToDoor.Position,
                ToDirection = connection.ToDoor.Direction,
            };

            for (int i = 0; i < corridorCells.Count; i++)
            {
                Vector2Int cell = corridorCells[i];
                corridor.Cells.Add(cell);
                AddWalkableCell(map, cell, Mathf.Max(1, floor));
            }

            map.Corridors.Add(corridor);
        }

        private static void ApplyRunMarkers(MapDefinition map, IReadOnlyList<PlacedRoom> rooms)
        {
            map.PlayerSpawn = Vector2Int.zero;
            if (rooms.Count > 2)
            {
                map.TemporaryWeaponSpawns.Add(rooms[2].Definition.Origin);
            }

            if (rooms.Count > 3)
            {
                map.MerchantPosition = rooms[3].Definition.Origin;
            }

            if (rooms.Count > 0)
            {
                map.ReliquaryPosition = rooms[rooms.Count - 1].Definition.Origin;
            }
        }

        private static bool OverlapsAnyRoom(PlacedRoom candidate, IReadOnlyList<PlacedRoom> rooms)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                foreach (Vector2Int cell in candidate.CellSet)
                {
                    if (rooms[i].CellSet.Contains(cell))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void AddDefaultDoors(List<TemplateCell> cells, List<TemplateDoor> doors)
        {
            Vector2Int min = cells[0].Position;
            Vector2Int max = cells[0].Position;
            for (int i = 1; i < cells.Count; i++)
            {
                min = Vector2Int.Min(min, cells[i].Position);
                max = Vector2Int.Max(max, cells[i].Position);
            }

            Vector2Int center = new Vector2Int((min.x + max.x) / 2, (min.y + max.y) / 2);
            AddClosestDefaultDoor(cells, doors, DoorDirection.Right, new Vector2Int(max.x, center.y));
            AddClosestDefaultDoor(cells, doors, DoorDirection.Left, new Vector2Int(min.x, center.y));
            AddClosestDefaultDoor(cells, doors, DoorDirection.Up, new Vector2Int(center.x, max.y));
            AddClosestDefaultDoor(cells, doors, DoorDirection.Down, new Vector2Int(center.x, min.y));
        }

        private static void AddClosestDefaultDoor(List<TemplateCell> cells, List<TemplateDoor> doors, DoorDirection direction, Vector2Int target)
        {
            Vector2Int best = cells[0].Position;
            int bestScore = int.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                int score = Mathf.Abs(cells[i].Position.x - target.x) + Mathf.Abs(cells[i].Position.y - target.y);
                if (score < bestScore)
                {
                    best = cells[i].Position;
                    bestScore = score;
                }
            }

            doors.Add(new TemplateDoor(best, direction));
        }

        private static void AddRoom(MapDefinition map, int minX, int minY, int maxX, int maxY, int depth)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    AddWalkableCell(map, new Vector2Int(x, y), depth);
                }
            }
        }

        private static void AddWalkableCell(MapDefinition map, Vector2Int cell, int depth)
        {
            if (!map.WalkableCells.Contains(cell))
            {
                map.WalkableCells.Add(cell);
            }

            map.DepthByCell[cell] = depth;
        }

        private static void AddDoor(DungeonRoomDefinition room, DungeonDoorDefinition door)
        {
            for (int i = 0; i < room.Doors.Count; i++)
            {
                if (room.Doors[i].Position == door.Position && room.Doors[i].Direction == door.Direction)
                {
                    return;
                }
            }

            room.Doors.Add(door);
        }

        private static bool TryCreateFromAsset(MapAsset asset, out MapDefinition map)
        {
            if (asset == null)
            {
                map = null;
                return false;
            }

            map = new MapDefinition
            {
                RunFloor = 0,
                PlayerSpawn = asset.PlayerSpawn,
                MerchantPosition = asset.MerchantPosition,
                ReliquaryPosition = asset.ReliquaryPosition,
                StartAltarPosition = asset.StartAltarPosition,
                UnlockAltarPosition = asset.UnlockAltarPosition,
                UndertakerPosition = asset.UndertakerPosition,
            };

            for (int i = 0; i < asset.TemporaryWeaponSpawns.Count; i++)
            {
                map.TemporaryWeaponSpawns.Add(asset.TemporaryWeaponSpawns[i]);
            }

            for (int i = 0; i < asset.Cells.Count; i++)
            {
                MapCellData cell = asset.Cells[i];
                if (cell == null || !cell.Walkable)
                {
                    continue;
                }

                AddWalkableCell(map, cell.Position, cell.Depth);
            }

            return map.WalkableCells.Count > 0;
        }

        private static DoorDirection DirectionToward(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                return delta.x >= 0 ? DoorDirection.Right : DoorDirection.Left;
            }

            return delta.y >= 0 ? DoorDirection.Up : DoorDirection.Down;
        }

        private static DoorDirection Opposite(DoorDirection direction)
        {
            switch (direction)
            {
                case DoorDirection.Up:
                    return DoorDirection.Down;
                case DoorDirection.Down:
                    return DoorDirection.Up;
                case DoorDirection.Left:
                    return DoorDirection.Right;
                default:
                    return DoorDirection.Left;
            }
        }

        private static bool IsPerpendicular(DoorDirection first, DoorDirection second)
        {
            return IsHorizontal(first) && IsVertical(second) || IsVertical(first) && IsHorizontal(second);
        }

        private static bool IsHorizontal(DoorDirection direction)
        {
            return direction == DoorDirection.Left || direction == DoorDirection.Right;
        }

        private static bool IsVertical(DoorDirection direction)
        {
            return direction == DoorDirection.Up || direction == DoorDirection.Down;
        }

        private static DoorDirection RandomDirection(System.Random random)
        {
            return (DoorDirection)random.Next(4);
        }

        private static Vector2Int ToVector(DoorDirection direction)
        {
            switch (direction)
            {
                case DoorDirection.Up:
                    return Vector2Int.up;
                case DoorDirection.Down:
                    return Vector2Int.down;
                case DoorDirection.Left:
                    return Vector2Int.left;
                default:
                    return Vector2Int.right;
            }
        }

        private static void AddUnique(List<Vector2Int> cells, Vector2Int cell)
        {
            if (cells.Count == 0 || cells[cells.Count - 1] != cell)
            {
                cells.Add(cell);
            }
        }

        private static readonly Vector2Int[] CardinalOffsets =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        private sealed class RoomTemplate
        {
            public readonly List<TemplateCell> Cells;
            public readonly List<TemplateDoor> Doors;
            public readonly Vector2Int Min;
            public readonly Vector2Int Max;
            public readonly int Width;
            public readonly int Height;

            public RoomTemplate(List<TemplateCell> cells, List<TemplateDoor> doors)
            {
                Cells = cells;
                Doors = doors;
                Min = cells[0].Position;
                Max = cells[0].Position;

                for (int i = 1; i < cells.Count; i++)
                {
                    Vector2Int position = cells[i].Position;
                    Min = Vector2Int.Min(Min, position);
                    Max = Vector2Int.Max(Max, position);
                }

                Width = Max.x - Min.x + 1;
                Height = Max.y - Min.y + 1;
            }
        }

        private readonly struct TemplateDoor
        {
            public readonly Vector2Int Position;
            public readonly DoorDirection Direction;

            public TemplateDoor(Vector2Int position, DoorDirection direction)
            {
                Position = position;
                Direction = direction;
            }
        }

        private readonly struct TemplateCell
        {
            public readonly Vector2Int Position;
            public readonly int Depth;

            public TemplateCell(Vector2Int position, int depth)
            {
                Position = position;
                Depth = depth;
            }
        }

        private sealed class PlacedRoom
        {
            public readonly int Id;
            public readonly Vector2Int GridPosition;
            public readonly DungeonRoomDefinition Definition;
            public readonly Dictionary<Vector2Int, int> DepthByCell;
            public readonly HashSet<Vector2Int> CellSet;
            public readonly List<DungeonDoorDefinition> DoorCandidates;

            public PlacedRoom(
                int id,
                Vector2Int gridPosition,
                DungeonRoomDefinition definition,
                Dictionary<Vector2Int, int> depthByCell,
                HashSet<Vector2Int> cellSet,
                List<DungeonDoorDefinition> doorCandidates)
            {
                Id = id;
                GridPosition = gridPosition;
                Definition = definition;
                DepthByCell = depthByCell;
                CellSet = cellSet;
                DoorCandidates = doorCandidates;
            }
        }

        private readonly struct DungeonGenerationSettings
        {
            public readonly int Floor;
            public readonly int MinimumRoomCount;
            public readonly int RoomGap;
            public readonly int PlacementAttempts;
            public readonly int RandomSeed;

            private DungeonGenerationSettings(int floor, int minimumRoomCount, int roomGap, int placementAttempts, int randomSeed)
            {
                Floor = floor;
                MinimumRoomCount = Mathf.Max(6, minimumRoomCount);
                RoomGap = Mathf.Max(1, roomGap);
                PlacementAttempts = Mathf.Max(MinimumRoomCount, placementAttempts);
                RandomSeed = randomSeed;
            }

            public static DungeonGenerationSettings From(RunFloorDefinition floorDefinition, int floor)
            {
                if (floorDefinition == null)
                {
                    return new DungeonGenerationSettings(floor, 6, 5, 300, 173);
                }

                return new DungeonGenerationSettings(
                    floor,
                    floorDefinition.MinimumRoomCount,
                    floorDefinition.RoomGap,
                    floorDefinition.PlacementAttempts,
                    floorDefinition.RandomSeed);
            }
        }

        private readonly struct DoorConnection
        {
            public readonly DungeonDoorDefinition FromDoor;
            public readonly DungeonDoorDefinition ToDoor;

            public DoorConnection(DungeonDoorDefinition fromDoor, DungeonDoorDefinition toDoor)
            {
                FromDoor = fromDoor;
                ToDoor = toDoor;
            }
        }
    }
}

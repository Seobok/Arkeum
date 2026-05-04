using System;
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    public sealed class MapGenerator
    {
        private readonly MapAsset hubMapAsset;
        private readonly RunDefinition runDefinition;

        public MapGenerator(MapAsset hubMapAsset, RunDefinition runDefinition)
        {
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
            if(floorDefinition == null || floorDefinition.MapAsset == null)
            {
                Debug.LogError("[MapGenerator] Failed to CreateRunMap. RunFloorDefinition is inValid");
                return null;
            }

            MapAsset floorMapAsset = floorDefinition.MapAsset;

            Debug.Log(
                $"[MapGenerator] CreateRunMap floor={floor}, fallbackFloor={fallbackFloor}, " +
                $"floorDefinition={(floorDefinition != null ? "set" : "null")}, " +
                $"floorMapAsset={DescribeAsset(floorMapAsset)}, " +
                $"roomAssetCount={(floorDefinition != null && floorDefinition.RoomAssets != null ? floorDefinition.RoomAssets.Count : 0)}, " +
                $"specialRoomDefinitionCount={(floorDefinition != null && floorDefinition.SpecialRooms != null ? floorDefinition.SpecialRooms.Count : 0)}");

            RoomTemplateSet roomTemplates = CreateRoomTemplates(floorDefinition, floorMapAsset);
            DungeonGenerationSettings settings = DungeonGenerationSettings.From(floorDefinition, floor, roomTemplates.SpecialRoomSlots.Count);
            MapDefinition map = CreateDungeonMap(roomTemplates, settings);
            map.RunFloor = floor;
            return map;
        }

        public MapDefinition CreateHubMap()
        {
            if (TryCreateFromAsset(hubMapAsset, out MapDefinition assetMap))
            {
                Debug.Log($"[MapGenerator] CreateHubMap loaded from asset={DescribeAsset(hubMapAsset)} cells={assetMap.WalkableCells.Count}");
                return assetMap;
            }

            Debug.LogWarning($"[MapGenerator] CreateHubMap using built-in fallback. hubMapAsset={DescribeAsset(hubMapAsset)}");

            MapDefinition map = new MapDefinition
            {
                RunFloor = 0,
                PlayerSpawn = new Vector2Int(0, 0),
                DungeonEntrancePosition = new Vector2Int(-2, 0),
            };

            AddRoom(map, -3, -2, 3, 2);
            return map;
        }

        private static MapDefinition CreateDungeonMap(RoomTemplateSet roomTemplates, DungeonGenerationSettings settings)
        {
            System.Random random = new System.Random();
            MapDefinition map = new MapDefinition
            {
                RunFloor = settings.Floor,
                PlayerSpawn = Vector2Int.zero,
            };

            List<PlacedRoom> rooms = new List<PlacedRoom>();
            HashSet<Vector2Int> occupiedRoomCells = new HashSet<Vector2Int>();
            HashSet<Vector2Int> occupiedGridPositions = new HashSet<Vector2Int>();

            PlacedRoom startRoom = CreatePlacedRoom(0, roomTemplates.StartRoom, Vector2Int.zero, Vector2Int.zero);
            rooms.Add(startRoom);
            occupiedGridPositions.Add(Vector2Int.zero);
            AddPlacedRoom(map, startRoom, occupiedRoomCells);

            int occupiedGridRejects = 0;
            int overlapRejects = 0;
            int doorRejects = 0;
            int corridorBuildRejects = 0;
            int corridorValidityRejects = 0;
            int attempts = 0;
            Dictionary<int, RoomTemplate> specialRoomSlots = CreateSpecialRoomSlotMap(settings.MinimumRoomCount, roomTemplates.SpecialRoomSlots, random);
            while (rooms.Count < settings.MinimumRoomCount && attempts < settings.PlacementAttempts)
            {
                attempts++;
                PlacedRoom parent = rooms[random.Next(rooms.Count)];
                DoorDirection direction = RandomDirection(random);
                Vector2Int gridDirection = ToVector(direction);
                Vector2Int candidateGrid = parent.GridPosition + gridDirection;

                if (occupiedGridPositions.Contains(candidateGrid))
                {
                    occupiedGridRejects++;
                    continue;
                }

                RoomTemplate candidateTemplate = SelectRoomTemplate(roomTemplates, specialRoomSlots, rooms.Count, random);
                Vector2Int candidateOrigin = CalculateCandidateOrigin(parent, candidateTemplate, direction, settings.RoomGap);
                PlacedRoom candidate = CreatePlacedRoom(rooms.Count, candidateTemplate, candidateOrigin, candidateGrid);

                if (OverlapsAnyRoom(candidate, rooms))
                {
                    overlapRejects++;
                    continue;
                }

                if (!TryBuildDoorConnection(parent, candidate, out DoorConnection connection))
                {
                    doorRejects++;
                    continue;
                }

                if (!TryBuildCorridorCells(connection.FromDoor.Position, connection.FromDoor.Direction, connection.ToDoor.Position, connection.ToDoor.Direction, out List<Vector2Int> corridorCells))
                {
                    corridorBuildRejects++;
                    continue;
                }

                if (!IsCorridorValid(corridorCells, connection.FromDoor.Position, connection.ToDoor.Position, occupiedRoomCells, candidate.CellSet))
                {
                    corridorValidityRejects++;
                    continue;
                }

                rooms.Add(candidate);
                occupiedGridPositions.Add(candidateGrid);
                AddPlacedRoom(map, candidate, occupiedRoomCells);
                AddDoor(parent.Definition, connection.FromDoor);
                AddDoor(candidate.Definition, connection.ToDoor);
                AddCorridor(map, parent.Id, candidate.Id, connection, corridorCells, settings.Floor);
            }

            Debug.Log(
                $"[MapGenerator] Dungeon placement summary floor={settings.Floor}, rooms={rooms.Count}/{settings.MinimumRoomCount}, " +
                $"attempts={attempts}/{settings.PlacementAttempts}, cells={map.WalkableCells.Count}, corridors={map.Corridors.Count}, " +
                $"specialRooms={CountSpecialRooms(rooms)}/{roomTemplates.SpecialRoomSlots.Count}, " +
                $"rejects occupiedGrid={occupiedGridRejects}, overlap={overlapRejects}, missingDoor={doorRejects}, " +
                $"corridorBuild={corridorBuildRejects}, corridorInvalid={corridorValidityRejects}");

            if (rooms.Count < settings.MinimumRoomCount)
            {
                Debug.LogWarning(
                    $"[MapGenerator] Dungeon placement failed to reach minimum room count. " +
                    $"Using fallback map. rooms={rooms.Count}, minimum={settings.MinimumRoomCount}");
                return CreateFallbackDungeonMap(roomTemplates, settings);
            }

            ApplyRunMarkers(map, rooms);
            Debug.Log($"[MapGenerator] Dungeon generated floor={settings.Floor}, rooms={map.Rooms.Count}, corridors={map.Corridors.Count}, cells={map.WalkableCells.Count}");
            return map;
        }

        private static MapDefinition CreateFallbackDungeonMap(RoomTemplateSet roomTemplates, DungeonGenerationSettings settings)
        {
            MapDefinition map = new MapDefinition
            {
                RunFloor = settings.Floor,
                PlayerSpawn = Vector2Int.zero,
            };

            List<PlacedRoom> rooms = new List<PlacedRoom>();
            HashSet<Vector2Int> occupiedRoomCells = new HashSet<Vector2Int>();
            int nextMinX = 0;
            Dictionary<int, RoomTemplate> specialRoomSlots = CreateSpecialRoomSlotMap(settings.MinimumRoomCount, roomTemplates.SpecialRoomSlots, new System.Random());

            for (int i = 0; i < settings.MinimumRoomCount; i++)
            {
                RoomTemplate roomTemplate = i == 0
                    ? roomTemplates.StartRoom
                    : SelectFallbackRoomTemplate(roomTemplates, specialRoomSlots, i);
                Vector2Int origin = new Vector2Int(nextMinX - roomTemplate.Min.x, 0);
                PlacedRoom room = CreatePlacedRoom(i, roomTemplate, origin, new Vector2Int(i, 0));
                rooms.Add(room);
                AddPlacedRoom(map, room, occupiedRoomCells);
                nextMinX = room.Definition.Max.x + settings.RoomGap + 1;

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
            Debug.LogWarning($"[MapGenerator] Fallback dungeon generated floor={settings.Floor}, rooms={map.Rooms.Count}, specialRooms={CountSpecialRooms(rooms)}/{roomTemplates.SpecialRoomSlots.Count}, corridors={map.Corridors.Count}, cells={map.WalkableCells.Count}");
            return map;
        }

        private static RoomTemplateSet CreateRoomTemplates(RunFloorDefinition floorDefinition, MapAsset startRoomAsset)
        {
            RoomTemplate startRoom = CreateRoomTemplate(startRoomAsset, false);
            List<MapAsset> roomAssets = new List<MapAsset>();
            List<MapAsset> specialRoomAssets = new List<MapAsset>();
            List<RoomTemplate> specialRoomSlots = new List<RoomTemplate>();

            if (floorDefinition != null && floorDefinition.SpecialRooms != null)
            {
                for (int i = 0; i < floorDefinition.SpecialRooms.Count; i++)
                {
                    RunSpecialRoomDefinition specialRoom = floorDefinition.SpecialRooms[i];
                    if (specialRoom == null || specialRoom.RoomAsset == null || specialRoom.Count <= 0)
                    {
                        continue;
                    }

                    AddUniqueAsset(specialRoomAssets, specialRoom.RoomAsset);
                    for (int count = 0; count < specialRoom.Count; count++)
                    {
                        specialRoomSlots.Add(CreateRoomTemplate(specialRoom.RoomAsset, true));
                    }
                }
            }

            if (floorDefinition != null && floorDefinition.RoomAssets != null)
            {
                for (int i = 0; i < floorDefinition.RoomAssets.Count; i++)
                {
                    AddUniqueNonSpecialAsset(roomAssets, specialRoomAssets, floorDefinition.RoomAssets[i]);
                }
            }

            List<RoomTemplate> rooms = new List<RoomTemplate>();
            for (int i = 0; i < roomAssets.Count; i++)
            {
                rooms.Add(CreateRoomTemplate(roomAssets[i], false));
            }

            if (rooms.Count == 0)
            {
                rooms.Add(startRoom);
            }

            Debug.Log(
                $"[MapGenerator] Room templates ready. startAsset={DescribeAsset(startRoomAsset)}, " +
                $"startCells={startRoom.Cells.Count}, startDoors={startRoom.Doors.Count}, roomTemplates={rooms.Count}, " +
                $"specialRoomTypes={specialRoomAssets.Count}, specialRoomSlots={specialRoomSlots.Count}");

            return new RoomTemplateSet(startRoom, rooms, specialRoomSlots);
        }

        private static void AddUniqueAsset(List<MapAsset> assets, MapAsset asset)
        {
            if (asset == null || assets.Contains(asset))
            {
                return;
            }

            assets.Add(asset);
        }

        private static void AddUniqueNonSpecialAsset(List<MapAsset> assets, List<MapAsset> specialAssets, MapAsset asset)
        {
            if (asset == null || specialAssets.Contains(asset) || assets.Contains(asset))
            {
                return;
            }

            assets.Add(asset);
        }

        private static Vector2Int CalculateCandidateOrigin(PlacedRoom parent, RoomTemplate candidateTemplate, DoorDirection direction, int roomGap)
        {
            switch (direction)
            {
                case DoorDirection.Up:
                    return new Vector2Int(parent.Definition.Origin.x, parent.Definition.Max.y + roomGap + 1 - candidateTemplate.Min.y);
                case DoorDirection.Down:
                    return new Vector2Int(parent.Definition.Origin.x, parent.Definition.Min.y - roomGap - 1 - candidateTemplate.Max.y);
                case DoorDirection.Left:
                    return new Vector2Int(parent.Definition.Min.x - roomGap - 1 - candidateTemplate.Max.x, parent.Definition.Origin.y);
                default:
                    return new Vector2Int(parent.Definition.Max.x + roomGap + 1 - candidateTemplate.Min.x, parent.Definition.Origin.y);
            }
        }

        private static Dictionary<int, RoomTemplate> CreateSpecialRoomSlotMap(int roomCount, List<RoomTemplate> specialRoomSlots, System.Random random)
        {
            Dictionary<int, RoomTemplate> slotsByRoomId = new Dictionary<int, RoomTemplate>();
            if (specialRoomSlots.Count <= 0)
            {
                return slotsByRoomId;
            }

            List<int> roomIds = new List<int>();
            for (int id = 1; id < roomCount; id++)
            {
                roomIds.Add(id);
            }

            Shuffle(roomIds, random);
            List<RoomTemplate> slots = new List<RoomTemplate>(specialRoomSlots);
            Shuffle(slots, random);

            int assignCount = Mathf.Min(roomIds.Count, slots.Count);
            for (int i = 0; i < assignCount; i++)
            {
                slotsByRoomId.Add(roomIds[i], slots[i]);
            }

            return slotsByRoomId;
        }

        private static RoomTemplate SelectRoomTemplate(RoomTemplateSet roomTemplates, Dictionary<int, RoomTemplate> specialRoomSlots, int roomId, System.Random random)
        {
            if (specialRoomSlots.TryGetValue(roomId, out RoomTemplate specialRoom))
            {
                return specialRoom;
            }

            return roomTemplates.Rooms[random.Next(roomTemplates.Rooms.Count)];
        }

        private static RoomTemplate SelectFallbackRoomTemplate(RoomTemplateSet roomTemplates, Dictionary<int, RoomTemplate> specialRoomSlots, int roomIndex)
        {
            if (specialRoomSlots.TryGetValue(roomIndex, out RoomTemplate specialRoom))
            {
                return specialRoom;
            }

            return roomTemplates.Rooms[(roomIndex - 1) % roomTemplates.Rooms.Count];
        }

        private static void Shuffle<T>(List<T> values, System.Random random)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                T value = values[i];
                values[i] = values[swapIndex];
                values[swapIndex] = value;
            }
        }

        private static int CountSpecialRooms(IReadOnlyList<PlacedRoom> rooms)
        {
            int count = 0;
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Definition.IsSpecialRoom)
                {
                    count++;
                }
            }

            return count;
        }

        private static RoomTemplate CreateRoomTemplate(MapAsset asset, bool specialRoom)
        {
            List<TemplateCell> cells = new List<TemplateCell>();
            List<TemplateDoor> doors = new List<TemplateDoor>();
            List<TemplateEnemySpawn> enemySpawns = new List<TemplateEnemySpawn>();
            List<TemplateWeaponSpawn> weaponSpawns = new List<TemplateWeaponSpawn>();
            Vector2Int origin = asset != null ? asset.PlayerSpawn : Vector2Int.zero;
            int ignoredDoors = 0;

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
                    cells.Add(new TemplateCell(position));
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
                        ignoredDoors++;
                        continue;
                    }

                    doors.Add(new TemplateDoor(position, door.Direction));
                }

                for (int i = 0; i < asset.EnemySpawns.Count; i++)
                {
                    EnemySpawnDefinition enemySpawn = asset.EnemySpawns[i];
                    if (enemySpawn == null || enemySpawn.EnemyDefinition == null)
                    {
                        continue;
                    }

                    enemySpawns.Add(new TemplateEnemySpawn(enemySpawn.EnemyDefinition, enemySpawn.Position - origin));
                }

                for (int i = 0; i < asset.WeaponSpawns.Count; i++)
                {
                    WeaponSpawnDefinition weaponSpawn = asset.WeaponSpawns[i];
                    if (weaponSpawn == null)
                    {
                        continue;
                    }

                    weaponSpawns.Add(new TemplateWeaponSpawn(weaponSpawn.Weapon, weaponSpawn.Position - origin));
                }

            }

            if (cells.Count == 0)
            {
                Debug.LogWarning($"[MapGenerator] MapAsset has no walkable cells. Using default room template. asset={DescribeAsset(asset)}");
                for (int x = -3; x <= 3; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        cells.Add(new TemplateCell(new Vector2Int(x, y)));
                    }
                }
            }

            if (doors.Count == 0)
            {
                Debug.LogWarning($"[MapGenerator] Room template has no valid doors. Adding default doors. asset={DescribeAsset(asset)}, ignoredDoors={ignoredDoors}");
                AddDefaultDoors(cells, doors);
            }
            else if (ignoredDoors > 0)
            {
                Debug.LogWarning($"[MapGenerator] Ignored doors outside walkable cells. asset={DescribeAsset(asset)}, ignoredDoors={ignoredDoors}, validDoors={doors.Count}");
            }

            return new RoomTemplate(cells, doors, enemySpawns, weaponSpawns, specialRoom);
        }

        private static PlacedRoom CreatePlacedRoom(int id, RoomTemplate template, Vector2Int origin, Vector2Int gridPosition)
        {
            DungeonRoomDefinition definition = new DungeonRoomDefinition
            {
                Id = id,
                Origin = origin,
                Min = template.Min + origin,
                Max = template.Max + origin,
                IsSpecialRoom = template.IsSpecialRoom,
            };

            HashSet<Vector2Int> cellSet = new HashSet<Vector2Int>();
            List<DungeonDoorDefinition> doorCandidates = new List<DungeonDoorDefinition>();
            List<EnemySpawnDefinition> enemySpawns = new List<EnemySpawnDefinition>();
            List<WeaponSpawnDefinition> weaponSpawns = new List<WeaponSpawnDefinition>();
            for (int i = 0; i < template.Cells.Count; i++)
            {
                TemplateCell templateCell = template.Cells[i];
                Vector2Int cell = templateCell.Position + origin;
                definition.Cells.Add(cell);
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

            for (int i = 0; i < template.EnemySpawns.Count; i++)
            {
                TemplateEnemySpawn templateEnemySpawn = template.EnemySpawns[i];
                enemySpawns.Add(new EnemySpawnDefinition
                {
                    EnemyDefinition = templateEnemySpawn.EnemyDefinition,
                    Position = templateEnemySpawn.Position + origin,
                });
            }

            for (int i = 0; i < template.WeaponSpawns.Count; i++)
            {
                TemplateWeaponSpawn templateWeaponSpawn = template.WeaponSpawns[i];
                weaponSpawns.Add(new WeaponSpawnDefinition
                {
                    Weapon = templateWeaponSpawn.Weapon,
                    Position = templateWeaponSpawn.Position + origin,
                });
            }

            return new PlacedRoom(id, gridPosition, definition, cellSet, doorCandidates, enemySpawns, weaponSpawns);
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
            foreach (Vector2Int cell in room.CellSet)
            {
                AddWalkableCell(map, cell);
                occupiedRoomCells.Add(cell);
            }

            for (int i = 0; i < room.EnemySpawns.Count; i++)
            {
                map.EnemySpawns.Add(room.EnemySpawns[i]);
            }

            for (int i = 0; i < room.WeaponSpawns.Count; i++)
            {
                map.WeaponSpawns.Add(room.WeaponSpawns[i]);
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
                AddWalkableCell(map, cell);
            }

            map.Corridors.Add(corridor);
        }

        private static void ApplyRunMarkers(MapDefinition map, IReadOnlyList<PlacedRoom> rooms)
        {
            map.PlayerSpawn = Vector2Int.zero;
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

        private static void AddRoom(MapDefinition map, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    AddWalkableCell(map, new Vector2Int(x, y));
                }
            }
        }

        private static void AddWalkableCell(MapDefinition map, Vector2Int cell)
        {
            if (!map.WalkableCells.Contains(cell))
            {
                map.WalkableCells.Add(cell);
            }
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
                FloorExitPosition = asset.FloorExitPosition,
                DungeonEntrancePosition = asset.DungeonEntrancePosition,
            };

            for (int i = 0; i < asset.WeaponSpawns.Count; i++)
            {
                WeaponSpawnDefinition weaponSpawn = asset.WeaponSpawns[i];
                if (weaponSpawn == null)
                {
                    continue;
                }

                map.WeaponSpawns.Add(new WeaponSpawnDefinition
                {
                    Weapon = weaponSpawn.Weapon,
                    Position = weaponSpawn.Position,
                });
            }

            for (int i = 0; i < asset.EnemySpawns.Count; i++)
            {
                EnemySpawnDefinition enemySpawn = asset.EnemySpawns[i];
                if (enemySpawn == null || enemySpawn.EnemyDefinition == null)
                {
                    continue;
                }

                map.EnemySpawns.Add(new EnemySpawnDefinition
                {
                    EnemyDefinition = enemySpawn.EnemyDefinition,
                    Position = enemySpawn.Position,
                });
            }

            for (int i = 0; i < asset.Cells.Count; i++)
            {
                MapCellData cell = asset.Cells[i];
                if (cell == null || !cell.Walkable)
                {
                    continue;
                }

                AddWalkableCell(map, cell.Position);
            }

            return map.WalkableCells.Count > 0;
        }

        private static string DescribeAsset(MapAsset asset)
        {
            if (asset == null)
            {
                return "null";
            }

            return $"{asset.name}(cells={asset.Cells.Count}, doors={asset.Doors.Count}, spawn={asset.PlayerSpawn})";
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
            public readonly List<TemplateEnemySpawn> EnemySpawns;
            public readonly List<TemplateWeaponSpawn> WeaponSpawns;
            public readonly Vector2Int Min;
            public readonly Vector2Int Max;
            public readonly int Width;
            public readonly int Height;
            public readonly bool IsSpecialRoom;

            public RoomTemplate(
                List<TemplateCell> cells,
                List<TemplateDoor> doors,
                List<TemplateEnemySpawn> enemySpawns,
                List<TemplateWeaponSpawn> weaponSpawns,
                bool isSpecialRoom)
            {
                Cells = cells;
                Doors = doors;
                EnemySpawns = enemySpawns;
                WeaponSpawns = weaponSpawns;
                IsSpecialRoom = isSpecialRoom;
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

        private sealed class RoomTemplateSet
        {
            public readonly RoomTemplate StartRoom;
            public readonly List<RoomTemplate> Rooms;
            public readonly List<RoomTemplate> SpecialRoomSlots;

            public RoomTemplateSet(RoomTemplate startRoom, List<RoomTemplate> rooms, List<RoomTemplate> specialRoomSlots)
            {
                StartRoom = startRoom;
                Rooms = rooms;
                SpecialRoomSlots = specialRoomSlots;
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

            public TemplateCell(Vector2Int position)
            {
                Position = position;
            }
        }

        private readonly struct TemplateEnemySpawn
        {
            public readonly EnemyDefinition EnemyDefinition;
            public readonly Vector2Int Position;

            public TemplateEnemySpawn(EnemyDefinition enemyDefinition, Vector2Int position)
            {
                EnemyDefinition = enemyDefinition;
                Position = position;
            }
        }

        private readonly struct TemplateWeaponSpawn
        {
            public readonly WeaponDefinition Weapon;
            public readonly Vector2Int Position;

            public TemplateWeaponSpawn(WeaponDefinition weapon, Vector2Int position)
            {
                Weapon = weapon;
                Position = position;
            }
        }

        private sealed class PlacedRoom
        {
            public readonly int Id;
            public readonly Vector2Int GridPosition;
            public readonly DungeonRoomDefinition Definition;
            public readonly HashSet<Vector2Int> CellSet;
            public readonly List<DungeonDoorDefinition> DoorCandidates;
            public readonly List<EnemySpawnDefinition> EnemySpawns;
            public readonly List<WeaponSpawnDefinition> WeaponSpawns;

            public PlacedRoom(
                int id,
                Vector2Int gridPosition,
                DungeonRoomDefinition definition,
                HashSet<Vector2Int> cellSet,
                List<DungeonDoorDefinition> doorCandidates,
                List<EnemySpawnDefinition> enemySpawns,
                List<WeaponSpawnDefinition> weaponSpawns)
            {
                Id = id;
                GridPosition = gridPosition;
                Definition = definition;
                CellSet = cellSet;
                DoorCandidates = doorCandidates;
                EnemySpawns = enemySpawns;
                WeaponSpawns = weaponSpawns;
            }
        }

        private readonly struct DungeonGenerationSettings
        {
            public readonly int Floor;
            public readonly int MinimumRoomCount;
            public readonly int RoomGap;
            public readonly int PlacementAttempts;

            private DungeonGenerationSettings(int floor, int minimumRoomCount, int specialRoomSlotCount, int roomGap, int placementAttempts)
            {
                Floor = floor;
                MinimumRoomCount = Mathf.Max(6, Mathf.Max(minimumRoomCount, Mathf.Max(0, specialRoomSlotCount) + 1));
                RoomGap = Mathf.Max(1, roomGap);
                PlacementAttempts = Mathf.Max(MinimumRoomCount, placementAttempts);
            }

            public static DungeonGenerationSettings From(RunFloorDefinition floorDefinition, int floor, int specialRoomSlotCount)
            {
                if (floorDefinition == null)
                {
                    return new DungeonGenerationSettings(floor, 6, specialRoomSlotCount, 5, 300);
                }

                return new DungeonGenerationSettings(
                    floor,
                    floorDefinition.MinimumRoomCount,
                    specialRoomSlotCount,
                    floorDefinition.RoomGap,
                    floorDefinition.PlacementAttempts);
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

using System.Collections.Generic;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    [CreateAssetMenu(fileName = "MapAsset", menuName = "Arkeum/Map Asset")]
    public sealed class MapAsset : ScriptableObject
    {
        public Vector2Int EditorMin = new Vector2Int(-8, -5);
        public Vector2Int EditorMax = new Vector2Int(16, 5);
        public Vector2Int PlayerSpawn;
        public Vector2Int FloorExitPosition;
        public Vector2Int DungeonEntrancePosition;
        public List<WeaponSpawnDefinition> WeaponSpawns = new List<WeaponSpawnDefinition>();
        public List<EnemySpawnDefinition> EnemySpawns = new List<EnemySpawnDefinition>();
        public List<MapCellData> Cells = new List<MapCellData>();
        public List<MapDoorData> Doors = new List<MapDoorData>();

        public bool TryGetCell(Vector2Int position, out MapCellData cell)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Position == position)
                {
                    cell = Cells[i];
                    return true;
                }
            }

            cell = null;
            return false;
        }

        public void SetCell(Vector2Int position, bool walkable)
        {
            if (TryGetCell(position, out MapCellData cell))
            {
                cell.Walkable = walkable;
                cell.HasWall = false;
                return;
            }

            Cells.Add(new MapCellData
            {
                Position = position,
                Walkable = walkable,
            });
        }

        public void SetWall(Vector2Int position, bool hasWall)
        {
            if (TryGetCell(position, out MapCellData cell))
            {
                cell.Walkable = true;
                cell.HasWall = hasWall;
                return;
            }

            Cells.Add(new MapCellData
            {
                Position = position,
                Walkable = true,
                HasWall = hasWall,
            });
        }

        public void RemoveCell(Vector2Int position)
        {
            for (int i = Cells.Count - 1; i >= 0; i--)
            {
                if (Cells[i].Position == position)
                {
                    Cells.RemoveAt(i);
                }
            }

            RemoveDoorsAt(position);
        }

        public bool TryGetDoor(Vector2Int position, DoorDirection direction, out MapDoorData door)
        {
            for (int i = 0; i < Doors.Count; i++)
            {
                if (Doors[i].Position == position && Doors[i].Direction == direction)
                {
                    door = Doors[i];
                    return true;
                }
            }

            door = null;
            return false;
        }

        public void SetDoor(Vector2Int position, DoorDirection direction)
        {
            if (TryGetDoor(position, direction, out _))
            {
                return;
            }

            Doors.Add(new MapDoorData
            {
                Position = position,
                Direction = direction,
            });
        }

        public void RemoveDoor(Vector2Int position, DoorDirection direction)
        {
            for (int i = Doors.Count - 1; i >= 0; i--)
            {
                if (Doors[i].Position == position && Doors[i].Direction == direction)
                {
                    Doors.RemoveAt(i);
                }
            }
        }

        public void RemoveDoorsAt(Vector2Int position)
        {
            for (int i = Doors.Count - 1; i >= 0; i--)
            {
                if (Doors[i].Position == position)
                {
                    Doors.RemoveAt(i);
                }
            }
        }
    }

    [System.Serializable]
    public sealed class MapDoorData
    {
        public Vector2Int Position;
        public DoorDirection Direction;
    }
}

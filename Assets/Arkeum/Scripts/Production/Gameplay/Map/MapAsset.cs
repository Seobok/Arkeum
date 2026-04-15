using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    [CreateAssetMenu(fileName = "MapAsset", menuName = "Arkeum/Map Asset")]
    public sealed class MapAsset : ScriptableObject
    {
        public Vector2Int EditorMin = new Vector2Int(-8, -5);
        public Vector2Int EditorMax = new Vector2Int(16, 5);
        public Vector2Int PlayerSpawn;
        public Vector2Int MerchantPosition;
        public Vector2Int ReliquaryPosition;
        public Vector2Int StartAltarPosition;
        public Vector2Int UnlockAltarPosition;
        public Vector2Int UndertakerPosition;
        public List<Vector2Int> TemporaryWeaponSpawns = new List<Vector2Int>();
        public List<MapCellData> Cells = new List<MapCellData>();

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

        public void SetCell(Vector2Int position, bool walkable, int depth)
        {
            if (TryGetCell(position, out MapCellData cell))
            {
                cell.Walkable = walkable;
                cell.Depth = depth;
                return;
            }

            Cells.Add(new MapCellData
            {
                Position = position,
                Walkable = walkable,
                Depth = depth,
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
        }
    }
}

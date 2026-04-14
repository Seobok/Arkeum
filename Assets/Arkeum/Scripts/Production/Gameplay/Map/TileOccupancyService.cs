using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    public sealed class TileOccupancyService
    {
        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        public bool IsOccupied(Vector2Int cell)
        {
            return occupiedCells.Contains(cell);
        }

        public void SetOccupied(Vector2Int cell, bool occupied)
        {
            if (occupied)
            {
                occupiedCells.Add(cell);
                return;
            }

            occupiedCells.Remove(cell);
        }

        public void Clear()
        {
            occupiedCells.Clear();
        }
    }
}

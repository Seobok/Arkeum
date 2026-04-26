using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [CreateAssetMenu(fileName = "RunDefinition", menuName = "Arkeum/Run Definition")]
    public sealed class RunDefinition : ScriptableObject
    {
        [SerializeField] private List<RunFloorDefinition> floors = new List<RunFloorDefinition>();

        public IReadOnlyList<RunFloorDefinition> Floors => floors;

        public RunFloorDefinition GetFloor(int floorIndex)
        {
            for (int i = 0; i < floors.Count; i++)
            {
                RunFloorDefinition floor = floors[i];
                if (floor != null && floor.FloorIndex == floorIndex)
                {
                    Debug.Log($"[RunDefinition] Found floor. requestedFloor={floorIndex}, configuredFloorCount={floors.Count}");
                    return floor;
                }
            }

            Debug.LogWarning(
                $"[RunDefinition] FloorIndex {floorIndex} was not found. " +
                $"Run-specific floor settings will not be applied. configuredFloorCount={floors.Count}");

            return null;
        }
    }
}

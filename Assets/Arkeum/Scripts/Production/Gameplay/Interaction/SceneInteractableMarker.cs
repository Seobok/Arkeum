using UnityEngine;

namespace Arkeum.Production.Gameplay.Interaction
{
    public sealed class SceneInteractableMarker : MonoBehaviour
    {
        [SerializeField] private InteractableType interactableType = InteractableType.None;
        [SerializeField] private bool useInHub = true;
        [SerializeField] private bool useInRun = true;

        public InteractableType InteractableType => interactableType;
        public bool UseInHub => useInHub;
        public bool UseInRun => useInRun;
        public Vector2Int GridPosition => Vector2Int.RoundToInt(transform.position);
    }
}

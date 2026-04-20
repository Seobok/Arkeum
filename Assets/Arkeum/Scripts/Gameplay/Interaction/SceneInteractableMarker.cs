using UnityEngine;

namespace Arkeum.Production.Gameplay.Interaction
{
    public sealed class SceneInteractableMarker : MonoBehaviour
    {
        [SerializeField] private InteractableType interactableType = InteractableType.None;

        public InteractableType InteractableType => interactableType;
        public Vector2Int GridPosition => Vector2Int.RoundToInt(transform.position);
    }
}

using UnityEngine;

namespace Arkeum.Production.Gameplay.Interaction
{
    public sealed class SceneInteractableMarker : MonoBehaviour
    {
        //TODO :: 현재는 HUB/RUN 의 이분법으로 구조가 작성되어있지만 추후 변경되어야함.
        [SerializeField] private InteractableType interactableType = InteractableType.None;
        [SerializeField] private bool useInHub = true;
        [SerializeField] private bool useInRun = true;

        public InteractableType InteractableType => interactableType;
        public bool UseInHub => useInHub;
        public bool UseInRun => useInRun;
        public Vector2Int GridPosition => Vector2Int.RoundToInt(transform.position);
    }
}

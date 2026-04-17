using Arkeum.Production.Gameplay.Actors;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Interaction
{
    public interface IInteractable
    {
        InteractableType InteractableType { get; }
        Vector2Int GridPosition { get; }

        void Interact(ActorEntity actor);
    }
}

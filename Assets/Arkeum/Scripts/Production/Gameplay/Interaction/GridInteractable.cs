using System;
using Arkeum.Production.Gameplay.Actors;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Interaction
{
    public sealed class GridInteractable : IInteractable
    {
        private readonly Action<ActorEntity> callback;

        public InteractableType InteractableType { get; }
        public Vector2Int GridPosition { get; }

        public GridInteractable(InteractableType interactableType, Vector2Int gridPosition, Action<ActorEntity> callback)
        {
            InteractableType = interactableType;
            GridPosition = gridPosition;
            this.callback = callback;
        }

        public void Interact(ActorEntity actor)
        {
            callback?.Invoke(actor);
        }
    }
}

using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Interaction
{
    public sealed class InteractionSystem
    {
        private readonly InteractionResolver interactionResolver;
        private readonly List<IInteractable> interactables = new List<IInteractable>();

        public InteractionSystem(InteractionResolver interactionResolver)
        {
            this.interactionResolver = interactionResolver;
        }

        public void SetInteractables(IEnumerable<IInteractable> sources)
        {
            interactables.Clear();
            if (sources == null)
            {
                return;
            }

            interactables.AddRange(sources);
        }

        public bool TryInteract(Vector2Int targetCell, ActorEntity actor)
        {
            if (TryGetInteractableAt(targetCell, out IInteractable interactable))
            {
                return interactionResolver.Resolve(interactable, actor);
            }

            return false;
        }

        public InteractionResolution ResolveRunInteractionAt(
            Vector2Int targetCell,
            ActorEntity actor,
            RunState runState,
            MapDefinition mapDefinition)
        {
            TryGetInteractableAt(targetCell, out IInteractable interactable);
            return interactionResolver.ResolveRunInteractionAt(targetCell, interactable, actor, runState, mapDefinition);
        }

        public bool TryGetInteractableAt(Vector2Int targetCell, out IInteractable interactableAtCell)
        {
            for (int i = 0; i < interactables.Count; i++)
            {
                IInteractable interactable = interactables[i];
                if (interactable.GridPosition == targetCell)
                {
                    interactableAtCell = interactable;
                    return true;
                }
            }

            interactableAtCell = null;
            return false;
        }
    }
}

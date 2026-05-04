using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Interaction
{
    public sealed class InteractionResolver
    {
        public bool Resolve(IInteractable interactable, ActorEntity actor)
        {
            if (interactable == null)
            {
                return false;
            }

            interactable.Interact(actor);
            return true;
        }

        public InteractionResolution ResolveRunInteractionAt(
            Vector2Int targetCell,
            IInteractable interactable,
            ActorEntity actor,
            RunState runState,
            MapDefinition mapDefinition)
        {
            if (runState == null)
            {
                return InteractionResolution.Unhandled;
            }

            InteractableType interactableType = interactable?.InteractableType ?? InteractableType.None;
            if (interactableType == InteractableType.None && mapDefinition != null)
            {
                if (mapDefinition.FloorExitPosition != Vector2Int.zero &&
                    targetCell == mapDefinition.FloorExitPosition)
                {
                    interactableType = InteractableType.FloorExit;
                }
            }

            switch (interactableType)
            {
                case InteractableType.FloorExit:
                    return TryUseFloorExit(runState);
                case InteractableType.None:
                    return InteractionResolution.Unhandled;
                default:
                    return Resolve(interactable, actor)
                        ? InteractionResolution.HandledWithoutTurn()
                        : InteractionResolution.Unhandled;
            }
        }

        private static InteractionResolution TryUseFloorExit(RunState runState)
        {
            if (runState.FloorExitUsed)
            {
                return InteractionResolution.Unhandled;
            }

            runState.FloorExitUsed = true;
            return new InteractionResolution(true, false, "You clear the floor.", RunEndReason.FloorClear);
        }
    }
}

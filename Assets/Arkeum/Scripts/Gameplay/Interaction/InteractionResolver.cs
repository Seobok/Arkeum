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
                if (targetCell == mapDefinition.MerchantPosition)
                {
                    interactableType = InteractableType.Merchant;
                }
                else if (targetCell == mapDefinition.ReliquaryPosition)
                {
                    interactableType = InteractableType.Reliquary;
                }
            }

            switch (interactableType)
            {
                case InteractableType.Merchant:
                    return TryBuyDraught(runState);
                case InteractableType.Reliquary:
                    return TryClaimReliquary(runState);
                case InteractableType.None:
                    return InteractionResolution.Unhandled;
                default:
                    return Resolve(interactable, actor)
                        ? InteractionResolution.HandledWithoutTurn()
                        : InteractionResolution.Unhandled;
            }
        }

        private static InteractionResolution TryBuyDraught(RunState runState)
        {
            if (runState.BloodShards < 3)
            {
                return InteractionResolution.HandledWithoutTurn("You do not have enough blood shards.");
            }

            if (runState.DraughtStock <= 0)
            {
                return InteractionResolution.HandledWithoutTurn("The merchant has nothing left to sell.");
            }

            runState.BloodShards -= 3;
            runState.DraughtCount += 1;
            runState.DraughtStock -= 1;
            return new InteractionResolution(true, true, "You buy a healing draught for this run.", RunEndReason.None);
        }

        private static InteractionResolution TryClaimReliquary(RunState runState)
        {
            if (runState.ReliquaryClaimed)
            {
                return InteractionResolution.Unhandled;
            }

            runState.ReliquaryClaimed = true;
            return new InteractionResolution(true, false, "You recover the reacting reliquary light.", RunEndReason.DepthClear);
        }
    }
}

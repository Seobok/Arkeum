using Arkeum.Production.Gameplay.Actors;

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
    }
}

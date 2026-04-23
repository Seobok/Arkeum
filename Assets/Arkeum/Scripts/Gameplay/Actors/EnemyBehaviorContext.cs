using Arkeum.Production.Gameplay.Combat;
using Arkeum.Production.Gameplay.Map;

namespace Arkeum.Production.Gameplay.Actors
{
    public sealed class EnemyBehaviorContext
    {
        public EnemyBehaviorContext(
            ActorEntity enemy,
            ActorEntity player,
            MapService mapService,
            ActorRepository actorRepository,
            EnemyBehaviorActions actions)
        {
            Enemy = enemy;
            Player = player;
            MapService = mapService;
            ActorRepository = actorRepository;
            Actions = actions;
        }

        public ActorEntity Enemy { get; }
        public ActorEntity Player { get; }
        public MapService MapService { get; }
        public ActorRepository ActorRepository { get; }
        public EnemyBehaviorActions Actions { get; }
    }
}

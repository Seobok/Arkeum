using Arkeum.Production.Gameplay.Combat;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Interaction;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using Arkeum.Production.Infrastructure.Input;
using Arkeum.Production.Presentation.UI;
using Arkeum.Production.Presentation.World;
using UnityEngine;

namespace Arkeum.Production.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameDirector))]
    [RequireComponent(typeof(WorldPresenter))]
    [RequireComponent(typeof(HudPresenter))]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameDirector gameDirector;
        [SerializeField] private WorldPresenter worldPresenter;
        [SerializeField] private HudPresenter hudPresenter;
        [Header("Map Assets")]
        [SerializeField] private MapAsset runMapAsset;
        [SerializeField] private MapAsset hubMapAsset;
        [SerializeField] private RunDefinition runDefinition;

        private void Reset()
        {
            gameDirector = GetComponent<GameDirector>();
            worldPresenter = GetComponent<WorldPresenter>();
            hudPresenter = GetComponent<HudPresenter>();
        }

        private void Awake()
        {
            if (gameDirector == null)
            {
                gameDirector = GetComponent<GameDirector>();
            }

            if (worldPresenter == null)
            {
                worldPresenter = GetComponent<WorldPresenter>();
            }

            if (hudPresenter == null)
            {
                hudPresenter = GetComponent<HudPresenter>();
            }

            ServiceRegistry services = BuildServices();
            worldPresenter.Initialize();
            hudPresenter.Initialize(gameDirector);
            SaveProfile profile = new SaveProfile();
            gameDirector.Initialize(services, profile);
        }

        private ServiceRegistry BuildServices()
        {
            InputReader inputReader = new InputReader();
            TurnSystem turnSystem = new TurnSystem();
            DamageResolver damageResolver = new DamageResolver();
            CombatSystem combatSystem = new CombatSystem(damageResolver);
            TargetingService targetingService = new TargetingService();
            EnemyTurnSystem enemyTurnSystem = new EnemyTurnSystem(combatSystem, targetingService);
            ActorRepository actorRepository = new ActorRepository();
            InteractionResolver interactionResolver = new InteractionResolver();
            InteractionSystem interactionSystem = new InteractionSystem(interactionResolver);
            TileOccupancyService tileOccupancyService = new TileOccupancyService();
            MapGenerator mapGenerator = new MapGenerator(runMapAsset, hubMapAsset, runDefinition);
            MapService mapService = new MapService(mapGenerator, tileOccupancyService);
            UnlockService unlockService = new UnlockService();
            QuestService questService = new QuestService();
            ProgressionService progressionService = new ProgressionService(unlockService, questService);
            RunResultBuilder runResultBuilder = new RunResultBuilder();

            return new ServiceRegistry(
                inputReader,
                turnSystem,
                combatSystem,
                enemyTurnSystem,
                actorRepository,
                interactionSystem,
                mapService,
                progressionService,
                runResultBuilder,
                worldPresenter,
                hudPresenter);
        }
    }
}

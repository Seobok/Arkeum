using Arkeum.Production.Gameplay.Combat;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Interaction;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using Arkeum.Production.Gameplay.Timing;
using Arkeum.Production.Infrastructure.Input;
using Arkeum.Production.Presentation.Audio;
using Arkeum.Production.Presentation.UI;
using Arkeum.Production.Presentation.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Production.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameDirector))]
    [RequireComponent(typeof(WorldPresenter))]
    [RequireComponent(typeof(HudPresenter))]
    [RequireComponent(typeof(TimingPopupPresenter))]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameDirector gameDirector;
        [SerializeField] private WorldPresenter worldPresenter;
        [SerializeField] private HudPresenter hudPresenter;
        [SerializeField] private TimingPopupPresenter timingPopupPresenter;
        [SerializeField] private InputActionAsset inputActions;
        [Header("Map Assets")]
        [SerializeField] private MapAsset hubMapAsset;
        [SerializeField] private RunDefinition runDefinition;

        private void Reset()
        {
            gameDirector = GetComponent<GameDirector>();
            worldPresenter = GetComponent<WorldPresenter>();
            hudPresenter = GetComponent<HudPresenter>();
            timingPopupPresenter = GetComponent<TimingPopupPresenter>();
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

            if (timingPopupPresenter == null)
            {
                timingPopupPresenter = GetComponent<TimingPopupPresenter>();
            }

            if (timingPopupPresenter == null)
            {
                Debug.LogError("[GameBootstrap] TimingPopupPresenter is missing. Add it to GameRoot and wire its UGUI references in the Inspector.", this);
                enabled = false;
                return;
            }

            //씬 오브젝트 초기화
            worldPresenter.Initialize();

            //HUD UI 초기화
            hudPresenter.Initialize(gameDirector);

            //타이밍 기능 팝업 관련 UI 초기화
            timingPopupPresenter.Initialize();

            //게임 디렉터 초기화 (게임 진입)
            ServiceRegistry services = BuildServices();
            SaveProfile profile = new SaveProfile();
            gameDirector.Initialize(services, profile);
        }

        private ServiceRegistry BuildServices()
        {
            InputReader inputReader = new InputReader(inputActions);
            TurnSystem turnSystem = new TurnSystem();
            DamageResolver damageResolver = new DamageResolver();
            CombatSystem combatSystem = new CombatSystem(damageResolver);
            TargetingService targetingService = new TargetingService();
            EnemyTurnSystem enemyTurnSystem = new EnemyTurnSystem(combatSystem, targetingService);
            ActorRepository actorRepository = new ActorRepository();
            InteractionResolver interactionResolver = new InteractionResolver();
            InteractionSystem interactionSystem = new InteractionSystem(interactionResolver);
            TileOccupancyService tileOccupancyService = new TileOccupancyService();
            MapGenerator mapGenerator = new MapGenerator(hubMapAsset, runDefinition);
            MapService mapService = new MapService(mapGenerator, tileOccupancyService);
            QuestService questService = new QuestService();
            ProgressionService progressionService = new ProgressionService(questService);
            RunResultBuilder runResultBuilder = new RunResultBuilder();
            TimingService timingService = new TimingService();
            AudioCueService audioCueService = new AudioCueService();

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
                timingService,
                runDefinition,
                audioCueService,
                worldPresenter,
                hudPresenter,
                timingPopupPresenter);
        }
    }
}

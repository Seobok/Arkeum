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

namespace Arkeum.Production.Core
{
    public sealed class ServiceRegistry
    {
        public InputReader InputReader { get; }
        public TurnSystem TurnSystem { get; }
        public CombatSystem CombatSystem { get; }
        public EnemyTurnSystem EnemyTurnSystem { get; }
        public ActorRepository ActorRepository { get; }
        public InteractionSystem InteractionSystem { get; }
        public MapService MapService { get; }
        public ProgressionService ProgressionService { get; }
        public RunResultBuilder RunResultBuilder { get; }
        public TimingService TimingService { get; }
        public RunDefinition RunDefinition { get; }
        public AudioCueService AudioCueService { get; }
        public WorldPresenter WorldPresenter { get; }
        public HudPresenter HudPresenter { get; }
        public TimingPopupPresenter TimingPopupPresenter { get; }

        public ServiceRegistry(
            InputReader inputReader,
            TurnSystem turnSystem,
            CombatSystem combatSystem,
            EnemyTurnSystem enemyTurnSystem,
            ActorRepository actorRepository,
            InteractionSystem interactionSystem,
            MapService mapService,
            ProgressionService progressionService,
            RunResultBuilder runResultBuilder,
            TimingService timingService,
            RunDefinition runDefinition,
            AudioCueService audioCueService,
            WorldPresenter worldPresenter,
            HudPresenter hudPresenter,
            TimingPopupPresenter timingPopupPresenter)
        {
            InputReader = inputReader;
            TurnSystem = turnSystem;
            CombatSystem = combatSystem;
            EnemyTurnSystem = enemyTurnSystem;
            ActorRepository = actorRepository;
            InteractionSystem = interactionSystem;
            MapService = mapService;
            ProgressionService = progressionService;
            RunResultBuilder = runResultBuilder;
            TimingService = timingService;
            RunDefinition = runDefinition;
            AudioCueService = audioCueService;
            WorldPresenter = worldPresenter;
            HudPresenter = hudPresenter;
            TimingPopupPresenter = timingPopupPresenter;
        }
    }
}

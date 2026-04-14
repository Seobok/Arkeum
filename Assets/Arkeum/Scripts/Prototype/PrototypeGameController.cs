using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed partial class PrototypeGameController : MonoBehaviour
    {
        public const int StartingBandageUnlockCost = 3;

        private readonly List<ActorRuntime> enemies = new List<ActorRuntime>();
        private readonly List<string> lostResultLines = new List<string>();
        private readonly List<string> keptResultLines = new List<string>();
        private readonly List<GameObject> spawnedViews = new List<GameObject>();
        private readonly Queue<string> undertakerLines = new Queue<string>();

        private PrototypeSaveService saveService;
        private PrototypeViewFactory viewFactory;
        private PrototypeHud hud;
        private PrototypeInputReader inputReader;
        private PrototypeCombatSystem combatSystem;
        private PrototypeProgressionService progressionService;
        private PrototypeLayoutFactory layoutFactory;

        private DungeonLayout layout;
        private DungeonLayout hubLayout;
        private Camera mainCamera;
        private Transform worldRoot;
        private Transform floorRoot;
        private Transform actorRoot;
        private Transform markerRoot;

        private ActorRuntime player;
        private GameObject hubPlayerView;
        private Vector2Int hubPlayerPosition;
        private Vector2Int hubStartGatePosition;
        private Vector2Int hubUnlockPosition;
        private Vector2Int hubUndertakerPosition;
        private bool reliquaryClaimed;
        private bool temporaryWeaponCollected;
        private int draughtStock;

        public GamePhase Phase { get; private set; }
        public ProfileSaveData Profile { get; private set; }
        public RunState Run { get; private set; }

        private void Awake()
        {
            saveService = new PrototypeSaveService();
            viewFactory = new PrototypeViewFactory();
            hud = new PrototypeHud();
            inputReader = new PrototypeInputReader();
            combatSystem = new PrototypeCombatSystem();
            progressionService = new PrototypeProgressionService();
            layoutFactory = new PrototypeLayoutFactory();
            Profile = saveService.LoadProfile();
            Phase = GamePhase.Hub;

            EnsureCamera();
            BuildWorldRoots();
            SeedDialogue();
            EnterHub("The return altar receives you once more.");
        }

        private void Update()
        {
            switch (Phase)
            {
                case GamePhase.Hub:
                    UpdateHubInput();
                    break;
                case GamePhase.InRun:
                    UpdateRunInput();
                    UpdateCamera();
                    break;
                case GamePhase.RunResult:
                    UpdateRunResultInput();
                    break;
            }
        }

        private void OnGUI()
        {
            hud.Draw(this);
        }

        public string GetHubSummary()
        {
            string questState = Profile.mq01Completed ? "Completed" : "In Progress";
            return $"MQ-01: {questState}\nSave Path: {saveService.GetProfilePath()}";
        }

        public string GetRunResultHeadline()
        {
            return Run.EndReason == RunEndReason.Death
                ? "You fell in the ash corridor."
                : "You recovered the reliquary light and returned to the altar.";
        }

        public IReadOnlyList<string> GetLostResultLines()
        {
            return lostResultLines;
        }

        public IReadOnlyList<string> GetKeptResultLines()
        {
            return keptResultLines;
        }
    }
}

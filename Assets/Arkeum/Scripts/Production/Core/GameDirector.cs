using System.Collections.Generic;
using System.Linq;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Interaction;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Production.Core
{
    public sealed class GameDirector : MonoBehaviour
    {
        private const int StartingBandageUnlockCost = 3;

        [SerializeField] private GameState startingState = GameState.Hub;

        private readonly Queue<string> undertakerLines = new Queue<string>();
        private readonly List<string> lostResultLines = new List<string>();
        private readonly List<string> keptResultLines = new List<string>();

        private Vector2Int hubPlayerPosition;

        public GameState CurrentState { get; private set; }
        public ServiceRegistry Services { get; private set; }
        public RunController CurrentRunController { get; private set; }
        public SaveProfile ActiveProfile { get; private set; }

        private void Update()
        {
            if (Services == null)
            {
                return;
            }

            switch (CurrentState)
            {
                case GameState.Hub:
                    UpdateHubInput();
                    break;
                case GameState.InRun:
                    UpdateRunInput();
                    break;
                case GameState.RunResult:
                    UpdateRunResultInput();
                    break;
            }
        }

        public void Initialize(ServiceRegistry services, SaveProfile profile)
        {
            Services = services;
            ActiveProfile = profile;
            CurrentState = startingState;
            SeedDialogue();

            switch(startingState)
            {
                case GameState.Hub:
                    EnterHub("The return altar receives you once more.");
                    break;
                case GameState.InRun:
                    StartRun();
                    break;
                case GameState.RunResult: 
                    break;
            }
        }

        public void EnterHub(string message = null)
        {
            Services.MapService.LoadHubMap();
            ApplySceneInteractablePositions(isHub: true);
            hubPlayerPosition = Services.MapService.CurrentMap.PlayerSpawn;
            BuildHubInteractables();
            Services.WorldPresenter.BindHub(Services.MapService.CurrentMap, hubPlayerPosition);
            Services.WorldPresenter.SetActorRepository(null);
            Services.WorldPresenter.Refresh();
            Services.HudPresenter.BindRun(null);
            Services.HudPresenter.ClearRunResult();
            Services.HudPresenter.SetDialogue(Services.ProgressionService.GetUndertakerGreeting(ActiveProfile));
            Services.HudPresenter.SetMessage(message ?? "The embers of the return altar flicker quietly.");
            CurrentRunController = null;
            CurrentState = GameState.Hub;
        }

        public void PrepareRun(RunController runController)
        {
            CurrentRunController = runController;
            CurrentState = GameState.RunPreparing;
        }

        public void StartRun()
        {
            Services.MapService.LoadRunMap();
            ApplySceneInteractablePositions(isHub: false);
            BuildRunActors();
            BuildRunInteractables();

            RunController runController = new RunController(
                Services.TurnSystem,
                Services.CombatSystem,
                Services.EnemyTurnSystem,
                Services.InteractionSystem,
                Services.MapService,
                Services.ActorRepository);

            RunState runState = runController.CreateRunState(ActiveProfile);
            ActorEntity player = Services.ActorRepository.Player;
            runState.Player = player;
            //TODO :: 추후에는 다른 파일에서 값을 읽어올 수 있도록, 데이터를 관리하는 파일을 생성해야 함.
            player.CurrentHp = 12;
            player.Stats.MaxHp = 12;
            player.Stats.AttackPower = runState.EffectiveAttack;
            runController.Begin(runState);

            PrepareRun(runController);
            Services.WorldPresenter.SetActorRepository(Services.ActorRepository);
            Services.WorldPresenter.BindRun(runState);
            Services.WorldPresenter.Refresh();
            Services.HudPresenter.BindRun(runState);
            Services.HudPresenter.SetDialogue(string.Empty);
            Services.HudPresenter.SetMessage("You descend into the ash corridor. Enemies react after every action.");
            CurrentState = GameState.InRun;
        }

        public void ShowRunResult()
        {
            if (CurrentRunController?.CurrentRun == null)
            {
                return;
            }

            int gleamGain = Services.ProgressionService.ApplyRunEnd(ActiveProfile, CurrentRunController.CurrentRun);
            CurrentRunController.CurrentRun.GleamReward = gleamGain;
            Services.ProgressionService.BuildResultLines(
                ActiveProfile,
                CurrentRunController.CurrentRun,
                lostResultLines,
                keptResultLines);
            Services.HudPresenter.SetRunResult(lostResultLines, keptResultLines);
            Services.HudPresenter.SetMessage(CurrentRunController.CurrentRun.EndReason == RunEndReason.Death
                ? "Death is not the end, only the start of reckoning."
                : "The recovered light returns to the altar.");
            Services.WorldPresenter.Refresh();
            CurrentState = GameState.RunResult;
        }

        private void UpdateHubInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (!Services.InputReader.TryGetMoveDirection(keyboard, out Vector2Int direction))
            {
                return;
            }

            Vector2Int target = hubPlayerPosition + direction;
            if (Services.InteractionSystem.TryInteract(target, null))
            {
                Services.WorldPresenter.UpdateHubPlayerPosition(hubPlayerPosition);
                Services.WorldPresenter.Refresh();
                return;
            }

            if (!Services.MapService.IsWalkableCell(target))
            {
                Services.HudPresenter.SetMessage("The path is blocked.");
                return;
            }

            hubPlayerPosition = target;
            Services.WorldPresenter.UpdateHubPlayerPosition(hubPlayerPosition);
            Services.WorldPresenter.Refresh();
            UpdateHubLocationMessage();
        }

        private void UpdateRunInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || CurrentRunController == null)
            {
                return;
            }

            bool handled = false;
            if (Services.InputReader.TryGetMoveDirection(keyboard, out Vector2Int direction))
            {
                handled = CurrentRunController.TryHandlePlayerAction(direction);
            }
            else if (keyboard.qKey.wasPressedThisFrame)
            {
                CurrentRunController.Wait();
                handled = true;
            }
            else if (keyboard.digit1Key.wasPressedThisFrame)
            {
                handled = CurrentRunController.UseBandage();
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                handled = CurrentRunController.UseDraught();
            }

            if (!handled)
            {
                return;
            }

            Services.WorldPresenter.Refresh();
            Services.HudPresenter.SetMessage(CurrentRunController.LastMessage);

            if (CurrentRunController.CurrentRun.EndReason != RunEndReason.None)
            {
                ShowRunResult();
            }
        }

        private void UpdateRunResultInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.enterKey.wasPressedThisFrame)
            {
                EnterHub("The echo of return fades, and you stand before the altar again.");
            }
        }

        private void BuildRunActors()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            List<ActorEntity> actors = new List<ActorEntity>
            {
                new ActorEntity
                {
                    Id = "player",
                    DisplayName = "Ash Knight",
                    GridPosition = map.PlayerSpawn,
                    CurrentHp = 12,
                    IsEnemy = false,
                    Stats = new ActorStats
                    {
                        MaxHp = 12,
                        AttackPower = 3,
                        Defense = 1,
                        ActionInterval = 1,
                    },
                },
                CreateEnemy("ash_crawler_a", "Ash Crawler", BrainType.Chaser, new Vector2Int(4, 0), 5, 2, 0, 1, 1),
                CreateEnemy("ash_crawler_b", "Ash Crawler", BrainType.Chaser, new Vector2Int(7, 1), 5, 2, 0, 1, 1),
                CreateEnemy("iron_warden", "Iron Warden", BrainType.HeavyChaser, new Vector2Int(12, 0), 9, 4, 1, 2, 2),
            };

            Services.ActorRepository.SetActors(actors);
        }

        private void BuildRunInteractables()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            SceneInteractableMarker[] sceneMarkers = FindSceneInteractableMarkers(isHub: false);
            if (sceneMarkers.Length > 0)
            {
                List<IInteractable> interactables = new List<IInteractable>();
                for (int i = 0; i < sceneMarkers.Length; i++)
                {
                    SceneInteractableMarker marker = sceneMarkers[i];
                    interactables.Add(new GridInteractable(marker.InteractableType, marker.GridPosition, _ => { }));
                }

                Services.InteractionSystem.SetInteractables(interactables);
                return;
            }

            Services.InteractionSystem.SetInteractables(new[]
            {
                new GridInteractable(InteractableType.Merchant, map.MerchantPosition, _ => { }),
                new GridInteractable(InteractableType.Reliquary, map.ReliquaryPosition, _ => { }),
            });
        }

        private void BuildHubInteractables()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            SceneInteractableMarker[] sceneMarkers = FindSceneInteractableMarkers(isHub: true);
            if (sceneMarkers.Length > 0)
            {
                List<IInteractable> interactables = new List<IInteractable>();
                for (int i = 0; i < sceneMarkers.Length; i++)
                {
                    SceneInteractableMarker marker = sceneMarkers[i];
                    interactables.Add(new GridInteractable(marker.InteractableType, marker.GridPosition, _ => HandleHubMarkerInteraction(marker.InteractableType)));
                }

                Services.InteractionSystem.SetInteractables(interactables);
                return;
            }

            Services.InteractionSystem.SetInteractables(new IInteractable[]
            {
                new GridInteractable(InteractableType.StartAltar, map.StartAltarPosition, _ => StartRun()),
                new GridInteractable(InteractableType.UnlockAltar, map.UnlockAltarPosition, _ => TryUnlockStartingBandage()),
                new GridInteractable(InteractableType.Undertaker, map.UndertakerPosition, _ => AdvanceUndertakerDialogue()),
            });
        }

        private void TryUnlockStartingBandage()
        {
            Services.ProgressionService.TryUnlockStartingBandage(ActiveProfile, StartingBandageUnlockCost, out string message);
            Services.HudPresenter.SetMessage(message);
        }

        private void AdvanceUndertakerDialogue()
        {
            if (undertakerLines.Count == 0)
            {
                SeedDialogue();
            }

            string line = undertakerLines.Dequeue();
            undertakerLines.Enqueue(line);
            Services.HudPresenter.SetDialogue(line);
            Services.HudPresenter.SetMessage("The undertaker speaks without emotion.");
        }

        private void UpdateHubLocationMessage()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            if (hubPlayerPosition == map.StartAltarPosition)
            {
                Services.HudPresenter.SetMessage("Move into the start altar to begin a run.");
                return;
            }

            if (hubPlayerPosition == map.UnlockAltarPosition)
            {
                Services.HudPresenter.SetMessage(ActiveProfile.StartingBandageUnlocked
                    ? "The bandage unlock altar is already active."
                    : $"Move into the unlock altar to spend {StartingBandageUnlockCost} gleam.");
                return;
            }

            if (hubPlayerPosition == map.UndertakerPosition)
            {
                Services.HudPresenter.SetMessage("Move into the undertaker to continue the conversation.");
                return;
            }

            Services.HudPresenter.SetMessage("The embers of the return altar flicker quietly.");
        }

        private void SeedDialogue()
        {
            Services.ProgressionService.SeedDialogue(undertakerLines);
        }

        private void ApplySceneInteractablePositions(bool isHub)
        {
            MapDefinition map = Services.MapService.CurrentMap;
            SceneInteractableMarker[] sceneMarkers = FindSceneInteractableMarkers(isHub);
            for (int i = 0; i < sceneMarkers.Length; i++)
            {
                SceneInteractableMarker marker = sceneMarkers[i];
                switch (marker.InteractableType)
                {
                    case InteractableType.StartAltar:
                        map.StartAltarPosition = marker.GridPosition;
                        break;
                    case InteractableType.UnlockAltar:
                        map.UnlockAltarPosition = marker.GridPosition;
                        break;
                    case InteractableType.Undertaker:
                        map.UndertakerPosition = marker.GridPosition;
                        break;
                    case InteractableType.Merchant:
                        map.MerchantPosition = marker.GridPosition;
                        break;
                    case InteractableType.Reliquary:
                        map.ReliquaryPosition = marker.GridPosition;
                        break;
                }

                if (!map.WalkableCells.Contains(marker.GridPosition))
                {
                    map.WalkableCells.Add(marker.GridPosition);
                }
            }
        }

        private SceneInteractableMarker[] FindSceneInteractableMarkers(bool isHub)
        {
            SceneInteractableMarker[] markers = Object.FindObjectsByType<SceneInteractableMarker>(FindObjectsSortMode.None);
            return markers
                .Where(marker => isHub ? marker.UseInHub : marker.UseInRun)
                .ToArray();
        }

        private void HandleHubMarkerInteraction(InteractableType interactableType)
        {
            switch (interactableType)
            {
                case InteractableType.StartAltar:
                    StartRun();
                    break;
                case InteractableType.UnlockAltar:
                    TryUnlockStartingBandage();
                    break;
                case InteractableType.Undertaker:
                    AdvanceUndertakerDialogue();
                    break;
            }
        }

        private static ActorEntity CreateEnemy(string id, string name, BrainType brainType, Vector2Int position, int hp, int attack, int defense, int interval, int bloodReward)
        {
            return new ActorEntity
            {
                Id = id,
                DisplayName = name,
                BrainType = brainType,
                GridPosition = position,
                CurrentHp = hp,
                IsEnemy = true,
                BloodReward = bloodReward,
                Stats = new ActorStats
                {
                    MaxHp = hp,
                    AttackPower = attack,
                    Defense = defense,
                    ActionInterval = interval,
                },
            };
        }
    }
}

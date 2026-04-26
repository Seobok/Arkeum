using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Interaction;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Core
{
    public sealed class GameDirector : MonoBehaviour
    {
        private const int StartingBandageUnlockCost = 3;
        private const int PlayerMaxHP = 12;
        private const int StartingRunFloor = 1;

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
            ApplySceneInteractablePositions();
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
            int runFloor = StartingRunFloor;
            RunFloorDefinition floorDefinition = Services.MapService.GetRunFloor(runFloor);
            Services.MapService.LoadRunFloor(floorDefinition, runFloor);
            ApplySceneInteractablePositions();
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
            runState.CurrentFloor = runFloor;
            runState.CurrentFloorDefinition = floorDefinition;
            ActorEntity player = Services.ActorRepository.Player;
            runState.Player = player;
            //TODO :: 추후에는 다른 파일에서 값을 읽어올 수 있도록, 데이터를 관리하는 파일을 생성해야 함.
            player.CurrentHp = PlayerMaxHP;
            player.Stats.MaxHp = PlayerMaxHP;
            player.Stats.AttackPower = runState.EffectiveAttack;
            runController.Begin(runState);

            PrepareRun(runController);
            Services.WorldPresenter.SetActorRepository(Services.ActorRepository);
            Services.WorldPresenter.BindRun(runState, Services.MapService.CurrentMap);
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
            if (!Services.InputReader.TryGetMoveDirection(out Vector2Int direction))
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
            if (CurrentRunController == null)
            {
                return;
            }

            bool handled = false;
            if (Services.InputReader.TryGetMoveDirection(out Vector2Int direction))
            {
                handled = CurrentRunController.TryHandlePlayerAction(direction);
            }
            else if (Services.InputReader.WasWaitPressed())
            {
                CurrentRunController.Wait();
                handled = true;
            }
            else if (Services.InputReader.WasUseBandagePressed())
            {
                handled = CurrentRunController.UseBandage();
            }
            else if (Services.InputReader.WasUseDraughtPressed())
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
            if (Services.InputReader.WasConfirmPressed())
            {
                EnterHub("The echo of return fades, and you stand before the altar again.");
            }
        }

        private void BuildRunActors()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            RunFloorDefinition floorDefinition = Services.MapService.CurrentRunFloor;
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
            };

            if (floorDefinition != null && floorDefinition.EnemySpawns.Count > 0)
            {
                for (int i = 0; i < floorDefinition.EnemySpawns.Count; i++)
                {
                    ActorEntity enemy = CreateEnemy(floorDefinition.EnemySpawns[i], i);
                    if (enemy != null)
                    {
                        actors.Add(enemy);
                    }
                }
            }
            else
            {
                Debug.Log("[GameDirector] No run enemy spawns configured. Only the player will be spawned.");
            }

            Services.ActorRepository.SetActors(actors);
        }

        private void BuildRunInteractables()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            SceneInteractableMarker[] sceneMarkers = FindSceneInteractableMarkers();
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

            List<IInteractable> runInteractables = new List<IInteractable>();
            if (map.MerchantPosition != Vector2Int.zero)
            {
                runInteractables.Add(new GridInteractable(InteractableType.Merchant, map.MerchantPosition, _ => { }));
            }

            if (map.ReliquaryPosition != Vector2Int.zero)
            {
                runInteractables.Add(new GridInteractable(InteractableType.Reliquary, map.ReliquaryPosition, _ => { }));
            }

            Services.InteractionSystem.SetInteractables(runInteractables);
        }

        private void BuildHubInteractables()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            SceneInteractableMarker[] sceneMarkers = FindSceneInteractableMarkers();
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

        private void ApplySceneInteractablePositions()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            SceneInteractableMarker[] sceneMarkers = FindSceneInteractableMarkers();
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

        private SceneInteractableMarker[] FindSceneInteractableMarkers()
        {
            return Object.FindObjectsByType<SceneInteractableMarker>(FindObjectsSortMode.None);
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

        private static ActorEntity CreateEnemy(EnemySpawnDefinition spawnDefinition, int index)
        {
            if (spawnDefinition == null || spawnDefinition.EnemyDefinition == null)
            {
                Debug.LogWarning($"[GameDirector] Skipping invalid enemy spawn at index={index}.");
                return null;
            }

            EnemyDefinition enemyDefinition = spawnDefinition.EnemyDefinition;
            ActorStats stats = enemyDefinition.Stats.Clone();
            ApplyLegacyEnemyTiming(stats);
            return new ActorEntity
            {
                Id = $"{enemyDefinition.EnemyId}_{index}",
                DisplayName = enemyDefinition.DisplayName,
                BrainType = enemyDefinition.BrainType,
                GridPosition = spawnDefinition.Position,
                FacingDirection = Vector2Int.up,
                CurrentHp = stats.MaxHp,
                IsEnemy = true,
                BloodReward = enemyDefinition.BloodReward,
                EnemyDefinition = enemyDefinition,
                Stats = stats,
            };
        }

        private static void ApplyLegacyEnemyTiming(ActorStats stats)
        {
            if (stats == null || stats.ActionInterval <= 1)
            {
                return;
            }

            int preparationTurns = stats.ActionInterval - 1;
            if (stats.AttackPreparationTurns == 0)
            {
                stats.AttackPreparationTurns = preparationTurns;
            }

            if (stats.MovePreparationTurns == 0)
            {
                stats.MovePreparationTurns = preparationTurns;
            }
        }
    }
}

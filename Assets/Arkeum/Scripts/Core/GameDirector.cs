using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Interaction;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Core
{
    public sealed class GameDirector : MonoBehaviour
    {
        private const int PlayerMaxHP = 12;
        private const int StartingRunFloor = 1;

        [SerializeField] private GameState startingState = GameState.Hub;

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
                case GameState.TimingChallenge:
                    UpdateTimingChallengeInput();
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
            Services.TimingPopupPresenter.Hide();
            Services.TimingService.CancelCurrent();
            Services.HudPresenter.ClearRunResult();
            Services.HudPresenter.SetDialogue(string.Empty);
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
                Services.ActorRepository,
                Services.TimingService,
                ActiveProfile);

            RunState runState = runController.CreateRunState(ActiveProfile, Services.RunDefinition?.StartingLoadout);
            runState.CurrentFloor = runFloor;
            runState.CurrentFloorDefinition = floorDefinition;
            ActorEntity player = Services.ActorRepository.Player;
            runState.Player = player;
            //TODO :: 추후에는 다른 파일에서 값을 읽어올 수 있도록, 데이터를 관리하는 파일을 생성해야 함.
            player.SetMaxHp(PlayerMaxHP);
            player.SetCurrentHp(PlayerMaxHP);
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

            Services.ProgressionService.ApplyRunEnd(ActiveProfile, CurrentRunController.CurrentRun);
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

            if (Services.InputReader.WasTimingTogglePressed())
            {
                CurrentRunController.ToggleTimingMode();
                Services.HudPresenter.SetMessage(CurrentRunController.LastMessage);
                return;
            }

            PlayerActionResultType actionResult = PlayerActionResultType.NotHandled;
            if (Services.InputReader.TryGetMoveDirection(out Vector2Int direction))
            {
                actionResult = CurrentRunController.TryHandlePlayerAction(direction);
            }
            else if (Services.InputReader.WasWaitPressed())
            {
                CurrentRunController.Wait();
                actionResult = PlayerActionResultType.Handled;
            }
            else if (Services.InputReader.WasUseBandagePressed())
            {
                actionResult = CurrentRunController.UseBandage()
                    ? PlayerActionResultType.Handled
                    : PlayerActionResultType.NotHandled;
            }

            if (actionResult == PlayerActionResultType.NotHandled)
            {
                return;
            }

            if (actionResult == PlayerActionResultType.TimingChallengeStarted)
            {
                Services.TimingPopupPresenter.Show(Services.TimingService.CurrentSession);
                Services.HudPresenter.SetMessage(CurrentRunController.LastMessage);
                CurrentState = GameState.TimingChallenge;
                return;
            }

            CompleteHandledRunAction();
        }

        private void UpdateTimingChallengeInput()
        {
            TimingSession session = Services.TimingService.CurrentSession;
            if (session == null)
            {
                Services.TimingPopupPresenter.Hide();
                CurrentState = GameState.InRun;
                return;
            }

            Services.TimingService.Tick(Time.deltaTime);
            ITimingChallengeRuntime runtime = session.Runtime;
            if (Services.InputReader.WasTimingActionPressed())
            {
                CompleteTimingChallenge(session, runtime.EvaluateAction());
                return;
            }

            if (runtime.IsExpired)
            {
                CompleteTimingChallenge(session, runtime.EvaluateTimeout());
            }
        }

        private void CompleteTimingChallenge(TimingSession session, TimingResultGrade grade)
        {
            TimingAttackResult result = session.BuildResult(grade);
            CurrentRunController.ResolveTimedAttack(result);
            Services.TimingPopupPresenter.Hide();
            CurrentState = GameState.InRun;
            CompleteHandledRunAction();
        }

        private void CompleteHandledRunAction()
        {
            Services.WorldPresenter.Refresh();
            Services.HudPresenter.SetMessage(CurrentRunController.LastMessage);

            if (CurrentRunController.CurrentRun.EndReason != RunEndReason.None)
            {
                if (CurrentRunController.CurrentRun.EndReason == RunEndReason.FloorClear && TryAdvanceToNextFloor())
                {
                    return;
                }

                ShowRunResult();
            }
        }

        private bool TryAdvanceToNextFloor()
        {
            RunState runState = CurrentRunController?.CurrentRun;
            if (runState == null)
            {
                return false;
            }

            int nextFloor = runState.CurrentFloor + 1;
            RunFloorDefinition nextFloorDefinition = Services.MapService.GetRunFloor(nextFloor);
            if (nextFloorDefinition == null)
            {
                return false;
            }

            int currentHp = runState.Player != null ? runState.Player.CurrentHp : PlayerMaxHP;
            Services.MapService.LoadRunFloor(nextFloorDefinition, nextFloor);
            if (Services.MapService.CurrentMap == null)
            {
                Debug.LogError($"[GameDirector] Failed to advance to floor {nextFloor}. Run floor map was not created.");
                return false;
            }

            ApplySceneInteractablePositions();
            BuildRunActors();
            BuildRunInteractables();

            ActorEntity player = Services.ActorRepository.Player;
            if (player == null)
            {
                Debug.LogError($"[GameDirector] Failed to advance to floor {nextFloor}. Player actor was not created.");
                return false;
            }

            runState.CurrentFloor = nextFloor;
            runState.CurrentFloorDefinition = nextFloorDefinition;
            runState.FloorExitUsed = false;
            runState.EndReason = RunEndReason.None;
            runState.Player = player;
            player.SetCurrentHp(currentHp);
            player.SetMaxHp(PlayerMaxHP);
            player.Stats.AttackPower = runState.EffectiveAttack;

            Services.WorldPresenter.SetActorRepository(Services.ActorRepository);
            Services.WorldPresenter.BindRun(runState, Services.MapService.CurrentMap);
            Services.WorldPresenter.Refresh();
            Services.HudPresenter.BindRun(runState);
            Services.HudPresenter.SetMessage($"You descend to floor {nextFloor}.");
            CurrentState = GameState.InRun;
            return true;
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
            ActorStats playerStats = new ActorStats
            {
                AttackPower = 3,
                Defense = 1,
            };
            playerStats.SetMaxHp(12);

            ActorEntity player = new ActorEntity
            {
                Id = "player",
                DisplayName = "Ash Knight",
                GridPosition = map.PlayerSpawn,
                IsEnemy = false,
                Stats = playerStats,
            };
            player.SetCurrentHp(12);

            List<ActorEntity> actors = new List<ActorEntity>
            {
                player,
            };

            IReadOnlyList<EnemySpawnDefinition> enemySpawns = map.EnemySpawns;

            if (enemySpawns != null && enemySpawns.Count > 0)
            {
                for (int i = 0; i < enemySpawns.Count; i++)
                {
                    ActorEntity enemy = CreateEnemy(enemySpawns[i], i);
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
            if (map.FloorExitPosition != Vector2Int.zero)
            {
                runInteractables.Add(new GridInteractable(InteractableType.FloorExit, map.FloorExitPosition, _ => { }));
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

            List<IInteractable> hubInteractables = new List<IInteractable>();
            if (IsMarkerEnabled(map.DungeonEntrancePosition))
            {
                hubInteractables.Add(new GridInteractable(InteractableType.DungeonEntrance, map.DungeonEntrancePosition, _ => StartRun()));
            }

            Services.InteractionSystem.SetInteractables(hubInteractables);
        }

        private void UpdateHubLocationMessage()
        {
            MapDefinition map = Services.MapService.CurrentMap;
            if (IsMarkerEnabled(map.DungeonEntrancePosition) && hubPlayerPosition == map.DungeonEntrancePosition)
            {
                Services.HudPresenter.SetMessage("Move into the dungeon entrance to begin a run.");
                return;
            }

            Services.HudPresenter.SetMessage("The embers of the return altar flicker quietly.");
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
                    case InteractableType.DungeonEntrance:
                        map.DungeonEntrancePosition = marker.GridPosition;
                        break;
                    case InteractableType.FloorExit:
                        map.FloorExitPosition = marker.GridPosition;
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

        private static bool IsMarkerEnabled(Vector2Int position)
        {
            return position != Vector2Int.zero;
        }

        private void HandleHubMarkerInteraction(InteractableType interactableType)
        {
            switch (interactableType)
            {
                case InteractableType.DungeonEntrance:
                    StartRun();
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
            ActorEntity enemy = new ActorEntity
            {
                Id = $"{enemyDefinition.EnemyId}_{index}",
                DisplayName = enemyDefinition.DisplayName,
                GridPosition = spawnDefinition.Position,
                FacingDirection = Vector2Int.up,
                IsEnemy = true,
                Gold = enemyDefinition.BloodReward,
                EnemyDefinition = enemyDefinition,
                Stats = stats,
            };

            enemy.SetCurrentHp(stats.MaxHp);
            return enemy;
        }
    }
}

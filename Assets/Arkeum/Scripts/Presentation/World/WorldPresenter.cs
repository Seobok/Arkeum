using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Presentation.World
{
    public sealed class WorldPresenter : MonoBehaviour
    {
        [SerializeField] private WorldVisualSet visualSet;

        private readonly List<GameObject> spawnedViews = new List<GameObject>();
        private readonly ProductionViewFactory viewFactory = new ProductionViewFactory();

        private Camera mainCamera;
        private Transform worldRoot;
        private Transform floorRoot;
        private Transform actorRoot;
        private Transform markerRoot;
        private ActorRepository actorRepository;

        public MapDefinition CurrentMap { get; private set; }
        public RunState CurrentRun { get; private set; }
        public Vector2Int HubPlayerPosition { get; private set; }
        public bool ShowEnemyPreparedTargetMarkers { get; private set; } = true;

        public void Initialize()
        {
            EnsureCamera();
            BuildWorldRoots();
        }

        public void SetActorRepository(ActorRepository repository)
        {
            actorRepository = repository;
        }

        public void BindHub(MapDefinition mapDefinition, Vector2Int hubPlayerPosition)
        {
            CurrentMap = mapDefinition;
            CurrentRun = null;
            HubPlayerPosition = hubPlayerPosition;
        }

        public void BindRun(RunState runState, MapDefinition mapDefinition)
        {
            CurrentRun = runState;
            CurrentMap = mapDefinition;
            Debug.Log(
                $"[WorldPresenter] BindRun mapCells={(mapDefinition != null ? mapDefinition.WalkableCells.Count : 0)}, " +
                $"rooms={(mapDefinition != null ? mapDefinition.Rooms.Count : 0)}, " +
                $"corridors={(mapDefinition != null ? mapDefinition.Corridors.Count : 0)}, " +
                $"player={(runState?.Player != null ? runState.Player.GridPosition.ToString() : "null")}");
        }

        public void Refresh()
        {
            EnsureCamera();
            if (worldRoot == null)
            {
                BuildWorldRoots();
            }

            ClearViews();
            if (CurrentMap == null)
            {
                return;
            }

            Debug.Log(
                $"[WorldPresenter] Refresh state={(CurrentRun != null ? "Run" : "Hub")}, " +
                $"mapCells={CurrentMap.WalkableCells.Count}, rooms={CurrentMap.Rooms.Count}, corridors={CurrentMap.Corridors.Count}");
            DrawMap(CurrentMap);
            if (CurrentRun != null && actorRepository != null)
            {
                DrawEnemyPreparedTargetMarkers();
                DrawRunActors();
                FocusCamera(CurrentRun.Player != null ? CurrentRun.Player.GridPosition : CurrentMap.PlayerSpawn);
                return;
            }

            DrawHubMarkers();
            DrawHubPlayer();
            FocusCamera(HubPlayerPosition);
        }

        public void UpdateHubPlayerPosition(Vector2Int hubPlayerPosition)
        {
            HubPlayerPosition = hubPlayerPosition;
        }

        public void SetShowEnemyPreparedTargetMarkers(bool show)
        {
            ShowEnemyPreparedTargetMarkers = show;
        }

        private void DrawMap(MapDefinition map)
        {
            foreach (Vector2Int cell in map.WalkableCells)
            {
                spawnedViews.Add(viewFactory.CreateCell(
                    floorRoot,
                    cell,
                    GetFloorSprite(),
                    GetFloorTint(),
                    $"Cell_{cell.x}_{cell.y}",
                    0));
            }

            for (int i = 0; i < map.WeaponSpawns.Count; i++)
            {
                WeaponSpawnDefinition weaponSpawn = map.WeaponSpawns[i];
                if (weaponSpawn == null)
                {
                    continue;
                }

                spawnedViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    weaponSpawn.Position,
                    GetWeaponSprite(weaponSpawn.Weapon),
                    GetWeaponTint(weaponSpawn.Weapon),
                    $"Weapon_{i}",
                    4));
            }

            if (map.FloorExitPosition != Vector2Int.zero)
            {
                spawnedViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    map.FloorExitPosition,
                    GetFloorExitSprite(),
                    GetFloorExitTint(),
                    "FloorExitMarker",
                    2));
            }
        }

        private void DrawHubMarkers()
        {
            if (IsMarkerEnabled(CurrentMap.DungeonEntrancePosition))
            {
                spawnedViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    CurrentMap.DungeonEntrancePosition,
                    GetDungeonEntranceSprite(),
                    GetDungeonEntranceTint(),
                    "DungeonEntrance",
                    2));
            }

        }

        private void DrawHubPlayer()
        {
            spawnedViews.Add(viewFactory.CreateActor(
                actorRoot,
                "HubPlayer",
                HubPlayerPosition,
                GetPlayerSprite(),
                GetPlayerTint(),
                20));
        }

        private void DrawRunActors()
        {
            IReadOnlyList<ActorEntity> actors = actorRepository.Actors;
            for (int i = 0; i < actors.Count; i++)
            {
                ActorEntity actor = actors[i];
                if (actor == null || !actor.IsAlive)
                {
                    continue;
                }

                Sprite sprite;
                Color tint;
                int sortingOrder;
                if (actor.IsPlayer)
                {
                    sprite = GetPlayerSprite();
                    tint = GetPlayerTint();
                    sortingOrder = 20;
                }
                else
                {
                    sprite = GetEnemySprite(actor);
                    tint = GetEnemyTint(actor);
                    sortingOrder = 10;
                }

                spawnedViews.Add(viewFactory.CreateActor(actorRoot, actor.DisplayName, actor.GridPosition, sprite, tint, sortingOrder));
            }
        }

        private void DrawEnemyPreparedTargetMarkers()
        {
            if (!ShowEnemyPreparedTargetMarkers)
            {
                return;
            }

            IReadOnlyList<ActorEntity> actors = actorRepository.Actors;
            for (int i = 0; i < actors.Count; i++)
            {
                ActorEntity actor = actors[i];
                if (actor == null ||
                    !actor.IsEnemy ||
                    !actor.IsAlive ||
                    !actor.HasPendingEnemyTargetCell)
                {
                    continue;
                }

                switch (actor.PendingEnemyAction)
                {
                    case EnemyActionType.Attack:
                        DrawEnemyPreparedAttackMarkers(actor);
                        break;
                    case EnemyActionType.WanderMove:
                    case EnemyActionType.ChaseMove:
                        DrawEnemyPreparedMoveMarker(actor);
                        break;
                    default:
                        continue;
                }
            }
        }

        private void DrawEnemyPreparedAttackMarkers(ActorEntity actor)
        {
            EnemyAttackPatternDefinition attackPattern = actor.EnemyDefinition != null
                ? actor.EnemyDefinition.AttackPattern
                : null;

            if (attackPattern == null)
            {
                string fallbackMarkerName = $"Pending_{actor.PendingEnemyAction}_{actor.Id}";
                spawnedViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    actor.PendingEnemyTargetCell,
                    GetEnemyAttackMarkerSprite(),
                    GetEnemyAttackMarkerTint(),
                    fallbackMarkerName,
                    6));
                return;
            }

            HashSet<Vector2Int> markedCells = new HashSet<Vector2Int>();
            for (int i = 0; i < attackPattern.Offsets.Count; i++)
            {
                Vector2Int offset = EnemyAttackPatternDefinition.RotateOffset(
                    attackPattern.Offsets[i],
                    actor.PendingEnemyFacingDirection);
                Vector2Int markerCell = actor.GridPosition + offset;
                if (!markedCells.Add(markerCell))
                {
                    continue;
                }

                string markerName = $"Pending_{actor.PendingEnemyAction}_{actor.Id}_{markerCell.x}_{markerCell.y}";
                spawnedViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    markerCell,
                    GetEnemyAttackMarkerSprite(),
                    GetEnemyAttackMarkerTint(),
                    markerName,
                    6));
            }
        }

        private void DrawEnemyPreparedMoveMarker(ActorEntity actor)
        {
            string markerName = $"Pending_{actor.PendingEnemyAction}_{actor.Id}";
            spawnedViews.Add(viewFactory.CreateCell(
                markerRoot,
                actor.PendingEnemyTargetCell,
                GetEnemyMoveMarkerSprite(),
                GetEnemyMoveMarkerTint(),
                markerName,
                6));
        }

        private void EnsureCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5.5f;
            mainCamera.backgroundColor = new Color(0.03f, 0.02f, 0.03f);
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void BuildWorldRoots()
        {
            worldRoot = new GameObject("ProductionWorld").transform;
            floorRoot = new GameObject("Floor").transform;
            floorRoot.SetParent(worldRoot, false);
            actorRoot = new GameObject("Actors").transform;
            actorRoot.SetParent(worldRoot, false);
            markerRoot = new GameObject("Markers").transform;
            markerRoot.SetParent(worldRoot, false);
        }

        private void FocusCamera(Vector2Int cell)
        {
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(cell.x, cell.y, -10f);
            }
        }

        private void ClearViews()
        {
            for (int i = 0; i < spawnedViews.Count; i++)
            {
                if (spawnedViews[i] != null)
                {
                    Destroy(spawnedViews[i]);
                }
            }

            spawnedViews.Clear();
        }

        private static bool IsMarkerEnabled(Vector2Int position)
        {
            return position != Vector2Int.zero;
        }

        private Sprite GetFloorSprite()
        {
            return visualSet != null ? visualSet.FloorSprite : null;
        }

        private Color GetFloorTint()
        {
            return visualSet != null ? visualSet.FloorTint : new Color(0.16f, 0.13f, 0.14f);
        }

        private Sprite GetPlayerSprite()
        {
            return visualSet != null ? visualSet.PlayerSprite : null;
        }

        private Color GetPlayerTint()
        {
            return visualSet != null ? visualSet.PlayerTint : new Color(0.91f, 0.86f, 0.78f);
        }

        private Sprite GetEnemySprite(ActorEntity actor)
        {
            if (actor?.EnemyDefinition != null && actor.EnemyDefinition.Sprite != null)
            {
                return actor.EnemyDefinition.Sprite;
            }

            return visualSet != null ? visualSet.DefaultEnemySprite : null;
        }

        private Color GetEnemyTint(ActorEntity actor)
        {
            if (actor?.EnemyDefinition != null && actor.EnemyDefinition.Sprite != null)
            {
                return actor.EnemyDefinition.Tint;
            }

            return visualSet != null ? visualSet.DefaultEnemyTint : new Color(0.63f, 0.25f, 0.21f);
        }

        private Sprite GetWeaponSprite(WeaponDefinition weapon)
        {
            if (weapon != null && weapon.Sprite != null)
            {
                return weapon.Sprite;
            }

            return visualSet != null ? visualSet.DefaultWeaponSprite : null;
        }

        private Color GetWeaponTint(WeaponDefinition weapon)
        {
            if (weapon != null)
            {
                return weapon.Tint;
            }

            return visualSet != null ? visualSet.DefaultWeaponTint : new Color(0.75f, 0.43f, 0.18f);
        }

        private Sprite GetFloorExitSprite()
        {
            return visualSet != null ? visualSet.FloorExitSprite : null;
        }

        private Color GetFloorExitTint()
        {
            return visualSet != null ? visualSet.FloorExitTint : new Color(0.76f, 0.65f, 0.17f);
        }

        private Sprite GetDungeonEntranceSprite()
        {
            return visualSet != null ? visualSet.DungeonEntranceSprite : null;
        }

        private Color GetDungeonEntranceTint()
        {
            return visualSet != null ? visualSet.DungeonEntranceTint : new Color(0.62f, 0.29f, 0.22f);
        }

        private Sprite GetEnemyAttackMarkerSprite()
        {
            return visualSet != null ? visualSet.EnemyAttackMarkerSprite : null;
        }

        private Color GetEnemyAttackMarkerTint()
        {
            return visualSet != null ? visualSet.EnemyAttackMarkerTint : new Color(0.82f, 0.16f, 0.13f);
        }

        private Sprite GetEnemyMoveMarkerSprite()
        {
            return visualSet != null ? visualSet.EnemyMoveMarkerSprite : null;
        }

        private Color GetEnemyMoveMarkerTint()
        {
            return visualSet != null ? visualSet.EnemyMoveMarkerTint : new Color(0.18f, 0.68f, 0.26f);
        }
    }
}

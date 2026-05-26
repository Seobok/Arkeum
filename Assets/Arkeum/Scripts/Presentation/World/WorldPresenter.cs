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

        private readonly List<GameObject> floorViews = new List<GameObject>();
        private readonly List<GameObject> markerViews = new List<GameObject>();
        private readonly Dictionary<string, ActorView> actorViews = new Dictionary<string, ActorView>();
        private readonly HashSet<string> activeActorIds = new HashSet<string>();
        private readonly ProductionViewFactory viewFactory = new ProductionViewFactory();

        private Camera mainCamera;
        private Transform worldRoot;
        private Transform floorRoot;
        private Transform actorRoot;
        private Transform markerRoot;
        private ActorRepository actorRepository;
        private MapDefinition renderedFloorMap;

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
            MarkFloorDirtyIfMapChanged(mapDefinition);
        }

        public void BindRun(RunState runState, MapDefinition mapDefinition)
        {
            CurrentRun = runState;
            CurrentMap = mapDefinition;
            MarkFloorDirtyIfMapChanged(mapDefinition);
        }

        public void Refresh()
        {
            EnsureCamera();
            if (worldRoot == null)
            {
                BuildWorldRoots();
            }

            if (CurrentMap == null)
            {
                ClearAllViews();
                return;
            }

            RefreshFloor(CurrentMap);
            ClearMarkerViews();
            if (CurrentRun != null && actorRepository != null)
            {
                DrawMapMarkers(CurrentMap);
                DrawEnemyPreparedTargetMarkers();
                RefreshRunActors();
                FocusCamera(CurrentRun.Player != null ? CurrentRun.Player.GridPosition : CurrentMap.PlayerSpawn);
                return;
            }

            DrawMapMarkers(CurrentMap);
            DrawHubMarkers();
            RefreshHubPlayer();
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

        private void RefreshFloor(MapDefinition map)
        {
            if (renderedFloorMap == map)
            {
                return;
            }

            ClearFloorViews();
            DrawFloor(map);
            renderedFloorMap = map;
        }

        private void DrawFloor(MapDefinition map)
        {
            foreach (Vector2Int cell in map.WalkableCells)
            {
                floorViews.Add(viewFactory.CreateCell(
                    floorRoot,
                    cell,
                    GetFloorSprite(),
                    GetFloorTint(),
                    $"Cell_{cell.x}_{cell.y}",
                    0));
            }
        }

        private void DrawMapMarkers(MapDefinition map)
        {
            for (int i = 0; i < map.WallCells.Count; i++)
            {
                Vector2Int wallCell = map.WallCells[i];
                markerViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    wallCell,
                    GetWallSprite(),
                    GetWallTint(),
                    $"Wall_{wallCell.x}_{wallCell.y}",
                    1));
            }

            for (int i = 0; i < map.WeaponSpawns.Count; i++)
            {
                WeaponSpawnDefinition weaponSpawn = map.WeaponSpawns[i];
                if (weaponSpawn == null)
                {
                    continue;
                }

                markerViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    weaponSpawn.Position,
                    GetWeaponSprite(weaponSpawn.Weapon),
                    GetWeaponTint(weaponSpawn.Weapon),
                    $"Weapon_{i}",
                    4));
            }

            for (int i = 0; i < map.ShopOffers.Count; i++)
            {
                ShopOfferDefinition shopOffer = map.ShopOffers[i];
                if (shopOffer == null)
                {
                    continue;
                }

                markerViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    shopOffer.Position,
                    GetWeaponSprite(shopOffer.Weapon),
                    GetShopOfferTint(shopOffer.Weapon),
                    $"ShopOffer_{i}",
                    5));
            }

            if (map.FloorExitPosition != Vector2Int.zero)
            {
                markerViews.Add(viewFactory.CreateCell(
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
                markerViews.Add(viewFactory.CreateCell(
                    markerRoot,
                    CurrentMap.DungeonEntrancePosition,
                    GetDungeonEntranceSprite(),
                    GetDungeonEntranceTint(),
                    "DungeonEntrance",
                    2));
            }

        }

        private void RefreshHubPlayer()
        {
            activeActorIds.Clear();
            RefreshActorView("HubPlayer", "HubPlayer", HubPlayerPosition, true, GetPlayerSprite(), GetPlayerTint(), 20);
            RemoveInactiveActorViews();
        }

        private void RefreshRunActors()
        {
            activeActorIds.Clear();
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

                RefreshActorView(actor.Id, actor.DisplayName, actor.GridPosition, actor.IsPlayer, sprite, tint, sortingOrder);
            }

            RemoveInactiveActorViews();
        }

        private void RefreshActorView(
            string actorId,
            string displayName,
            Vector2Int position,
            bool isPlayer,
            Sprite sprite,
            Color tint,
            int sortingOrder)
        {
            if (string.IsNullOrEmpty(actorId))
            {
                return;
            }

            activeActorIds.Add(actorId);
            if (!actorViews.TryGetValue(actorId, out ActorView actorView) || actorView == null)
            {
                actorView = viewFactory.CreateActor(actorRoot, displayName, position, sprite, tint, sortingOrder);
                actorViews[actorId] = actorView;
                return;
            }

            actorView.name = displayName;
            actorView.SetVisual(sprite, tint, sortingOrder);
            actorView.MoveTo(position, isPlayer);
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
                markerViews.Add(viewFactory.CreateCell(
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
                markerViews.Add(viewFactory.CreateCell(
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
            markerViews.Add(viewFactory.CreateCell(
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

        private void MarkFloorDirtyIfMapChanged(MapDefinition mapDefinition)
        {
            if (renderedFloorMap != mapDefinition)
            {
                renderedFloorMap = null;
            }
        }

        private void ClearAllViews()
        {
            ClearFloorViews();
            ClearMarkerViews();
            ClearActorViews();
            renderedFloorMap = null;
        }

        private void ClearFloorViews()
        {
            DestroyViews(floorViews);
        }

        private void ClearMarkerViews()
        {
            DestroyViews(markerViews);
        }

        private void ClearActorViews()
        {
            foreach (ActorView actorView in actorViews.Values)
            {
                if (actorView != null)
                {
                    Destroy(actorView.gameObject);
                }
            }

            actorViews.Clear();
            activeActorIds.Clear();
        }

        private void RemoveInactiveActorViews()
        {
            List<string> removedActorIds = null;
            foreach (KeyValuePair<string, ActorView> entry in actorViews)
            {
                if (activeActorIds.Contains(entry.Key))
                {
                    continue;
                }

                if (entry.Value != null)
                {
                    Destroy(entry.Value.gameObject);
                }

                if (removedActorIds == null)
                {
                    removedActorIds = new List<string>();
                }

                removedActorIds.Add(entry.Key);
            }

            if (removedActorIds == null)
            {
                return;
            }

            for (int i = 0; i < removedActorIds.Count; i++)
            {
                actorViews.Remove(removedActorIds[i]);
            }
        }

        private void DestroyViews(List<GameObject> views)
        {
            for (int i = 0; i < views.Count; i++)
            {
                if (views[i] != null)
                {
                    Destroy(views[i]);
                }
            }

            views.Clear();
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

        private Sprite GetWallSprite()
        {
            return visualSet != null ? visualSet.WallSprite : null;
        }

        private Color GetWallTint()
        {
            return visualSet != null ? visualSet.WallTint : new Color(0.08f, 0.08f, 0.09f);
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

        private Color GetShopOfferTint(WeaponDefinition weapon)
        {
            Color tint = GetWeaponTint(weapon);
            return Color.Lerp(tint, new Color(1f, 0.82f, 0.22f), 0.35f);
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

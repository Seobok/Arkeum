using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Presentation.World
{
    public sealed class WorldPresenter : MonoBehaviour
    {
        private const float CameraZ = -10f;
        private const int RunVisionRange = 5;
        private const int UnexploredFogSortingOrder = 100;
        private const int ExploredFogSortingOrder = 99;

        [SerializeField] private WorldVisualSet visualSet;

        private readonly List<GameObject> floorViews = new List<GameObject>();
        private readonly List<GameObject> markerViews = new List<GameObject>();
        private readonly List<GameObject> fogViews = new List<GameObject>();
        private readonly Dictionary<Vector2Int, SpriteRenderer> fogRenderers = new Dictionary<Vector2Int, SpriteRenderer>();
        private readonly Dictionary<string, ActorView> actorViews = new Dictionary<string, ActorView>();
        private readonly HashSet<string> activeActorIds = new HashSet<string>();
        private readonly HashSet<Vector2Int> exploredCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> visibleCells = new HashSet<Vector2Int>();
        private readonly ProductionViewFactory viewFactory = new ProductionViewFactory();

        private Camera mainCamera;
        private Transform worldRoot;
        private Transform floorRoot;
        private Transform actorRoot;
        private Transform markerRoot;
        private Transform fogRoot;
        private Transform cameraFollowTarget;
        private ActorRepository actorRepository;
        private MapDefinition renderedFloorMap;
        private MapDefinition renderedFogMap;

        public MapDefinition CurrentMap { get; private set; }
        public RunState CurrentRun { get; private set; }
        public Vector2Int HubPlayerPosition { get; private set; }
        public bool ShowEnemyPreparedTargetMarkers { get; private set; } = true;

        public void Initialize()
        {
            EnsureCamera();
            BuildWorldRoots();
        }

        // 액터 등록
        public void SetActorRepository(ActorRepository repository)
        {
            actorRepository = repository;
        }

        // HUB 맵 등록
        public void BindHub(MapDefinition mapDefinition, Vector2Int hubPlayerPosition)
        {
            if (CurrentMap != mapDefinition)
            {
                ClearRunFogState();
                ClearFogViews();
            }

            CurrentMap = mapDefinition;
            CurrentRun = null;
            HubPlayerPosition = hubPlayerPosition;
            MarkFloorDirtyIfMapChanged(mapDefinition);
        }

        // Run 맵 등록
        public void BindRun(RunState runState, MapDefinition mapDefinition)
        {
            if (CurrentMap != mapDefinition)
            {
                ClearRunFogState();
                ClearFogViews();
            }

            CurrentRun = runState;
            CurrentMap = mapDefinition;
            MarkFloorDirtyIfMapChanged(mapDefinition);
        }

        // 새로 그리기
        // 호출 시점 :: 허브 진입 시 / 런 시작 시 / 런 결과 출력 시 / 인터렉션 시 / 플레이어 이동 시 / 타이밍 능력 사용 시 / 타이밍 능력 종료 시 / 다음 층 이동 시 / 예측 온오프 시
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

            // 바닥 타일 View
            RefreshFloor(CurrentMap);
            ClearMarkerViews();

            // 런 사용 View 새로고침
            if (CurrentRun != null && actorRepository != null)
            {
                UpdateRunFog();

                // 마커 View
                DrawMapMarkers(CurrentMap);
                DrawEnemyPreparedTargetMarkers();
                DrawRunFog(CurrentMap);

                // 액터 View
                RefreshRunActors();
                FollowRunPlayer();
                return;
            }

            // 허브 사용 View 새로고침
            DrawMapMarkers(CurrentMap);
            DrawHubMarkers();
            RefreshHubPlayer();
            FollowHubPlayer();
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
            //같은 MapDefinition이면 바닥 재생성 X
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
            // 마커 View는 항상 재생성
            // 벽, 바닥 무기, 상점 진열품, 층 출구, 적 예고 공격/이동, 허브 던전 입구

            // 벽 View
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

            // 무기 View
            for (int i = 0; i < map.WeaponSpawns.Count; i++)
            {
                WeaponSpawnDefinition weaponSpawn = map.WeaponSpawns[i];
                if (weaponSpawn == null || !IsRunCellVisible(weaponSpawn.Position))
                {
                    continue;
                }

                var weaponMark = viewFactory.CreateCell(
                    markerRoot,
                    weaponSpawn.Position,
                    GetWeaponSprite(weaponSpawn.Weapon),
                    GetWeaponTint(weaponSpawn.Weapon),
                    $"Weapon_{i}",
                    4);
                // 무기 마크가 잘 안보여서 스케일 조절
                weaponMark.transform.localScale = new Vector3(2, 2, 1);
                markerViews.Add(weaponMark);
            }

            // 진열대 View
            for (int i = 0; i < map.ShopOffers.Count; i++)
            {
                ShopOfferDefinition shopOffer = map.ShopOffers[i];
                if (shopOffer == null || !IsRunCellVisible(shopOffer.Position))
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

            DrawShopTeleportMarker(map.ShopEntrancePosition, "ShopEntranceMarker");
            DrawShopTeleportMarker(map.ShopExitPosition, "ShopExitMarker");

            // 출구 View
            if (map.FloorExitPosition != Vector2Int.zero && IsRunCellVisible(map.FloorExitPosition))
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

        private void DrawShopTeleportMarker(Vector2Int position, string markerName)
        {
            if (!IsMarkerEnabled(position) || !IsRunCellVisible(position))
            {
                return;
            }

            GameObject marker = viewFactory.CreateCell(
                markerRoot,
                position,
                GetFloorExitSprite(),
                GetShopTeleportMarkerTint(),
                markerName,
                3);
            marker.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            markerViews.Add(marker);
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
            RefreshActorView(
                "HubPlayer",
                "HubPlayer",
                HubPlayerPosition,
                Vector2Int.zero,
                true,
                GetPlayerSprite(),
                GetPlayerTint(),
                20);
            RemoveInactiveActorViews();
        }

        private void RefreshRunActors()
        {
            // 액터 View는 매번 전부 지우지 않고 ID 기준으로 재사용

            activeActorIds.Clear();
            IReadOnlyList<ActorEntity> actors = actorRepository.Actors;
            for (int i = 0; i < actors.Count; i++)
            {
                // 살아있는 액터 순회
                ActorEntity actor = actors[i];
                if (actor == null || !actor.IsAlive)
                {
                    continue;
                }

                if (actor.IsEnemy && !IsRunCellVisible(actor.GridPosition))
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

                // 액터 View 새로고침
                RefreshActorView(
                    actor.Id,
                    actor.DisplayName,
                    actor.GridPosition,
                    actor.FacingDirection,
                    actor.IsPlayer,
                    sprite,
                    tint,
                    sortingOrder);
            }

            // 비활성화 액터 제거
            RemoveInactiveActorViews();
        }

        private void RefreshActorView(
            string actorId,
            string displayName,
            Vector2Int position,
            Vector2Int facingDirection,
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
                // 기존에 있는 액터는 재생성 X
                actorView = viewFactory.CreateActor(actorRoot, displayName, position, sprite, tint, sortingOrder);
                actorView.SetFacing(facingDirection);
                actorViews[actorId] = actorView;
                return;
            }

            actorView.name = displayName;
            actorView.SetVisual(sprite, tint, sortingOrder);
            actorView.SetFacing(facingDirection);
            actorView.MoveTo(position, isPlayer);
        }

        private void DrawEnemyPreparedTargetMarkers()
        {
            if (!ShowEnemyPreparedTargetMarkers)
            {
                return;
            }

            // 적 예고 View
            IReadOnlyList<ActorEntity> actors = actorRepository.Actors;
            for (int i = 0; i < actors.Count; i++)
            {
                ActorEntity actor = actors[i];
                if (actor == null ||
                    !actor.IsEnemy ||
                    !actor.IsAlive ||
                    !actor.HasPendingEnemyTargetCell ||
                    !IsRunCellVisible(actor.GridPosition))
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
                if (!IsRunCellVisible(actor.PendingEnemyTargetCell))
                {
                    return;
                }

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
                if (!markedCells.Add(markerCell) || !IsRunCellVisible(markerCell))
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
            if (!IsRunCellVisible(actor.PendingEnemyTargetCell))
            {
                return;
            }

            string markerName = $"Pending_{actor.PendingEnemyAction}_{actor.Id}";
            markerViews.Add(viewFactory.CreateCell(
                markerRoot,
                actor.PendingEnemyTargetCell,
                GetEnemyMoveMarkerSprite(),
                GetEnemyMoveMarkerTint(),
                markerName,
                6));
        }

        private void UpdateRunFog()
        {
            visibleCells.Clear();
            if (CurrentRun?.Player == null || CurrentMap == null)
            {
                return;
            }

            Vector2Int playerCell = CurrentRun.Player.GridPosition;
            for (int i = 0; i < CurrentMap.WalkableCells.Count; i++)
            {
                Vector2Int cell = CurrentMap.WalkableCells[i];
                int distance = Mathf.Abs(cell.x - playerCell.x) + Mathf.Abs(cell.y - playerCell.y);
                if (distance > RunVisionRange)
                {
                    continue;
                }

                visibleCells.Add(cell);
                exploredCells.Add(cell);
            }
        }

        private void DrawRunFog(MapDefinition map)
        {
            if (CurrentRun == null || map == null)
            {
                return;
            }

            EnsureFogViews(map);
            for (int i = 0; i < map.WalkableCells.Count; i++)
            {
                Vector2Int cell = map.WalkableCells[i];
                if (!fogRenderers.TryGetValue(cell, out SpriteRenderer fogRenderer) || fogRenderer == null)
                {
                    continue;
                }

                if (visibleCells.Contains(cell))
                {
                    fogRenderer.gameObject.SetActive(false);
                    continue;
                }

                bool explored = exploredCells.Contains(cell);
                fogRenderer.gameObject.SetActive(true);
                fogRenderer.color = explored ? GetExploredFogTint() : GetUnexploredFogTint();
                fogRenderer.sortingOrder = explored ? ExploredFogSortingOrder : UnexploredFogSortingOrder;
            }
        }

        private void EnsureFogViews(MapDefinition map)
        {
            if (renderedFogMap == map)
            {
                return;
            }

            ClearFogViews();
            if (fogRoot == null)
            {
                BuildWorldRoots();
            }

            for (int i = 0; i < map.WalkableCells.Count; i++)
            {
                Vector2Int cell = map.WalkableCells[i];
                GameObject fog = viewFactory.CreateCell(
                    fogRoot,
                    cell,
                    null,
                    GetUnexploredFogTint(),
                    $"Fog_{cell.x}_{cell.y}",
                    UnexploredFogSortingOrder);
                fog.transform.localScale = new Vector3(1f, 1f, 1f);
                fogViews.Add(fog);

                SpriteRenderer renderer = fog.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    fogRenderers[cell] = renderer;
                }
            }

            renderedFogMap = map;
        }

        private void EnsureCamera()
        {
            mainCamera = Camera.main;
            bool createdCamera = false;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                createdCamera = true;
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5.5f;
            mainCamera.backgroundColor = new Color(0.03f, 0.02f, 0.03f);
            if (createdCamera)
            {
                mainCamera.transform.position = new Vector3(0f, 0f, CameraZ);
            }
            else if (!Mathf.Approximately(mainCamera.transform.position.z, CameraZ))
            {
                Vector3 position = mainCamera.transform.position;
                position.z = CameraZ;
                mainCamera.transform.position = position;
            }
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
            fogRoot = new GameObject("Fog").transform;
            fogRoot.SetParent(worldRoot, false);
        }

        private void FollowRunPlayer()
        {
            if (CurrentRun?.Player == null)
            {
                SetCameraFollowTarget(null, CurrentMap != null ? CurrentMap.PlayerSpawn : Vector2Int.zero);
                return;
            }

            SetCameraFollowTargetByActorId(CurrentRun.Player.Id, CurrentRun.Player.GridPosition);
        }

        private void FollowHubPlayer()
        {
            SetCameraFollowTargetByActorId("HubPlayer", HubPlayerPosition);
        }

        private void SetCameraFollowTargetByActorId(string actorId, Vector2Int fallbackCell)
        {
            if (!string.IsNullOrEmpty(actorId) &&
                actorViews.TryGetValue(actorId, out ActorView actorView) &&
                actorView != null)
            {
                SetCameraFollowTarget(actorView.transform, fallbackCell);
                return;
            }

            SetCameraFollowTarget(null, fallbackCell);
        }

        private void SetCameraFollowTarget(Transform target, Vector2Int fallbackCell)
        {
            cameraFollowTarget = target;
            if (cameraFollowTarget == null)
            {
                MoveCameraTo(new Vector3(fallbackCell.x, fallbackCell.y, CameraZ));
            }
        }

        private void LateUpdate()
        {
            if (cameraFollowTarget == null)
            {
                return;
            }

            Vector3 targetPosition = cameraFollowTarget.position;
            MoveCameraTo(new Vector3(targetPosition.x, targetPosition.y, CameraZ));
        }

        private void MoveCameraTo(Vector3 position)
        {
            if (mainCamera == null)
            {
                EnsureCamera();
            }

            if (mainCamera != null)
            {
                mainCamera.transform.position = position;
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
            ClearFogViews();
            ClearActorViews();
            ClearRunFogState();
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

        private void ClearFogViews()
        {
            DestroyViews(fogViews);
            fogRenderers.Clear();
            renderedFogMap = null;
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

        private void ClearRunFogState()
        {
            exploredCells.Clear();
            visibleCells.Clear();
        }

        private bool IsRunCellVisible(Vector2Int cell)
        {
            return CurrentRun == null || visibleCells.Contains(cell);
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

        private Color GetShopTeleportMarkerTint()
        {
            return new Color(0.25f, 0.82f, 0.85f);
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

        private Color GetUnexploredFogTint()
        {
            return visualSet != null ? visualSet.UnexploredFogTint : Color.black;
        }

        private Color GetExploredFogTint()
        {
            return visualSet != null ? visualSet.ExploredFogTint : new Color(0.45f, 0.45f, 0.45f, 0.65f);
        }
    }
}

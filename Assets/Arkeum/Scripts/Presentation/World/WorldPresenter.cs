using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Presentation.World
{
    public sealed class WorldPresenter : MonoBehaviour
    {
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

        public void BindRun(RunState runState)
        {
            CurrentRun = runState;
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

            DrawMap(CurrentMap);
            if (CurrentRun != null && actorRepository != null)
            {
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

        private void DrawMap(MapDefinition map)
        {
            foreach (Vector2Int cell in map.WalkableCells)
            {
                Color floorColor = map.DepthByCell.TryGetValue(cell, out int depth) && depth >= 2
                    ? new Color(0.12f, 0.09f, 0.16f)
                    : new Color(0.16f, 0.13f, 0.14f);
                spawnedViews.Add(viewFactory.CreateCell(floorRoot, cell, floorColor, $"Cell_{cell.x}_{cell.y}", 0));
            }

            for (int i = 0; i < map.TemporaryWeaponSpawns.Count; i++)
            {
                spawnedViews.Add(viewFactory.CreateCell(markerRoot, map.TemporaryWeaponSpawns[i], new Color(0.75f, 0.43f, 0.18f), $"Weapon_{i}", 4));
            }

            if (map.MerchantPosition != Vector2Int.zero || map.ReliquaryPosition != Vector2Int.zero)
            {
                spawnedViews.Add(viewFactory.CreateCell(markerRoot, map.MerchantPosition, new Color(0.1f, 0.4f, 0.37f), "MerchantMarker", 3));
                spawnedViews.Add(viewFactory.CreateCell(markerRoot, map.ReliquaryPosition, new Color(0.76f, 0.65f, 0.17f), "ReliquaryMarker", 2));
            }
        }

        private void DrawHubMarkers()
        {
            spawnedViews.Add(viewFactory.CreateCell(markerRoot, CurrentMap.StartAltarPosition, new Color(0.62f, 0.29f, 0.22f), "HubStartGate", 2));
            spawnedViews.Add(viewFactory.CreateCell(markerRoot, CurrentMap.UnlockAltarPosition, new Color(0.84f, 0.73f, 0.28f), "HubUnlock", 2));
            spawnedViews.Add(viewFactory.CreateCell(markerRoot, CurrentMap.UndertakerPosition, new Color(0.19f, 0.55f, 0.51f), "HubUndertaker", 2));
            spawnedViews.Add(viewFactory.CreateActor(actorRoot, "Undertaker", CurrentMap.UndertakerPosition, new Color(0.19f, 0.55f, 0.51f), 10));
        }

        private void DrawHubPlayer()
        {
            spawnedViews.Add(viewFactory.CreateActor(actorRoot, "HubPlayer", HubPlayerPosition, new Color(0.91f, 0.86f, 0.78f), 20));
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

                Color color;
                int sortingOrder;
                if (actor.IsPlayer)
                {
                    color = new Color(0.91f, 0.86f, 0.78f);
                    sortingOrder = 20;
                }
                else if (actor.BrainType == BrainType.HeavyChaser)
                {
                    color = new Color(0.42f, 0.48f, 0.52f);
                    sortingOrder = 10;
                }
                else
                {
                    color = new Color(0.63f, 0.25f, 0.21f);
                    sortingOrder = 10;
                }

                spawnedViews.Add(viewFactory.CreateActor(actorRoot, actor.DisplayName, actor.GridPosition, color, sortingOrder));
            }
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
    }
}

using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed partial class PrototypeGameController
    {
        private void SpawnActors()
        {
            player = new ActorRuntime
            {
                Kind = ActorKind.Player,
                Brain = BrainType.Player,
                DisplayName = "Ash Knight",
                Position = layout.PlayerSpawn,
                MaxHp = 12,
                Hp = 12,
                Attack = 3,
                Defense = 1,
                ActionInterval = 1,
                BloodReward = 0,
            };
            player.View = viewFactory.CreateActor(actorRoot, "Player", player.Position, new Color(0.91f, 0.86f, 0.78f), 20);
            spawnedViews.Add(player.View);

            enemies.Clear();
            AddEnemy("AshCrawler_A", ActorKind.AshCrawler, BrainType.Chaser, new Vector2Int(4, 0), 5, 2, 0, 1, 1, new Color(0.63f, 0.25f, 0.21f));
            AddEnemy("AshCrawler_B", ActorKind.AshCrawler, BrainType.Chaser, new Vector2Int(7, 1), 5, 2, 0, 1, 1, new Color(0.63f, 0.25f, 0.21f));
            AddEnemy("IronWarden", ActorKind.IronWarden, BrainType.HeavyChaser, new Vector2Int(12, 0), 9, 4, 1, 2, 2, new Color(0.42f, 0.48f, 0.52f));

            GameObject merchant = viewFactory.CreateActor(actorRoot, "Merchant", layout.MerchantPosition, new Color(0.16f, 0.62f, 0.55f), 18);
            spawnedViews.Add(merchant);
            GameObject reliquary = viewFactory.CreateActor(actorRoot, "Reliquary", layout.ReliquaryPosition, new Color(0.93f, 0.72f, 0.28f), 16);
            reliquary.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            spawnedViews.Add(reliquary);
        }

        private void AddEnemy(string name, ActorKind kind, BrainType brain, Vector2Int position, int hp, int attack, int defense, int interval, int reward, Color color)
        {
            ActorRuntime enemy = new ActorRuntime
            {
                Kind = kind,
                Brain = brain,
                DisplayName = kind == ActorKind.AshCrawler ? "Ash Crawler" : "Iron Warden",
                Position = position,
                MaxHp = hp,
                Hp = hp,
                Attack = attack,
                Defense = defense,
                ActionInterval = interval,
                BloodReward = reward,
            };
            enemy.View = viewFactory.CreateActor(actorRoot, name, position, color, 10);
            enemies.Add(enemy);
            spawnedViews.Add(enemy.View);
        }

        private DungeonLayout BuildLayout()
        {
            return layoutFactory.BuildRunLayout();
        }

        private void DrawLayout(DungeonLayout target)
        {
            for (int x = -1; x <= 15; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (!target.IsWalkable(cell))
                    {
                        continue;
                    }

                    Color floorColor = target.GetDepth(cell) == 1
                        ? new Color(0.16f, 0.13f, 0.14f)
                        : new Color(0.12f, 0.09f, 0.16f);
                    spawnedViews.Add(viewFactory.CreateCell(floorRoot, cell, floorColor, $"Floor_{x}_{y}", 0));
                }
            }

            for (int i = 0; i < target.TemporaryWeaponSpawns.Count; i++)
            {
                spawnedViews.Add(viewFactory.CreateCell(markerRoot, target.TemporaryWeaponSpawns[i], new Color(0.75f, 0.43f, 0.18f), $"Weapon_{i}", 4));
            }

            spawnedViews.Add(viewFactory.CreateCell(markerRoot, target.MerchantPosition, new Color(0.1f, 0.4f, 0.37f), "MerchantMarker", 3));
            spawnedViews.Add(viewFactory.CreateCell(markerRoot, target.ReliquaryPosition, new Color(0.76f, 0.65f, 0.17f), "ReliquaryMarker", 2));
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

        private void UpdateCamera()
        {
            if (mainCamera == null)
            {
                return;
            }

            Vector3 target;
            if (Phase != GamePhase.Hub && player != null)
            {
                target = new Vector3(player.Position.x, player.Position.y, -10f);
            }
            else
            {
                target = new Vector3(hubPlayerPosition.x, hubPlayerPosition.y, -10f);
            }

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, 0.2f);
        }

        private void BuildWorldRoots()
        {
            worldRoot = new GameObject("PrototypeWorld").transform;
            floorRoot = new GameObject("Floor").transform;
            floorRoot.SetParent(worldRoot, false);
            actorRoot = new GameObject("Actors").transform;
            actorRoot.SetParent(worldRoot, false);
            markerRoot = new GameObject("Markers").transform;
            markerRoot.SetParent(worldRoot, false);
        }

        private void ClearSpawnedViews()
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

        private void SyncActorView(ActorRuntime actor)
        {
            if (actor.View != null)
            {
                actor.View.transform.position = new Vector3(actor.Position.x, actor.Position.y, -0.1f);
            }
        }

        private string GetDepthName(int depth)
        {
            return depth <= 1 ? "Outer Corridor" : "Deep Corridor";
        }

        private void SetMessage(string message)
        {
            hud.CurrentMessage = message;
        }
    }
}

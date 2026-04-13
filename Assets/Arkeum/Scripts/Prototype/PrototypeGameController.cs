using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Prototype
{
    public sealed class PrototypeGameController : MonoBehaviour
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
        private DungeonLayout layout;
        private Camera mainCamera;
        private Transform worldRoot;
        private Transform floorRoot;
        private Transform actorRoot;
        private Transform markerRoot;

        private ActorRuntime player;
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
            Profile = saveService.LoadProfile();
            Phase = GamePhase.Hub;

            EnsureCamera();
            BuildWorldRoots();
            SeedDialogue();
            EnterHub("귀환 제단이 당신을 다시 받아들였다.");
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
            string questState = Profile.mq01Completed ? "완료됨" : "진행 중";
            return $"MQ-01 상태: {questState}\n저장 위치: {saveService.GetProfilePath()}";
        }

        public string GetRunResultHeadline()
        {
            return Run.EndReason == RunEndReason.Death
                ? "당신은 잿빛 회랑에서 쓰러졌다."
                : "네 이름에 반응한 잔광을 회수하고 제단으로 되돌아왔다.";
        }

        public IReadOnlyList<string> GetLostResultLines()
        {
            return lostResultLines;
        }

        public IReadOnlyList<string> GetKeptResultLines()
        {
            return keptResultLines;
        }

        private void UpdateHubInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.enterKey.wasPressedThisFrame)
            {
                StartRun();
                return;
            }

            if (keyboard.uKey.wasPressedThisFrame)
            {
                TryUnlockStartingBandage();
                return;
            }

            if (keyboard.tKey.wasPressedThisFrame)
            {
                AdvanceUndertakerDialogue();
            }
        }

        private void UpdateRunInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (TryHandleMovement(keyboard))
            {
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                TryAttack();
                return;
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                ConsumeTurn("숨을 고르며 주변의 움직임을 살핀다.");
                return;
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                TryInteract();
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                UseBandage();
                return;
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                UseDraught();
            }
        }

        private void UpdateRunResultInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.enterKey.wasPressedThisFrame)
            {
                EnterHub("회귀의 잔향이 가라앉고 다시 제단 앞에 선다.");
            }
        }

        private bool TryHandleMovement(Keyboard keyboard)
        {
            Vector2Int direction = Vector2Int.zero;
            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.up;
            }
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.down;
            }
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.left;
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.right;
            }

            if (direction == Vector2Int.zero)
            {
                return false;
            }

            Vector2Int target = player.Position + direction;
            if (!layout.IsWalkable(target))
            {
                SetMessage("벽과 뒤틀린 혈흔이 길을 막고 있다.");
                return true;
            }

            if (TryGetEnemyAt(target, out _))
            {
                SetMessage("적이 길을 막고 있다. 공격하거나 다른 경로를 찾아야 한다.");
                return true;
            }

            player.Position = target;
            SyncActorView(player);
            Run.DepthReached = Mathf.Max(Run.DepthReached, layout.GetDepth(target));
            SetMessage($"{GetDepthName(Run.DepthReached)} 쪽으로 한 칸 전진했다.");
            ConsumeTurn(null);
            return true;
        }

        private void StartRun()
        {
            ClearSpawnedViews();
            layout = BuildLayout();
            DrawLayout(layout);
            SpawnActors();
            reliquaryClaimed = false;
            temporaryWeaponCollected = false;
            draughtStock = 2;

            int startingBandage = Profile.unlockedStartingBandage ? 1 : 0;
            Run = new RunState
            {
                RunIndex = Profile.totalReturns + 1,
                TurnCount = 0,
                DepthReached = 1,
                Hyeolpyeon = 0,
                CurrentHp = 12,
                MaxHp = 12,
                BandageCount = startingBandage,
                DraughtCount = 0,
                AttackBonus = 0,
                TemporaryWeaponEquipped = false,
                EndReason = RunEndReason.None,
            };

            player.Hp = Run.CurrentHp;
            player.MaxHp = Run.MaxHp;
            Phase = GamePhase.InRun;
            hud.DialogueLine = string.Empty;
            SetMessage("잿빛 회랑으로 하강했다. 행동할 때마다 적이 반응한다.");
            UpdateCamera();
        }

        private void EnterHub(string message)
        {
            Phase = GamePhase.Hub;
            Run = null;
            player = null;
            enemies.Clear();
            ClearSpawnedViews();
            SetMessage(message);
            hud.DialogueLine = GetUndertakerGreeting();
            EnsureCamera();
        }

        private void EndRun(RunEndReason reason)
        {
            Run.EndReason = reason;
            Profile.totalReturns += 1;
            Profile.highestReachedDepth = Mathf.Max(Profile.highestReachedDepth, Run.DepthReached);

            int jangwangGain = reason == RunEndReason.DepthClear ? 2 : 1;
            if (Run.DepthReached >= 2)
            {
                jangwangGain += 1;
            }

            Profile.jangwang += jangwangGain;
            if (reason == RunEndReason.DepthClear && !Profile.mq01Completed)
            {
                Profile.mq01Completed = true;
                if (!Profile.completedQuestIds.Contains("MQ-01"))
                {
                    Profile.completedQuestIds.Add("MQ-01");
                }
            }

            saveService.SaveProfile(Profile);
            BuildResultLines(jangwangGain);
            Phase = GamePhase.RunResult;
            SetMessage(reason == RunEndReason.Death
                ? "죽음은 끝이 아니라 정산의 시작이다."
                : "회수한 잔광이 제단으로 흘러들어 간다.");
        }

        private void BuildResultLines(int jangwangGain)
        {
            lostResultLines.Clear();
            keptResultLines.Clear();

            lostResultLines.Add($"혈편 {Run.Hyeolpyeon} 전량 소실");
            lostResultLines.Add($"던전 내 구매 소모품 {Run.DraughtCount}개 소실");
            lostResultLines.Add(Run.TemporaryWeaponEquipped ? "임시 장비 마모된 톱날 소실" : "추가 임시 장비 없음");

            keptResultLines.Add($"잔광 +{jangwangGain} 획득");
            keptResultLines.Add($"총 잔광 {Profile.jangwang}");
            keptResultLines.Add($"총 회귀 횟수 {Profile.totalReturns}");
            keptResultLines.Add($"최고 도달 구역 {Profile.highestReachedDepth}");
            keptResultLines.Add(Profile.unlockedStartingBandage
                ? "응고 지혈포 해금 유지"
                : "응고 지혈포 미해금");
        }

        private void TryUnlockStartingBandage()
        {
            if (Profile.unlockedStartingBandage)
            {
                SetMessage("응고 지혈포는 이미 해금되어 있다.");
                return;
            }

            if (Profile.jangwang < StartingBandageUnlockCost)
            {
                SetMessage("잔광이 부족하다. 더 깊은 곳에서 회수해야 한다.");
                return;
            }

            Profile.jangwang -= StartingBandageUnlockCost;
            Profile.unlockedStartingBandage = true;
            saveService.SaveProfile(Profile);
            SetMessage("응고 지혈포를 해금했다. 앞으로 모든 런 시작 시 1개를 지급한다.");
        }

        private void AdvanceUndertakerDialogue()
        {
            if (undertakerLines.Count == 0)
            {
                SeedDialogue();
            }

            hud.DialogueLine = undertakerLines.Dequeue();
            undertakerLines.Enqueue(hud.DialogueLine);
            SetMessage("장례자가 회귀의 기록을 넘기며 담담히 말한다.");
        }

        private string GetUndertakerGreeting()
        {
            if (Profile.totalReturns == 0)
            {
                return "장례자: 처음 내려가는 얼굴은 아니군. 다만 아직 자기 이름을 모르는 표정이야.";
            }

            if (Profile.totalReturns < 3)
            {
                return "장례자: 또 돌아왔군. 이번에는 네 이름에 반응한 잔광을 놓치지 마.";
            }

            return "장례자: 회귀가 익숙해질수록 더 위험하지. 익숙함은 늘 네 일부를 가져간다.";
        }

        private void TryAttack()
        {
            ActorRuntime target = FindAdjacentEnemy();
            if (target == null)
            {
                SetMessage("닿는 거리의 적이 없다.");
                return;
            }

            int damage = Mathf.Max(1, Run.EffectiveAttack - target.Defense);
            target.Hp -= damage;
            SetMessage($"{target.DisplayName}에게 {damage} 피해를 입혔다.");

            if (target.Hp <= 0)
            {
                KillEnemy(target);
            }

            ConsumeTurn(null);
        }

        private void TryInteract()
        {
            if (IsAdjacentOrStanding(player.Position, layout.MerchantPosition))
            {
                TryBuyDraught();
                return;
            }

            if (player.Position == layout.ReliquaryPosition && !reliquaryClaimed)
            {
                reliquaryClaimed = true;
                SetMessage("네 이름에 반응한 잔광을 회수했다. 회랑의 맥박이 잠시 잦아든다.");
                EndRun(RunEndReason.DepthClear);
                return;
            }

            for (int i = 0; i < layout.TemporaryWeaponSpawns.Count; i++)
            {
                if (player.Position == layout.TemporaryWeaponSpawns[i] && !temporaryWeaponCollected)
                {
                    temporaryWeaponCollected = true;
                    Run.TemporaryWeaponEquipped = true;
                    Run.AttackBonus = 1;
                    SetMessage("마모된 톱날을 주워 들었다. 이번 런 동안 공격력이 1 증가한다.");
                    ConsumeTurn(null);
                    return;
                }
            }

            SetMessage("상호작용할 대상이 없다.");
        }

        private void TryBuyDraught()
        {
            if (Run.Hyeolpyeon < 3)
            {
                SetMessage("혈편이 부족하다. 상인은 3조각 아래로는 물건을 넘기지 않는다.");
                return;
            }

            if (draughtStock <= 0)
            {
                SetMessage("상인의 상자는 비어 있다. 더는 살 수 있는 것이 없다.");
                return;
            }

            Run.Hyeolpyeon -= 3;
            Run.DraughtCount += 1;
            draughtStock -= 1;
            SetMessage("응급 회복 소모품을 샀다. 이 약은 이번 런에서만 유지된다.");
            ConsumeTurn(null);
        }

        private void UseBandage()
        {
            if (Run.BandageCount <= 0)
            {
                SetMessage("응고 지혈포가 남아 있지 않다.");
                return;
            }

            if (Run.CurrentHp >= Run.MaxHp)
            {
                SetMessage("이미 최대 체력이다.");
                return;
            }

            Run.BandageCount -= 1;
            Run.CurrentHp = Mathf.Min(Run.MaxHp, Run.CurrentHp + 4);
            player.Hp = Run.CurrentHp;
            SetMessage("응고 지혈포를 사용해 상처를 묶었다.");
            ConsumeTurn(null);
        }

        private void UseDraught()
        {
            if (Run.DraughtCount <= 0)
            {
                SetMessage("응급약이 남아 있지 않다.");
                return;
            }

            if (Run.CurrentHp >= Run.MaxHp)
            {
                SetMessage("이미 최대 체력이다.");
                return;
            }

            Run.DraughtCount -= 1;
            Run.CurrentHp = Mathf.Min(Run.MaxHp, Run.CurrentHp + 6);
            player.Hp = Run.CurrentHp;
            SetMessage("혈편으로 산 응급약이 상처를 억지로 봉합한다.");
            ConsumeTurn(null);
        }

        private void ConsumeTurn(string overrideMessage)
        {
            Run.TurnCount += 1;
            UpdateEnemies();
            if (!string.IsNullOrEmpty(overrideMessage))
            {
                SetMessage(overrideMessage);
            }

            if (Run.CurrentHp <= 0 && Phase == GamePhase.InRun)
            {
                EndRun(RunEndReason.Death);
                return;
            }

            UpdateCamera();
        }

        private void UpdateEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                ActorRuntime enemy = enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.ActionInterval > 1 && Run.TurnCount % enemy.ActionInterval != 0)
                {
                    continue;
                }

                int distance = Manhattan(enemy.Position, player.Position);
                if (distance == 1)
                {
                    EnemyAttack(enemy);
                    if (Run.CurrentHp <= 0)
                    {
                        return;
                    }

                    continue;
                }

                Vector2Int step = GetStepTowards(enemy.Position, player.Position);
                Vector2Int target = enemy.Position + step;
                if (step != Vector2Int.zero && layout.IsWalkable(target) && target != player.Position && !IsEnemyOccupied(target))
                {
                    enemy.Position = target;
                    SyncActorView(enemy);
                }
            }
        }

        private void EnemyAttack(ActorRuntime enemy)
        {
            int damage = Mathf.Max(1, enemy.Attack - 1);
            Run.CurrentHp = Mathf.Max(0, Run.CurrentHp - damage);
            player.Hp = Run.CurrentHp;
            SetMessage($"{enemy.DisplayName}의 공격으로 {damage} 피해를 입었다.");
        }

        private void KillEnemy(ActorRuntime enemy)
        {
            enemy.IsAlive = false;
            if (enemy.View != null)
            {
                enemy.View.SetActive(false);
            }

            Run.Hyeolpyeon += enemy.BloodReward;
            SetMessage($"{enemy.DisplayName}를 쓰러뜨렸다. 혈편 {enemy.BloodReward}개를 얻었다.");
        }

        private ActorRuntime FindAdjacentEnemy()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                ActorRuntime enemy = enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (Manhattan(enemy.Position, player.Position) == 1)
                {
                    return enemy;
                }
            }

            return null;
        }

        private bool TryGetEnemyAt(Vector2Int cell, out ActorRuntime enemyAtCell)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                ActorRuntime enemy = enemies[i];
                if (enemy.IsAlive && enemy.Position == cell)
                {
                    enemyAtCell = enemy;
                    return true;
                }
            }

            enemyAtCell = null;
            return false;
        }

        private bool IsEnemyOccupied(Vector2Int cell)
        {
            return TryGetEnemyAt(cell, out _);
        }

        private Vector2Int GetStepTowards(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
            }

            if (delta.y != 0)
            {
                return new Vector2Int(0, delta.y > 0 ? 1 : -1);
            }

            if (delta.x != 0)
            {
                return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
            }

            return Vector2Int.zero;
        }

        private void SpawnActors()
        {
            player = new ActorRuntime
            {
                Kind = ActorKind.Player,
                Brain = BrainType.Player,
                DisplayName = "재를 뒤집어쓴 기사",
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
                DisplayName = kind == ActorKind.AshCrawler ? "재 긁개" : "흑철 문지기",
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
            DungeonLayout newLayout = new DungeonLayout
            {
                PlayerSpawn = new Vector2Int(0, 0),
                MerchantPosition = new Vector2Int(9, -1),
                ReliquaryPosition = new Vector2Int(14, 0),
            };

            AddRoom(newLayout, -1, -2, 5, 2, 1);
            AddCorridor(newLayout, 5, 0, 9, 1);
            AddRoom(newLayout, 8, -2, 11, 2, 1);
            AddCorridor(newLayout, 11, 0, 14, 2);
            AddRoom(newLayout, 12, -2, 15, 2, 2);
            newLayout.TemporaryWeaponSpawns.Add(new Vector2Int(10, 2));

            return newLayout;
        }

        private void AddRoom(DungeonLayout target, int minX, int minY, int maxX, int maxY, int depth)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    target.Walkable.Add(cell);
                    target.DepthByCell[cell] = depth;
                }
            }
        }

        private void AddCorridor(DungeonLayout target, int minX, int y, int maxX, int depth)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                target.Walkable.Add(cell);
                target.DepthByCell[cell] = depth;
            }
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

            Vector3 target = player != null
                ? new Vector3(player.Position.x, player.Position.y, -10f)
                : new Vector3(0f, 0f, -10f);
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

        private bool IsAdjacentOrStanding(Vector2Int a, Vector2Int b)
        {
            return a == b || Manhattan(a, b) == 1;
        }

        private int Manhattan(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private string GetDepthName(int depth)
        {
            return depth <= 1 ? "회랑 전초부" : "회랑 심부";
        }

        private void SetMessage(string message)
        {
            hud.CurrentMessage = message;
        }

        private void SeedDialogue()
        {
            undertakerLines.Clear();
            undertakerLines.Enqueue("장례자: 성혈 아래에 묻힌 이름은 쉽게 돌아오지 않는다. 하지만 잔광은 거짓말을 덜 하지.");
            undertakerLines.Enqueue("장례자: 잿빛 회랑에서 돌아오면 네 장비는 거의 남지 않는다. 남는 건 네가 버티며 움켜쥔 잔광뿐이지.");
            undertakerLines.Enqueue("장례자: 혈편은 회랑에서만 통한다. 제단으로 돌아오는 순간 재와 함께 사라진다.");
        }
    }
}

using System.Collections.Generic;
using System.Text;
using Arkeum.Production.Core;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using Arkeum.Production.Presentation.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [Header("Health")] 
        [SerializeField] private GameObject healthBar;
        [SerializeField] private Image healthPrefab;
        [SerializeField] private Sprite[] healthSprites; // 0=>Filled, 1=>Empty
        private List<Image> healthImages;

        [Header("Cost")]
        [SerializeField] private TextMeshProUGUI goldText;
        
        [Header("Inventory")]
        [SerializeField] private Image inventoryImage;
        [SerializeField] private Image weaponIconImage;
        [SerializeField] private Sprite[] inventorySprites; // 0=>Normal, 1=>Active
        
        [Header("Top Status")]
        //[SerializeField] private Text topStatusText;
        [SerializeField] private Text topDetailsText;
        [SerializeField] private Text topTimingText;
        [SerializeField] private Text topRuleText;

        [Header("Controls")]
        [SerializeField] private Text controlsBodyText;
        [SerializeField] private Text stateText;
        [SerializeField] private Toggle preparedTargetToggle;

        [Header("Log")]
        [SerializeField] private Text logMessageText;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Text weaponText;

        [Header("Run Result")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultLostText;
        [SerializeField] private Text resultKeptText;

        [Header("Shop")]
        [SerializeField] private ShopOfferPopupPresenter shopOfferPopupPresenter;

        private readonly List<string> lostLines = new List<string>();
        private readonly List<string> keptLines = new List<string>();

        private GameDirector gameDirector;
        private RunState boundRun;
        private ActorEntity boundPlayer;
        private SaveProfile boundProfile;
        private RunController boundRunController;
        private bool missingReferencesLogged;

        public string CurrentMessage { get; private set; } = string.Empty;
        public string DialogueLine { get; private set; } = string.Empty;
        public IReadOnlyList<string> LostLines => lostLines;
        public IReadOnlyList<string> KeptLines => keptLines;

        // 게임 상 1번 초기화 하는 코드
        public void Initialize(GameDirector director)
        {
            gameDirector = director;
            InitializeShopOfferPopup();
            if (preparedTargetToggle != null)
            {
                preparedTargetToggle.onValueChanged.RemoveListener(OnPreparedTargetToggleChanged);
                preparedTargetToggle.onValueChanged.AddListener(OnPreparedTargetToggleChanged);
            }

            // Init HP Component
            TryInitializeHpList();

            Refresh();
        }

        // UI가 초기화 될 때 사용 (런을 시작 / 허브로 귀환 등)
        public void BindRun(RunState runState)
        {
            boundRun = runState;
            boundProfile = gameDirector?.ActiveProfile;
            boundPlayer = boundRun?.Player;
            boundRunController = gameDirector?.CurrentRunController;

            BindProfileEvents();
            BindPlayerEvents();
            BindRunControllerEvents();
            
            // Init HP UI
            ToggleHpUi(runState != null);

            if (runState != null)
            {
                UpdateMaxHpUi();
                UpdateCurrentHpUi();
            }
            
            UpdateGoldUi();
            UpdateInventoryUi();

            Refresh();
        }

        private void BindProfileEvents()
        {
            UnbindProfileEvents();
            
            if (boundProfile != null)
            {
                boundProfile.GoldChanged += UpdateGoldUi;
            }
        }

        private void UnbindProfileEvents()
        {
            if (boundProfile == null)
            {
                return;
            }

            boundProfile.GoldChanged -= UpdateGoldUi;
        }

        private void BindPlayerEvents()
        {
            UnbindPlayerEvents();

            if (boundPlayer != null)
            {
                boundPlayer.CurrentHpChanged += UpdateCurrentHpUi;
                boundPlayer.MaxHpChanged += UpdateMaxHpUi;
            }
        }

        private void UnbindPlayerEvents()
        {
            if (boundPlayer == null)
            {
                return;
            }

            boundPlayer.CurrentHpChanged -= UpdateCurrentHpUi;
            boundPlayer.MaxHpChanged -= UpdateMaxHpUi;
        }

        private void BindRunControllerEvents()
        {
            UnbindRunControllerEvents();

            if (boundRunController != null)
            {
                boundRunController.WeaponPickedUp += UpdateInventoryUi;
            }
        }

        private void UnbindRunControllerEvents()
        {
            if (boundRunController == null)
            {
                return;
            }
            
            boundRunController.WeaponPickedUp -= UpdateInventoryUi;
        }

        public void SetMessage(string message)
        {
            CurrentMessage = message ?? string.Empty;
            Refresh();
        }

        public void SetDialogue(string dialogue)
        {
            DialogueLine = dialogue ?? string.Empty;
            Refresh();
        }

        public void SetRunResult(IReadOnlyList<string> lost, IReadOnlyList<string> kept)
        {
            lostLines.Clear();
            keptLines.Clear();

            if (lost != null)
            {
                for (int i = 0; i < lost.Count; i++)
                {
                    lostLines.Add(lost[i]);
                }
            }

            if (kept != null)
            {
                for (int i = 0; i < kept.Count; i++)
                {
                    keptLines.Add(kept[i]);
                }
            }

            Refresh();
        }

        public void ClearRunResult()
        {
            lostLines.Clear();
            keptLines.Clear();
            Refresh();
        }

        private void OnDestroy()
        {
            if (preparedTargetToggle != null)
            {
                preparedTargetToggle.onValueChanged.RemoveListener(OnPreparedTargetToggleChanged);
            }

            UnbindPlayerEvents();
            UnbindProfileEvents();
            UnbindRunControllerEvents();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (gameDirector == null)
            {
                shopOfferPopupPresenter?.Hide();
                return;
            }

            HasRequiredReferences();
            bool inRun = gameDirector.CurrentState == GameState.InRun || gameDirector.CurrentState == GameState.TimingChallenge;
            WorldPresenter worldPresenter = gameDirector.Services?.WorldPresenter;
            shopOfferPopupPresenter?.Refresh(
                boundRunController,
                boundProfile != null ? boundProfile.Gold : 0,
                inRun,
                worldPresenter);
            if (inRun && boundRun != null && boundRun.Player != null)
            {
                //BandageCount
                //TurnCount
                //CurrentFloor
                //HasEquippedWeapon
                //Timing O/X
                UpdateInventoryUi();
            }
            else
            {
                // topDetailsText.text = gameDirector.ActiveProfile != null
                //     ? $"Gold {gameDirector.ActiveProfile.Gold}  |  Returns {gameDirector.ActiveProfile.TotalReturns}"
                //     : string.Empty;
                // topTimingText.text = string.Empty;
                // topRuleText.text = string.Empty;
            }

            // controlsBodyText.text = BuildControlsText();
            // stateText.text = gameDirector.CurrentState.ToString();
            // RefreshPreparedTargetToggle(inRun);
            //
            // logMessageText.text = string.IsNullOrEmpty(CurrentMessage) ? "..." : CurrentMessage;
            // dialogueText.text = DialogueLine;
            // dialogueText.gameObject.SetActive(!string.IsNullOrEmpty(DialogueLine));
            //
            // bool showWeapon = inRun && boundRun != null;
            // weaponText.gameObject.SetActive(showWeapon);
            // if (showWeapon)
            // {
            //     builder.Clear();
            //     builder.Append("Weapon: ");
            //     builder.Append(FormatWeaponLine(boundRun));
            //     weaponText.text = builder.ToString();
            // }
            //
            // bool showResult = gameDirector.CurrentState == GameState.RunResult;
            // resultPanel.SetActive(showResult);
            // if (showResult)
            // {
            //     resultLostText.text = BuildResultText("Lost", lostLines);
            //     resultKeptText.text = BuildResultText("Kept", keptLines) + "\n\nPress Enter to return to the altar.";
            // }
        }

        private void InitializeShopOfferPopup()
        {
            if (shopOfferPopupPresenter == null)
            {
                GameObject popupObject = GameObject.Find("ShopOfferPopup");
                if (popupObject != null)
                {
                    shopOfferPopupPresenter = popupObject.GetComponent<ShopOfferPopupPresenter>();
                    if (shopOfferPopupPresenter == null)
                    {
                        shopOfferPopupPresenter = popupObject.AddComponent<ShopOfferPopupPresenter>();
                    }
                }
            }

            shopOfferPopupPresenter?.Initialize();
        }

        private bool HasRequiredReferences()
        {
            bool hasReferences =
                topDetailsText != null &&
                topTimingText != null &&
                topRuleText != null &&
                controlsBodyText != null &&
                stateText != null &&
                preparedTargetToggle != null &&
                logMessageText != null &&
                dialogueText != null &&
                weaponText != null &&
                resultPanel != null &&
                resultLostText != null &&
                resultKeptText != null;

            if (!hasReferences && !missingReferencesLogged)
            {
                missingReferencesLogged = true;
                Debug.LogWarning("[HudPresenter] UGUI references are not fully assigned. Create the HUD Canvas manually and wire the serialized fields in the Inspector.", this);
            }

            return hasReferences;
        }

        #region HP UI
        private bool TryInitializeHpList()
        {
            if(healthBar == null)
            {
                Debug.LogWarning("[HudPresenter] HealthBar is Missing");
                return false;
            }

            healthImages = new List<Image>();
            foreach(var image in healthBar.GetComponentsInChildren<Image>())
            {
                healthImages.Add(image);
            }
            return true;
        }

        public void UpdateMaxHpUi()
        {
            if (boundRun == null || boundRun.Player == null)
            {
                Debug.LogWarning("[HudPresenter] Run Data is Not Valid");
                return;
            }
            if (healthBar == null)
            {
                Debug.LogWarning("[HudPresenter] Health Bar is Missing");
                return;
            }
            if (healthImages == null)
            {
                if (!TryInitializeHpList())
                {
                    return;
                }
            }

            //MAX HP ENSURE
            int maxHp = boundRun.Player.Stats.MaxHp;
            int curMaxHp = healthBar.transform.childCount;
            
            if (curMaxHp > maxHp)
            {
                while (healthBar.transform.childCount > maxHp)
                {
                    Transform child = healthBar.transform.GetChild(healthBar.transform.childCount - 1);
                    healthImages.Remove(child.GetComponent<Image>());
                    Destroy(child.gameObject);
                }
            }
            else if (curMaxHp < maxHp)
            {
                while (healthBar.transform.childCount < maxHp)
                {
                    Image hp = Instantiate(healthPrefab, healthBar.transform);
                    healthImages.Add(hp);
                }
            }
        }

        public void UpdateCurrentHpUi()
        {
            if (boundRun == null || boundRun.Player == null)
            {
                Debug.LogWarning("[HudPresenter] Run Data is Not Valid");
                return;
            }
            if(healthSprites == null || healthSprites.Length < 2)
            {
                Debug.LogWarning("[HudPresenter] HealthSprite is Missing");
                return;
            }
            if (healthImages == null)
            {
                if (!TryInitializeHpList())
                {
                    return;
                }
            }

            int curHp = boundRun.Player.CurrentHp;
            for (int i = 0; i < healthImages.Count; i++)
            {
                if (i <= curHp)
                {
                    healthImages[i].sprite = healthSprites[0];
                }
                else
                {
                    healthImages[i].sprite = healthSprites[1];
                }
            }
        }

        public void ToggleHpUi(bool isOn)
        {
            if(isOn != healthBar.activeSelf)
            {
                healthBar.SetActive(isOn);
            }
        }
        #endregion

        public void UpdateGoldUi()
        {
            if (gameDirector == null || gameDirector.ActiveProfile == null)
            {
                Debug.LogWarning("[HudPresenter] Game Director is Not Valid");
                return;
            }
            if (goldText == null)
            {
                Debug.LogWarning("[HudPresenter] goldText is Missing");
                return;
            }

            goldText.text = gameDirector.ActiveProfile.Gold.ToString();
        }

        public void UpdateInventoryUi()
        {
            if (boundRun == null)
            {
                inventoryImage?.gameObject.SetActive(false);
                
                Debug.LogWarning("[HudPresenter] Run Data is Not Valid");
                return;
            }
            
            inventoryImage?.gameObject.SetActive(true);
            
            // 타이밍 UI Update
            if (inventoryImage != null && inventorySprites != null && inventorySprites.Length >= 2)
            {
                if (boundRun.IsTimingModeEnabled)
                {
                    inventoryImage.sprite = inventorySprites[1];
                }
                else
                {
                    inventoryImage.sprite = inventorySprites[0];
                }
            }
            else
            {
                Debug.LogWarning("[HudPresenter] inventoryImage is Missing");
            }
            
            // 무기 아이콘 Update
            if (weaponIconImage != null)
            {
                WeaponDefinition weapon = boundRun.EquippedWeapon;
                bool hasWeaponIcon = boundRun.HasEquippedWeapon && weapon != null && weapon.Sprite != null;

                weaponIconImage.gameObject.SetActive(hasWeaponIcon);
                if (hasWeaponIcon)
                {
                    weaponIconImage.sprite = weapon.Sprite;
                    weaponIconImage.color = weapon.Tint;
                }
            }
            else
            {
                Debug.LogWarning("[HudPresenter] weaponIconImage is Missing");
            }
        }

        #region PreparedTarget UI
        private void OnPreparedTargetToggleChanged(bool show)
        {
            if (gameDirector?.Services?.WorldPresenter == null)
            {
                return;
            }

            gameDirector.Services.WorldPresenter.SetShowEnemyPreparedTargetMarkers(show);
            gameDirector.Services.WorldPresenter.Refresh();
        }
        #endregion
    }
}

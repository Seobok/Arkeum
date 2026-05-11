using System.Collections.Generic;
using System.Text;
using Arkeum.Production.Core;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [Header("Health")] 
        [SerializeField] private GameObject healthBar;
        [SerializeField] private GameObject healthPrefab;
        [SerializeField] private Sprite[] healthSprites;
        private List<GameObject> healthUis = new List<GameObject>();
        
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

        private readonly List<string> lostLines = new List<string>();
        private readonly List<string> keptLines = new List<string>();
        private readonly StringBuilder builder = new StringBuilder(256);

        private GameDirector gameDirector;
        private RunState boundRun;
        private bool missingReferencesLogged;

        public string CurrentMessage { get; private set; } = string.Empty;
        public string DialogueLine { get; private set; } = string.Empty;
        public IReadOnlyList<string> LostLines => lostLines;
        public IReadOnlyList<string> KeptLines => keptLines;

        public void Initialize(GameDirector director)
        {
            gameDirector = director;
            if (preparedTargetToggle != null)
            {
                preparedTargetToggle.onValueChanged.RemoveListener(OnPreparedTargetToggleChanged);
                preparedTargetToggle.onValueChanged.AddListener(OnPreparedTargetToggleChanged);
            }

            Refresh();
        }

        public void BindRun(RunState runState)
        {
            boundRun = runState;
            Refresh();
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
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (gameDirector == null || !HasRequiredReferences())
            {
                return;
            }

            bool inRun = gameDirector.CurrentState == GameState.InRun || gameDirector.CurrentState == GameState.TimingChallenge;
            if (inRun && boundRun != null && boundRun.Player != null)
            {
                //topStatusText.text = $"HP {boundRun.Player.CurrentHp}/{boundRun.Player.Stats.MaxHp}  |  Shards {boundRun.BloodShards}  |  Bandage {boundRun.BandageCount}  |  Turn {boundRun.TurnCount}";
                
                //MAX HP ENSURE
                int maxHp = boundRun.Player.Stats.MaxHp;
                int curMaxHp = healthBar.transform.childCount;

                if (curMaxHp > maxHp)
                {
                    while (healthBar.transform.childCount > maxHp)
                    {
                        Destroy(healthBar.transform.GetChild(healthBar.transform.childCount - 1));
                    }
                }
                else if (curMaxHp < maxHp)
                {
                    
                }
                
                topDetailsText.text = $"Floor {boundRun.CurrentFloor}  |  Weapon {(boundRun.HasEquippedWeapon ? "Equipped" : "None")}";
                topTimingText.text = FormatTimingLine(boundRun);
                topRuleText.text = "Rule: every action gives enemies a response.";
            }
            else
            {
                //topStatusText.text = "Hub: Return Altar";
                topDetailsText.text = gameDirector.ActiveProfile != null
                    ? $"Gleam {gameDirector.ActiveProfile.Gleam}  |  Returns {gameDirector.ActiveProfile.TotalReturns}"
                    : string.Empty;
                topTimingText.text = string.Empty;
                topRuleText.text = string.Empty;
            }

            controlsBodyText.text = BuildControlsText();
            stateText.text = gameDirector.CurrentState.ToString();
            RefreshPreparedTargetToggle(inRun);

            logMessageText.text = string.IsNullOrEmpty(CurrentMessage) ? "..." : CurrentMessage;
            dialogueText.text = DialogueLine;
            dialogueText.gameObject.SetActive(!string.IsNullOrEmpty(DialogueLine));

            bool showWeapon = inRun && boundRun != null;
            weaponText.gameObject.SetActive(showWeapon);
            if (showWeapon)
            {
                builder.Clear();
                builder.Append("Weapon: ");
                builder.Append(FormatWeaponLine(boundRun));
                weaponText.text = builder.ToString();
            }

            bool showResult = gameDirector.CurrentState == GameState.RunResult;
            resultPanel.SetActive(showResult);
            if (showResult)
            {
                resultLostText.text = BuildResultText("Lost", lostLines);
                resultKeptText.text = BuildResultText("Kept", keptLines) + "\n\nPress Enter to return to the altar.";
            }
        }

        private bool HasRequiredReferences()
        {
            bool hasReferences =
                topStatusText != null &&
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

        private string BuildControlsText()
        {
            if (gameDirector.CurrentState == GameState.InRun || gameDirector.CurrentState == GameState.TimingChallenge)
            {
                return "Move keys: attack, interact, or move\nWait: Q\nItems: 1 bandage\nTiming: Next";
            }

            if (gameDirector.CurrentState == GameState.RunResult)
            {
                return "Close result: Enter";
            }

            return "Move: arrow keys / WASD\nInteract: bump the target in front of you";
        }

        private void RefreshPreparedTargetToggle(bool inRun)
        {
            bool canShow = inRun && gameDirector.Services?.WorldPresenter != null;
            preparedTargetToggle.gameObject.SetActive(canShow);
            if (!canShow)
            {
                return;
            }

            preparedTargetToggle.SetIsOnWithoutNotify(gameDirector.Services.WorldPresenter.ShowEnemyPreparedTargetMarkers);
        }

        private void OnPreparedTargetToggleChanged(bool show)
        {
            if (gameDirector?.Services?.WorldPresenter == null)
            {
                return;
            }

            gameDirector.Services.WorldPresenter.SetShowEnemyPreparedTargetMarkers(show);
            gameDirector.Services.WorldPresenter.Refresh();
        }

        private static string BuildResultText(string title, List<string> lines)
        {
            StringBuilder resultBuilder = new StringBuilder(title);
            if (lines.Count == 0)
            {
                resultBuilder.Append("\nNone");
                return resultBuilder.ToString();
            }

            for (int i = 0; i < lines.Count; i++)
            {
                resultBuilder.Append('\n');
                resultBuilder.Append(lines[i]);
            }

            return resultBuilder.ToString();
        }

        private static string FormatWeaponLine(RunState runState)
        {
            if (runState == null || !runState.HasEquippedWeapon)
            {
                return "Default blade";
            }

            WeaponDefinition weapon = runState.EquippedWeapon;
            if (weapon == null)
            {
                return "Weapon (+1 attack)";
            }

            return $"{weapon.DisplayName} (+{weapon.AttackBonus} attack)";
        }

        private static string FormatTimingLine(RunState runState)
        {
            if (runState == null)
            {
                return "Timing: Off";
            }

            string state = runState.IsTimingModeEnabled ? "On" : "Off";
            WeaponDefinition weapon = runState.EquippedWeapon;
            if (!runState.IsTimingModeEnabled || weapon == null)
            {
                return $"Timing: {state}";
            }

            return weapon.HasTimingChallenge ? $"Timing: {state}" : $"Timing: {state} (weapon has no challenge)";
        }
    }
}

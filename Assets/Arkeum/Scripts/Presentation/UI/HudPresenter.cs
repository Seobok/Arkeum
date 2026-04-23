using System.Collections.Generic;
using System.Text;
using Arkeum.Production.Core;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        private readonly List<string> lostLines = new List<string>();
        private readonly List<string> keptLines = new List<string>();
        private readonly StringBuilder builder = new StringBuilder(256);

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle panelStyle;
        private GUIStyle accentStyle;
        private GameDirector gameDirector;
        private RunState boundRun;

        public string CurrentMessage { get; private set; } = string.Empty;
        public string DialogueLine { get; private set; } = string.Empty;
        public IReadOnlyList<string> LostLines => lostLines;
        public IReadOnlyList<string> KeptLines => keptLines;

        public void Initialize(GameDirector director)
        {
            gameDirector = director;
        }

        public void BindRun(RunState runState)
        {
            boundRun = runState;
        }

        public void SetMessage(string message)
        {
            CurrentMessage = message;
        }

        public void SetDialogue(string dialogue)
        {
            DialogueLine = dialogue;
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
        }

        public void ClearRunResult()
        {
            lostLines.Clear();
            keptLines.Clear();
        }

        private void OnGUI()
        {
            if (gameDirector == null)
            {
                return;
            }

            EnsureStyles();
            DrawTopBar();
            DrawHelp();
            DrawBottomLog();

            if (gameDirector.CurrentState == GameState.RunResult)
            {
                DrawRunResult();
            }
        }

        private void DrawTopBar()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 500f, 150f), panelStyle);
            GUILayout.Label("Arkeum Production Preview", titleStyle);

            if (gameDirector.CurrentState == GameState.InRun && boundRun != null && boundRun.Player != null)
            {
                GUILayout.Label(
                    $"HP {boundRun.Player.CurrentHp}/{boundRun.Player.Stats.MaxHp}  |  Shards {boundRun.BloodShards}  |  Bandage {boundRun.BandageCount}  |  Draught {boundRun.DraughtCount}  |  Turn {boundRun.TurnCount}",
                    bodyStyle);
                GUILayout.Label($"Depth {boundRun.DepthReached}  |  Temporary weapon {(boundRun.TemporaryWeaponEquipped ? "Equipped" : "None")}", bodyStyle);
                GUILayout.Label("Rule: every action gives enemies a response.", accentStyle);
            }
            else
            {
                GUILayout.Label("Hub: Return Altar", bodyStyle);
                GUILayout.Label(
                    $"Gleam {gameDirector.ActiveProfile.Gleam}  |  Returns {gameDirector.ActiveProfile.TotalReturns}  |  Best depth {gameDirector.ActiveProfile.HighestDepth}",
                    bodyStyle);
                GUILayout.Label(
                    gameDirector.ActiveProfile.StartingBandageUnlocked
                        ? "Starting bandage unlock: active"
                        : "Starting bandage unlock: locked",
                    accentStyle);
            }

            GUILayout.EndArea();
        }

        private void DrawHelp()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 330f, 12f, 318f, 220f), panelStyle);
            GUILayout.Label("Controls", titleStyle);
            if (gameDirector.CurrentState == GameState.InRun)
            {
                GUILayout.Label("Move keys: attack, interact, or move", bodyStyle);
                GUILayout.Label("Wait: Q", bodyStyle);
                GUILayout.Label("Items: 1 bandage / 2 draught", bodyStyle);
                DrawRunOptions();
            }
            else if (gameDirector.CurrentState == GameState.RunResult)
            {
                GUILayout.Label("Close result: Enter", bodyStyle);
            }
            else
            {
                GUILayout.Label("Move: arrow keys / WASD", bodyStyle);
                GUILayout.Label("Interact: bump the target in front of you", bodyStyle);
            }

            GUILayout.Space(12f);
            GUILayout.Label("State", titleStyle);
            GUILayout.Label(gameDirector.CurrentState.ToString(), bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawRunOptions()
        {
            if (gameDirector.Services?.WorldPresenter == null)
            {
                return;
            }

            GUILayout.Space(8f);
            GUILayout.Label("Options", titleStyle);
            bool showPreparedTargetMarkers = GUILayout.Toggle(
                gameDirector.Services.WorldPresenter.ShowEnemyPreparedTargetMarkers,
                "Prepared target tiles",
                bodyStyle);
            if (showPreparedTargetMarkers != gameDirector.Services.WorldPresenter.ShowEnemyPreparedTargetMarkers)
            {
                gameDirector.Services.WorldPresenter.SetShowEnemyPreparedTargetMarkers(showPreparedTargetMarkers);
                gameDirector.Services.WorldPresenter.Refresh();
            }
        }

        private void DrawBottomLog()
        {
            GUILayout.BeginArea(new Rect(12f, Screen.height - 148f, Screen.width - 24f, 136f), panelStyle);
            GUILayout.Label("Log", titleStyle);
            GUILayout.Label(string.IsNullOrEmpty(CurrentMessage) ? "..." : CurrentMessage, bodyStyle);

            if (!string.IsNullOrEmpty(DialogueLine))
            {
                GUILayout.Space(6f);
                GUILayout.Label(DialogueLine, accentStyle);
            }

            if (gameDirector.CurrentState == GameState.InRun && boundRun != null)
            {
                builder.Clear();
                builder.Append("Weapon: ");
                builder.Append(boundRun.TemporaryWeaponEquipped ? "Worn blade (+1 attack)" : "Default blade");
                GUILayout.Space(6f);
                GUILayout.Label(builder.ToString(), bodyStyle);
            }

            GUILayout.EndArea();
        }

        private void DrawRunResult()
        {
            Rect rect = new Rect(Screen.width * 0.5f - 250f, Screen.height * 0.5f - 190f, 500f, 380f);
            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Label("Run Result", titleStyle);
            GUILayout.Space(8f);

            GUILayout.Label("Lost", titleStyle);
            for (int i = 0; i < lostLines.Count; i++)
            {
                GUILayout.Label(lostLines[i], bodyStyle);
            }

            GUILayout.Space(8f);
            GUILayout.Label("Kept", titleStyle);
            for (int i = 0; i < keptLines.Count; i++)
            {
                GUILayout.Label(keptLines[i], bodyStyle);
            }

            GUILayout.Space(12f);
            GUILayout.Label("Press Enter to return to the altar.", accentStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.92f, 0.84f) }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = new Color(0.84f, 0.82f, 0.76f) }
            };

            accentStyle = new GUIStyle(bodyStyle)
            {
                normal = { textColor = new Color(0.95f, 0.63f, 0.46f) }
            };

            Texture2D panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            panelTexture.SetPixel(0, 0, new Color(0.09f, 0.06f, 0.07f, 0.88f));
            panelTexture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 12, 12),
                normal = { background = panelTexture }
            };
        }
    }
}

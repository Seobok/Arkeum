using System.Text;
using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed class PrototypeHud
    {
        private readonly StringBuilder builder = new StringBuilder(512);
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle panelStyle;
        private GUIStyle accentStyle;

        public string CurrentMessage { get; set; } = string.Empty;
        public string DialogueLine { get; set; } = string.Empty;

        public void Draw(PrototypeGameController controller)
        {
            EnsureStyles();

            DrawTopBar(controller);
            DrawHelp(controller);
            DrawBottomLog(controller);

            if (controller.Phase == GamePhase.Hub)
            {
                DrawHubPanel(controller);
            }

            if (controller.Phase == GamePhase.RunResult)
            {
                DrawRunResult(controller);
            }
        }

        private void DrawTopBar(PrototypeGameController controller)
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 460f, 140f), panelStyle);
            GUILayout.Label("Arkeum Prototype", titleStyle);

            if (controller.Phase == GamePhase.InRun)
            {
                RunState run = controller.Run;
                GUILayout.Label("Goal: reach the reliquary and return through action-by-action play.", bodyStyle);
                GUILayout.Label(
                    $"HP {run.CurrentHp}/{run.MaxHp}  |  Shards {run.Hyeolpyeon}  |  Bandage {run.BandageCount}  |  Draught {run.DraughtCount}  |  Turn {run.TurnCount}",
                    bodyStyle);
                GUILayout.Label($"Best depth {controller.Profile.highestReachedDepth}  |  Current depth {run.DepthReached}", bodyStyle);
                GUILayout.Label("Rule: every action gives enemies a response.", accentStyle);
            }
            else
            {
                GUILayout.Label("Hub: Return Altar", bodyStyle);
                GUILayout.Label($"Gleam {controller.Profile.jangwang}  |  Returns {controller.Profile.totalReturns}  |  Best depth {controller.Profile.highestReachedDepth}", bodyStyle);
                GUILayout.Label(
                    controller.Profile.unlockedStartingBandage
                        ? "Starting bandage unlock: active, runs begin with 1 bandage"
                        : $"Starting bandage unlock cost: {PrototypeGameController.StartingBandageUnlockCost} gleam",
                    accentStyle);
            }

            GUILayout.EndArea();
        }

        private void DrawHelp(PrototypeGameController controller)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 330f, 12f, 318f, 215f), panelStyle);
            GUILayout.Label("Controls", titleStyle);
            if (controller.Phase == GamePhase.InRun)
            {
                GUILayout.Label("Move keys: attack enemies, interact with front targets, otherwise move", bodyStyle);
                GUILayout.Label("Wait: Q", bodyStyle);
                GUILayout.Label("Items: 1 bandage / 2 draught", bodyStyle);
            }
            else
            {
                GUILayout.Label("Move: arrow keys / WASD", bodyStyle);
                GUILayout.Label("Interact: bump the target in front of you", bodyStyle);
                GUILayout.Label("Close result: Enter", bodyStyle);
            }
            GUILayout.Space(12f);
            GUILayout.Label("Prototype Checks", titleStyle);
            GUILayout.Label("Persistent progression after death", bodyStyle);
            GUILayout.Label("Action-by-action pacing", bodyStyle);
            GUILayout.Label("Loss vs. persistence tension", bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawBottomLog(PrototypeGameController controller)
        {
            GUILayout.BeginArea(new Rect(12f, Screen.height - 148f, Screen.width - 24f, 136f), panelStyle);
            GUILayout.Label("Status", titleStyle);
            GUILayout.Label(string.IsNullOrEmpty(CurrentMessage) ? "..." : CurrentMessage, bodyStyle);

            if (!string.IsNullOrEmpty(DialogueLine))
            {
                GUILayout.Space(6f);
                GUILayout.Label(DialogueLine, accentStyle);
            }

            if (controller.Phase == GamePhase.InRun)
            {
                builder.Clear();
                builder.Append("Current weapon: ");
                builder.Append(controller.Run.TemporaryWeaponEquipped ? "Worn blade (+1 attack)" : "Default blade");
                GUILayout.Space(6f);
                GUILayout.Label(builder.ToString(), bodyStyle);
            }

            GUILayout.EndArea();
        }

        private void DrawHubPanel(PrototypeGameController controller)
        {
            GUILayout.BeginArea(new Rect(12f, 168f, 420f, 188f), panelStyle);
            GUILayout.Label("Hub Actions", titleStyle);
            GUILayout.Label("Bump the start altar to begin a run", bodyStyle);
            GUILayout.Label("Bump the unlock altar to attempt the starting bandage unlock", bodyStyle);
            GUILayout.Label("Bump the undertaker to talk", bodyStyle);
            GUILayout.Space(8f);
            GUILayout.Label(controller.GetHubSummary(), bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawRunResult(PrototypeGameController controller)
        {
            Rect rect = new Rect(Screen.width * 0.5f - 250f, Screen.height * 0.5f - 190f, 500f, 380f);
            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Label("Run Result", titleStyle);
            GUILayout.Label(controller.GetRunResultHeadline(), accentStyle);
            GUILayout.Space(8f);

            GUILayout.Label("Lost", titleStyle);
            foreach (string line in controller.GetLostResultLines())
            {
                GUILayout.Label(line, bodyStyle);
            }

            GUILayout.Space(8f);
            GUILayout.Label("Kept", titleStyle);
            foreach (string line in controller.GetKeptResultLines())
            {
                GUILayout.Label(line, bodyStyle);
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

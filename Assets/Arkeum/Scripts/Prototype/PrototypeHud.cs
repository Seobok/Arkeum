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
            GUILayout.Label("Arkeum 1단계 프로토타입", titleStyle);

            if (controller.Phase == GamePhase.InRun)
            {
                RunState run = controller.Run;
                GUILayout.Label($"목표: 잿빛 회랑으로 내려가 네 이름에 반응한 잔광을 회수하라.", bodyStyle);
                GUILayout.Label(
                    $"HP {run.CurrentHp}/{run.MaxHp}  |  혈편 {run.Hyeolpyeon}  |  붕대 {run.BandageCount}  |  응급약 {run.DraughtCount}  |  턴 {run.TurnCount}",
                    bodyStyle);
                GUILayout.Label($"최고 도달 구역: {controller.Profile.highestReachedDepth}  |  이번 런 도달 구역: {run.DepthReached}", bodyStyle);
                GUILayout.Label("규칙: 행동 후 적이 반응합니다.", accentStyle);
            }
            else
            {
                GUILayout.Label("거점: 귀환 제단", bodyStyle);
                GUILayout.Label($"잔광 {controller.Profile.jangwang}  |  총 회귀 {controller.Profile.totalReturns}  |  최고 도달 구역 {controller.Profile.highestReachedDepth}", bodyStyle);
                GUILayout.Label(
                    controller.Profile.unlockedStartingBandage
                        ? "시작 보급: 응고 지혈포 해금됨. 매 런 시작 시 1개 지급."
                        : $"응고 지혈포 해금 비용: {PrototypeGameController.StartingBandageUnlockCost} 잔광",
                    accentStyle);
            }

            GUILayout.EndArea();
        }

        private void DrawHelp(PrototypeGameController controller)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 330f, 12f, 318f, 215f), panelStyle);
            GUILayout.Label("조작", titleStyle);

            if (controller.Phase == GamePhase.InRun)
            {
                GUILayout.Label("방향 입력: 적이 있으면 공격, 없으면 이동", bodyStyle);
                GUILayout.Label("상호작용: E", bodyStyle);
                GUILayout.Label("대기: Q", bodyStyle);
                GUILayout.Label("소모품: 1 붕대 / 2 응급약", bodyStyle);
            }
            else
            {
                GUILayout.Label("이동: 화살표 / WASD", bodyStyle);
                GUILayout.Label("상호작용: E", bodyStyle);
                GUILayout.Label("결과창 닫기: Enter", bodyStyle);
            }

            GUILayout.Space(12f);
            GUILayout.Label("프로토타입 검증 포인트", titleStyle);
            GUILayout.Label("죽음 이후 회귀 흐름", bodyStyle);
            GUILayout.Label("행동 단위 리듬", bodyStyle);
            GUILayout.Label("혈편 소실 / 잔광 유지 감각", bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawBottomLog(PrototypeGameController controller)
        {
            GUILayout.BeginArea(new Rect(12f, Screen.height - 148f, Screen.width - 24f, 136f), panelStyle);
            GUILayout.Label("상태", titleStyle);
            GUILayout.Label(string.IsNullOrEmpty(CurrentMessage) ? "..." : CurrentMessage, bodyStyle);

            if (!string.IsNullOrEmpty(DialogueLine))
            {
                GUILayout.Space(6f);
                GUILayout.Label(DialogueLine, accentStyle);
            }

            if (controller.Phase == GamePhase.InRun)
            {
                builder.Clear();
                builder.Append("현재 임시 장비: ");
                builder.Append(controller.Run.TemporaryWeaponEquipped ? "마모된 톱날 (+1 공격)" : "기본 철검");
                GUILayout.Space(6f);
                GUILayout.Label(builder.ToString(), bodyStyle);
            }

            GUILayout.EndArea();
        }

        private void DrawHubPanel(PrototypeGameController controller)
        {
            GUILayout.BeginArea(new Rect(12f, 168f, 420f, 188f), panelStyle);
            GUILayout.Label("거점 기능", titleStyle);
            GUILayout.Label("하강 제단 타일: E로 런 시작", bodyStyle);
            GUILayout.Label("해금 제단 타일: E로 응고 지혈포 해금", bodyStyle);
            GUILayout.Label("장례자 타일: E로 대화", bodyStyle);
            GUILayout.Space(8f);
            GUILayout.Label(controller.GetHubSummary(), bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawRunResult(PrototypeGameController controller)
        {
            Rect rect = new Rect(Screen.width * 0.5f - 250f, Screen.height * 0.5f - 190f, 500f, 380f);
            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Label("런 종료", titleStyle);
            GUILayout.Label(controller.GetRunResultHeadline(), accentStyle);
            GUILayout.Space(8f);

            GUILayout.Label("사라진 것", titleStyle);
            foreach (string line in controller.GetLostResultLines())
            {
                GUILayout.Label(line, bodyStyle);
            }

            GUILayout.Space(8f);
            GUILayout.Label("남은 것", titleStyle);
            foreach (string line in controller.GetKeptResultLines())
            {
                GUILayout.Label(line, bodyStyle);
            }

            GUILayout.Space(12f);
            GUILayout.Label("Enter를 눌러 귀환 제단으로 돌아갑니다.", accentStyle);
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

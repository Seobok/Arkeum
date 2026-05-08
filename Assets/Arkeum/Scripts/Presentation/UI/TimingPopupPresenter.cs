using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class TimingPopupPresenter : MonoBehaviour
    {
        private TimingSession session;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle panelStyle;
        private GUIStyle trackStyle;
        private GUIStyle goodZoneStyle;
        private GUIStyle perfectZoneStyle;
        private GUIStyle markerStyle;

        public bool IsVisible => session != null;

        public void Initialize()
        {
            EnsureStyles();
        }

        public void Show(TimingSession timingSession)
        {
            session = timingSession;
        }

        public void Hide()
        {
            session = null;
        }

        private void OnGUI()
        {
            if (session == null)
            {
                return;
            }

            EnsureStyles();
            DrawPopup();
        }

        private void DrawPopup()
        {
            float width = Mathf.Min(520f, Screen.width - 48f);
            Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - 94f, width, 188f);

            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Label(session.Definition != null ? session.Definition.DisplayName : "Timing", titleStyle);
            GUILayout.Space(10f);
            DrawTimingBar(GUILayoutUtility.GetRect(width - 48f, 34f));
            GUILayout.Space(10f);
            GUILayout.Label("Press Attack when the marker reaches the center window.", bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawTimingBar(Rect rect)
        {
            ITimingChallengeRuntime runtime = session.Runtime;
            if (runtime == null)
            {
                return;
            }

            GUI.Box(rect, GUIContent.none, trackStyle);
            DrawZone(rect, runtime.GoodZoneMin, runtime.GoodZoneMax, goodZoneStyle);
            DrawZone(rect, runtime.PerfectZoneMin, runtime.PerfectZoneMax, perfectZoneStyle);

            float markerX = Mathf.Lerp(rect.xMin, rect.xMax, runtime.NormalizedPosition);
            Rect markerRect = new Rect(markerX - 3f, rect.yMin - 5f, 6f, rect.height + 10f);
            GUI.Box(markerRect, GUIContent.none, markerStyle);
        }

        private static void DrawZone(Rect trackRect, float min, float max, GUIStyle style)
        {
            float xMin = Mathf.Lerp(trackRect.xMin, trackRect.xMax, Mathf.Clamp01(min));
            float xMax = Mathf.Lerp(trackRect.xMin, trackRect.xMax, Mathf.Clamp01(max));
            GUI.Box(new Rect(xMin, trackRect.yMin, Mathf.Max(1f, xMax - xMin), trackRect.height), GUIContent.none, style);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.92f, 0.84f) }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.84f, 0.82f, 0.76f) }
            };

            panelStyle = BuildBoxStyle(new Color(0.09f, 0.06f, 0.07f, 0.94f), new RectOffset(18, 18, 18, 18));
            trackStyle = BuildBoxStyle(new Color(0.20f, 0.18f, 0.17f, 1f), new RectOffset());
            goodZoneStyle = BuildBoxStyle(new Color(0.66f, 0.38f, 0.20f, 1f), new RectOffset());
            perfectZoneStyle = BuildBoxStyle(new Color(0.92f, 0.72f, 0.31f, 1f), new RectOffset());
            markerStyle = BuildBoxStyle(new Color(0.95f, 0.95f, 0.90f, 1f), new RectOffset());
        }

        private static GUIStyle BuildBoxStyle(Color color, RectOffset padding)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            return new GUIStyle(GUI.skin.box)
            {
                padding = padding,
                normal = { background = texture }
            };
        }
    }
}

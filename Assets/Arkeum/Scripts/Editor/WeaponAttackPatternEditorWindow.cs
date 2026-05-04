#if UNITY_EDITOR
using Arkeum.Production.Gameplay.Run;
using UnityEditor;
using UnityEngine;

namespace Arkeum.Editor
{
    public sealed class WeaponAttackPatternEditorWindow : EditorWindow
    {
        private const float CellSize = 28f;
        private static readonly Color GridColor = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color AttackColor = new Color(0.75f, 0.43f, 0.18f);
        private static readonly Color OriginColor = new Color(0.91f, 0.86f, 0.78f);

        private WeaponDefinition selectedAsset;
        private Vector2 scrollPosition;

        [MenuItem("Arkeum/Weapon Attack Pattern Editor")]
        private static void OpenWindow()
        {
            GetWindow<WeaponAttackPatternEditorWindow>("Weapon Pattern");
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (selectedAsset == null)
            {
                EditorGUILayout.HelpBox("Select or create a WeaponDefinition to edit its attack range.", MessageType.Info);
                return;
            }

            DrawSettings();
            DrawGrid();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                WeaponDefinition nextAsset = (WeaponDefinition)EditorGUILayout.ObjectField(
                    selectedAsset,
                    typeof(WeaponDefinition),
                    false,
                    GUILayout.Width(280f));
                if (nextAsset != selectedAsset)
                {
                    selectedAsset = nextAsset;
                    Repaint();
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawSettings()
        {
            EditorGUI.BeginChangeCheck();
            Vector2Int editorMin = EditorGUILayout.Vector2IntField("Editor Min", selectedAsset.AttackEditorMin);
            Vector2Int editorMax = EditorGUILayout.Vector2IntField("Editor Max", selectedAsset.AttackEditorMax);
            bool rotateByFacing = EditorGUILayout.Toggle("Rotate By Facing", selectedAsset.RotateAttackByFacing);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedAsset, "Edit Weapon Attack Pattern Settings");
                selectedAsset.AttackEditorMin = Vector2Int.Min(editorMin, editorMax);
                selectedAsset.AttackEditorMax = Vector2Int.Max(editorMin, editorMax);
                selectedAsset.RotateAttackByFacing = rotateByFacing;
                MarkDirty();
            }

            EditorGUILayout.HelpBox("Click cells to toggle attack offsets. Right is the default facing direction.", MessageType.None);
        }

        private void DrawGrid()
        {
            int width = selectedAsset.AttackEditorMax.x - selectedAsset.AttackEditorMin.x + 1;
            int height = selectedAsset.AttackEditorMax.y - selectedAsset.AttackEditorMin.y + 1;
            if (width <= 0 || height <= 0)
            {
                EditorGUILayout.HelpBox("Editor bounds are invalid.", MessageType.Warning);
                return;
            }

            Rect viewRect = GUILayoutUtility.GetRect(
                position.width - 16f,
                Mathf.Min(position.height - 150f, height * CellSize + 16f),
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            Rect contentRect = new Rect(0f, 0f, width * CellSize, height * CellSize);
            scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, contentRect);
            Handles.BeginGUI();

            for (int y = selectedAsset.AttackEditorMax.y; y >= selectedAsset.AttackEditorMin.y; y--)
            {
                for (int x = selectedAsset.AttackEditorMin.x; x <= selectedAsset.AttackEditorMax.x; x++)
                {
                    Vector2Int offset = new Vector2Int(x, y);
                    Rect cellRect = GetCellRect(offset);
                    EditorGUI.DrawRect(cellRect, GetCellColor(offset));
                    Handles.color = GridColor;
                    Handles.DrawAAPolyLine(
                        new Vector3(cellRect.xMin, cellRect.yMin),
                        new Vector3(cellRect.xMax, cellRect.yMin),
                        new Vector3(cellRect.xMax, cellRect.yMax),
                        new Vector3(cellRect.xMin, cellRect.yMax),
                        new Vector3(cellRect.xMin, cellRect.yMin));

                    DrawCellOverlay(offset, cellRect);
                    HandleCellInput(offset, cellRect);
                }
            }

            Handles.EndGUI();
            GUI.EndScrollView();
        }

        private void DrawCellOverlay(Vector2Int offset, Rect cellRect)
        {
            if (offset == Vector2Int.zero)
            {
                GUI.Label(cellRect, "P", CenteredMiniLabel());
                return;
            }

            if (selectedAsset.AttackOffsets.Contains(offset))
            {
                GUI.Label(cellRect, "X", CenteredMiniLabel());
            }
        }

        private void HandleCellInput(Vector2Int offset, Rect cellRect)
        {
            Event current = Event.current;
            if (!cellRect.Contains(current.mousePosition))
            {
                return;
            }

            if (current.type == EventType.MouseDown && current.button == 0)
            {
                Undo.RecordObject(selectedAsset, "Edit Weapon Attack Pattern");
                selectedAsset.ToggleAttackOffset(offset);
                MarkDirty();
                current.Use();
            }
        }

        private Rect GetCellRect(Vector2Int offset)
        {
            float x = (offset.x - selectedAsset.AttackEditorMin.x) * CellSize;
            float y = (selectedAsset.AttackEditorMax.y - offset.y) * CellSize;
            return new Rect(x, y, CellSize, CellSize);
        }

        private Color GetCellColor(Vector2Int offset)
        {
            if (offset == Vector2Int.zero)
            {
                return OriginColor;
            }

            return selectedAsset.AttackOffsets.Contains(offset) ? AttackColor : Color.black;
        }

        private static GUIStyle CenteredMiniLabel()
        {
            return new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(selectedAsset);
            Repaint();
        }
    }
}
#endif

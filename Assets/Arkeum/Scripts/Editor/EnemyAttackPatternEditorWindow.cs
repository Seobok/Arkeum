#if UNITY_EDITOR
using Arkeum.Production.Gameplay.Actors;
using UnityEditor;
using UnityEngine;

namespace Arkeum.Editor
{
    public sealed class EnemyAttackPatternEditorWindow : EditorWindow
    {
        private const float CellSize = 28f;
        private static readonly Color GridColor = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color AttackColor = new Color(0.72f, 0.2f, 0.17f);
        private static readonly Color OriginColor = new Color(0.91f, 0.86f, 0.78f);

        private EnemyAttackPatternDefinition selectedAsset;
        private Vector2 scrollPosition;

        [MenuItem("Arkeum/Enemy Attack Pattern Editor")]
        private static void OpenWindow()
        {
            GetWindow<EnemyAttackPatternEditorWindow>("Attack Pattern");
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (selectedAsset == null)
            {
                EditorGUILayout.HelpBox("Select or create an EnemyAttackPattern asset to begin editing.", MessageType.Info);
                return;
            }

            DrawSettings();
            DrawGrid();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EnemyAttackPatternDefinition nextAsset = (EnemyAttackPatternDefinition)EditorGUILayout.ObjectField(
                    selectedAsset,
                    typeof(EnemyAttackPatternDefinition),
                    false,
                    GUILayout.Width(280f));
                if (nextAsset != selectedAsset)
                {
                    selectedAsset = nextAsset;
                    Repaint();
                }

                if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                {
                    CreateNewAsset();
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawSettings()
        {
            EditorGUI.BeginChangeCheck();
            Vector2Int editorMin = EditorGUILayout.Vector2IntField("Editor Min", selectedAsset.EditorMin);
            Vector2Int editorMax = EditorGUILayout.Vector2IntField("Editor Max", selectedAsset.EditorMax);
            bool rotateByFacing = EditorGUILayout.Toggle("Rotate By Facing", selectedAsset.RotateByFacing);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedAsset, "Edit Attack Pattern Settings");
                selectedAsset.EditorMin = Vector2Int.Min(editorMin, editorMax);
                selectedAsset.EditorMax = Vector2Int.Max(editorMin, editorMax);
                selectedAsset.RotateByFacing = rotateByFacing;
                MarkDirty();
            }

            EditorGUILayout.HelpBox("Click cells to toggle attack offsets. The center cell is the enemy position.", MessageType.None);
        }

        private void DrawGrid()
        {
            int width = selectedAsset.EditorMax.x - selectedAsset.EditorMin.x + 1;
            int height = selectedAsset.EditorMax.y - selectedAsset.EditorMin.y + 1;
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

            for (int y = selectedAsset.EditorMax.y; y >= selectedAsset.EditorMin.y; y--)
            {
                for (int x = selectedAsset.EditorMin.x; x <= selectedAsset.EditorMax.x; x++)
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
                GUI.Label(cellRect, "E", CenteredMiniLabel());
                return;
            }

            if (selectedAsset.Offsets.Contains(offset))
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
                Undo.RecordObject(selectedAsset, "Edit Attack Pattern");
                selectedAsset.ToggleOffset(offset);
                MarkDirty();
                current.Use();
            }
        }

        private Rect GetCellRect(Vector2Int offset)
        {
            float x = (offset.x - selectedAsset.EditorMin.x) * CellSize;
            float y = (selectedAsset.EditorMax.y - offset.y) * CellSize;
            return new Rect(x, y, CellSize, CellSize);
        }

        private Color GetCellColor(Vector2Int offset)
        {
            if (offset == Vector2Int.zero)
            {
                return OriginColor;
            }

            return selectedAsset.Offsets.Contains(offset) ? AttackColor : Color.black;
        }

        private void CreateNewAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Enemy Attack Pattern",
                "NewEnemyAttackPattern",
                "asset",
                "Choose a location for the attack pattern asset.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            EnemyAttackPatternDefinition asset = CreateInstance<EnemyAttackPatternDefinition>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            selectedAsset = asset;
            Selection.activeObject = asset;
        }

        private static GUIStyle CenteredMiniLabel()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            return style;
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(selectedAsset);
            Repaint();
        }
    }
}
#endif

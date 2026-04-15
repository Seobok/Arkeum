#if UNITY_EDITOR
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Map;
using UnityEditor;
using UnityEngine;

namespace Arkeum.Editor
{
    public sealed class MapAssetEditorWindow : EditorWindow
    {
        private const float CellSize = 28f;
        private static readonly Color GridColor = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color WalkableColor = new Color(0.23f, 0.2f, 0.2f);
        private static readonly Color DeepColor = new Color(0.18f, 0.14f, 0.25f);
        private static readonly Color WeaponColor = new Color(0.75f, 0.43f, 0.18f);
        private static readonly Color PlayerColor = new Color(0.91f, 0.86f, 0.78f);
        private static readonly Color MerchantColor = new Color(0.1f, 0.4f, 0.37f);
        private static readonly Color ReliquaryColor = new Color(0.76f, 0.65f, 0.17f);
        private static readonly Color StartColor = new Color(0.62f, 0.29f, 0.22f);
        private static readonly Color UnlockColor = new Color(0.84f, 0.73f, 0.28f);
        private static readonly Color UndertakerColor = new Color(0.19f, 0.55f, 0.51f);

        private MapTool selectedTool = MapTool.Walkable;
        private MapAsset selectedAsset;
        private Vector2 scrollPosition;
        private int brushDepth = 1;

        [MenuItem("Arkeum/Map Editor")]
        private static void OpenWindow()
        {
            GetWindow<MapAssetEditorWindow>("Map Editor");
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (selectedAsset == null)
            {
                EditorGUILayout.HelpBox("Select or create a MapAsset to begin editing.", MessageType.Info);
                return;
            }

            DrawBoundsEditor();
            DrawLegend();
            DrawValidation();
            DrawGrid();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                MapAsset nextAsset = (MapAsset)EditorGUILayout.ObjectField(selectedAsset, typeof(MapAsset), false, GUILayout.Width(280f));
                if (nextAsset != selectedAsset)
                {
                    selectedAsset = nextAsset;
                    Repaint();
                }

                if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                {
                    CreateNewAsset();
                }

                GUILayout.Space(8f);
                selectedTool = (MapTool)EditorGUILayout.EnumPopup(selectedTool, EditorStyles.toolbarPopup, GUILayout.Width(140f));
                brushDepth = EditorGUILayout.IntField(brushDepth, GUILayout.Width(40f));
                brushDepth = Mathf.Max(0, brushDepth);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Frame Cells", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                {
                    FrameToCells();
                }
            }
        }

        private void DrawBoundsEditor()
        {
            EditorGUI.BeginChangeCheck();
            Vector2Int editorMin = EditorGUILayout.Vector2IntField("Editor Min", selectedAsset.EditorMin);
            Vector2Int editorMax = EditorGUILayout.Vector2IntField("Editor Max", selectedAsset.EditorMax);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedAsset, "Edit Map Bounds");
                selectedAsset.EditorMin = Vector2Int.Min(editorMin, editorMax);
                selectedAsset.EditorMax = Vector2Int.Max(editorMin, editorMax);
                MarkDirty();
            }
        }

        private void DrawLegend()
        {
            EditorGUILayout.LabelField("Brushes", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Walkable/Erase edits terrain. Depth applies to Walkable. Marker brushes place unique points or toggle weapon spawns.",
                MessageType.None);
        }

        private void DrawValidation()
        {
            List<string> issues = BuildValidationIssues(selectedAsset);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Validation passed.", MessageType.Info);
                return;
            }

            for (int i = 0; i < issues.Count; i++)
            {
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
            }
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
                Mathf.Min(position.height - 180f, height * CellSize + 16f),
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            Rect contentRect = new Rect(0f, 0f, width * CellSize, height * CellSize);
            scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, contentRect);
            Handles.BeginGUI();

            for (int y = selectedAsset.EditorMax.y; y >= selectedAsset.EditorMin.y; y--)
            {
                for (int x = selectedAsset.EditorMin.x; x <= selectedAsset.EditorMax.x; x++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    Rect cellRect = GetCellRect(cell);
                    EditorGUI.DrawRect(cellRect, GetCellColor(cell));
                    Handles.color = GridColor;
                    Handles.DrawAAPolyLine(
                        new Vector3(cellRect.xMin, cellRect.yMin),
                        new Vector3(cellRect.xMax, cellRect.yMin),
                        new Vector3(cellRect.xMax, cellRect.yMax),
                        new Vector3(cellRect.xMin, cellRect.yMax),
                        new Vector3(cellRect.xMin, cellRect.yMin));

                    DrawCellOverlay(cell, cellRect);
                    HandleCellInput(cell, cellRect);
                }
            }

            Handles.EndGUI();
            GUI.EndScrollView();
        }

        private void DrawCellOverlay(Vector2Int cell, Rect cellRect)
        {
            if (selectedAsset.TryGetCell(cell, out MapCellData cellData) && cellData.Walkable)
            {
                GUI.Label(cellRect, cellData.Depth.ToString(), CenteredMiniLabel());
            }

            DrawMarker(cellRect, selectedAsset.PlayerSpawn == cell, "P", PlayerColor);
            DrawMarker(cellRect, selectedAsset.MerchantPosition == cell, "M", MerchantColor);
            DrawMarker(cellRect, selectedAsset.ReliquaryPosition == cell, "R", ReliquaryColor);
            DrawMarker(cellRect, selectedAsset.StartAltarPosition == cell, "S", StartColor);
            DrawMarker(cellRect, selectedAsset.UnlockAltarPosition == cell, "U", UnlockColor);
            DrawMarker(cellRect, selectedAsset.UndertakerPosition == cell, "T", UndertakerColor);

            if (selectedAsset.TemporaryWeaponSpawns.Contains(cell))
            {
                DrawMarker(cellRect, true, "W", WeaponColor);
            }
        }

        private void HandleCellInput(Vector2Int cell, Rect cellRect)
        {
            Event current = Event.current;
            if (!cellRect.Contains(current.mousePosition))
            {
                return;
            }

            if (current.type == EventType.MouseDown && current.button == 0)
            {
                ApplyTool(cell);
                current.Use();
            }
            else if (current.type == EventType.MouseDrag && current.button == 0 &&
                     (selectedTool == MapTool.Walkable || selectedTool == MapTool.Erase))
            {
                ApplyTool(cell);
                current.Use();
            }
        }

        private void ApplyTool(Vector2Int cell)
        {
            Undo.RecordObject(selectedAsset, "Edit Map Asset");

            switch (selectedTool)
            {
                case MapTool.Walkable:
                    selectedAsset.SetCell(cell, true, brushDepth);
                    break;
                case MapTool.Erase:
                    selectedAsset.RemoveCell(cell);
                    RemoveMarkerIfMatches(ref selectedAsset.PlayerSpawn, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.MerchantPosition, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.ReliquaryPosition, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.StartAltarPosition, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.UnlockAltarPosition, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.UndertakerPosition, cell);
                    selectedAsset.TemporaryWeaponSpawns.RemoveAll(position => position == cell);
                    break;
                case MapTool.PlayerSpawn:
                    selectedAsset.PlayerSpawn = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.Merchant:
                    selectedAsset.MerchantPosition = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.Reliquary:
                    selectedAsset.ReliquaryPosition = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.StartAltar:
                    selectedAsset.StartAltarPosition = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.UnlockAltar:
                    selectedAsset.UnlockAltarPosition = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.Undertaker:
                    selectedAsset.UndertakerPosition = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.WeaponSpawn:
                    EnsureWalkable(cell);
                    if (selectedAsset.TemporaryWeaponSpawns.Contains(cell))
                    {
                        selectedAsset.TemporaryWeaponSpawns.RemoveAll(position => position == cell);
                    }
                    else
                    {
                        selectedAsset.TemporaryWeaponSpawns.Add(cell);
                    }
                    break;
            }

            MarkDirty();
        }

        private void EnsureWalkable(Vector2Int cell)
        {
            selectedAsset.SetCell(cell, true, brushDepth);
        }

        private void FrameToCells()
        {
            if (selectedAsset.Cells.Count == 0)
            {
                return;
            }

            Vector2Int min = selectedAsset.Cells[0].Position;
            Vector2Int max = selectedAsset.Cells[0].Position;
            for (int i = 1; i < selectedAsset.Cells.Count; i++)
            {
                min = Vector2Int.Min(min, selectedAsset.Cells[i].Position);
                max = Vector2Int.Max(max, selectedAsset.Cells[i].Position);
            }

            Undo.RecordObject(selectedAsset, "Frame Map Bounds");
            selectedAsset.EditorMin = min - Vector2Int.one;
            selectedAsset.EditorMax = max + Vector2Int.one;
            MarkDirty();
        }

        private void CreateNewAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Map Asset", "NewMapAsset", "asset", "Choose a location for the map asset.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            MapAsset asset = CreateInstance<MapAsset>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            selectedAsset = asset;
            Selection.activeObject = asset;
        }

        private Rect GetCellRect(Vector2Int cell)
        {
            float x = (cell.x - selectedAsset.EditorMin.x) * CellSize;
            float y = (selectedAsset.EditorMax.y - cell.y) * CellSize;
            return new Rect(x, y, CellSize, CellSize);
        }

        private Color GetCellColor(Vector2Int cell)
        {
            if (!selectedAsset.TryGetCell(cell, out MapCellData cellData) || !cellData.Walkable)
            {
                return Color.black;
            }

            return cellData.Depth >= 2 ? DeepColor : WalkableColor;
        }

        private List<string> BuildValidationIssues(MapAsset asset)
        {
            List<string> issues = new List<string>();
            if (asset.Cells.Count == 0)
            {
                issues.Add("No walkable cells are defined.");
                return issues;
            }

            if (!IsWalkable(asset, asset.PlayerSpawn))
            {
                issues.Add("Player spawn is not on a walkable cell.");
            }

            AddReachabilityIssue(asset, asset.PlayerSpawn, asset.MerchantPosition, "Merchant", issues);
            AddReachabilityIssue(asset, asset.PlayerSpawn, asset.ReliquaryPosition, "Reliquary", issues);
            AddReachabilityIssue(asset, asset.PlayerSpawn, asset.StartAltarPosition, "Start altar", issues);
            AddReachabilityIssue(asset, asset.PlayerSpawn, asset.UnlockAltarPosition, "Unlock altar", issues);
            AddReachabilityIssue(asset, asset.PlayerSpawn, asset.UndertakerPosition, "Undertaker", issues);

            for (int i = 0; i < asset.TemporaryWeaponSpawns.Count; i++)
            {
                if (!IsWalkable(asset, asset.TemporaryWeaponSpawns[i]))
                {
                    issues.Add($"Weapon spawn {asset.TemporaryWeaponSpawns[i]} is not on a walkable cell.");
                }
            }

            return issues;
        }

        private static void AddReachabilityIssue(MapAsset asset, Vector2Int origin, Vector2Int target, string label, ICollection<string> issues)
        {
            if (target == Vector2Int.zero && label != "Start altar")
            {
                return;
            }

            if (!IsWalkable(asset, target))
            {
                issues.Add($"{label} is not on a walkable cell.");
                return;
            }

            if (!CanReach(asset, origin, target))
            {
                issues.Add($"{label} is not reachable from player spawn.");
            }
        }

        private static bool IsWalkable(MapAsset asset, Vector2Int cell)
        {
            return asset.TryGetCell(cell, out MapCellData cellData) && cellData.Walkable;
        }

        private static bool CanReach(MapAsset asset, Vector2Int origin, Vector2Int target)
        {
            if (origin == target)
            {
                return true;
            }

            if (!IsWalkable(asset, origin) || !IsWalkable(asset, target))
            {
                return false;
            }

            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int> { origin };
            frontier.Enqueue(origin);

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
            };

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2Int next = current + directions[i];
                    if (!visited.Add(next) || !IsWalkable(asset, next))
                    {
                        continue;
                    }

                    if (next == target)
                    {
                        return true;
                    }

                    frontier.Enqueue(next);
                }
            }

            return false;
        }

        private static void RemoveMarkerIfMatches(ref Vector2Int marker, Vector2Int cell)
        {
            if (marker == cell)
            {
                marker = Vector2Int.zero;
            }
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

        private static void DrawMarker(Rect cellRect, bool enabled, string label, Color color)
        {
            if (!enabled)
            {
                return;
            }

            Rect markerRect = new Rect(cellRect.x + 3f, cellRect.y + 3f, 14f, 14f);
            EditorGUI.DrawRect(markerRect, color);
            GUI.Label(markerRect, label, CenteredMiniLabel());
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(selectedAsset);
            Repaint();
        }

        private enum MapTool
        {
            Walkable,
            Erase,
            PlayerSpawn,
            Merchant,
            Reliquary,
            StartAltar,
            UnlockAltar,
            Undertaker,
            WeaponSpawn,
        }
    }
}
#endif

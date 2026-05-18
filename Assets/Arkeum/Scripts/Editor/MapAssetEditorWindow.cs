#if UNITY_EDITOR
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;
using UnityEditor;
using UnityEngine;

namespace Arkeum.Editor
{
    public sealed class MapAssetEditorWindow : EditorWindow
    {
        private const float CellSize = 28f;
        private static readonly Color GridColor = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color WalkableColor = new Color(0.23f, 0.2f, 0.2f);
        private static readonly Color WeaponColor = new Color(0.75f, 0.43f, 0.18f);
        private static readonly Color PlayerColor = new Color(0.91f, 0.86f, 0.78f);
        private static readonly Color FloorExitColor = new Color(0.76f, 0.65f, 0.17f);
        private static readonly Color DungeonEntranceColor = new Color(0.62f, 0.29f, 0.22f);
        private static readonly Color DoorColor = new Color(0.22f, 0.5f, 0.84f);
        private static readonly Color EnemyColor = new Color(0.64f, 0.15f, 0.16f);
        private static readonly Color WallColor = new Color(0.08f, 0.08f, 0.09f);

        private MapTool selectedTool = MapTool.Walkable;
        private MapAsset selectedAsset;
        private Vector2 scrollPosition;
        private DoorDirection selectedDoorDirection = DoorDirection.Right;
        private EnemyDefinition selectedEnemyDefinition;
        private WeaponDefinition selectedWeaponDefinition;

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
                if (selectedTool == MapTool.Door)
                {
                    selectedDoorDirection = (DoorDirection)EditorGUILayout.EnumPopup(selectedDoorDirection, EditorStyles.toolbarPopup, GUILayout.Width(84f));
                }
                else if (selectedTool == MapTool.EnemySpawn)
                {
                    selectedEnemyDefinition = (EnemyDefinition)EditorGUILayout.ObjectField(
                        selectedEnemyDefinition,
                        typeof(EnemyDefinition),
                        false,
                        GUILayout.Width(180f));
                }
                else if (selectedTool == MapTool.WeaponSpawn)
                {
                    selectedWeaponDefinition = (WeaponDefinition)EditorGUILayout.ObjectField(
                        selectedWeaponDefinition,
                        typeof(WeaponDefinition),
                        false,
                        GUILayout.Width(180f));
                }

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
                "Walkable/Erase edits terrain. Wall places blocking objects. Door stores explicit room exits for dungeon generation.",
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
            DrawMarker(cellRect, selectedAsset.PlayerSpawn == cell, "P", PlayerColor);
            DrawMarker(cellRect, IsMarkerEnabled(selectedAsset.FloorExitPosition) && selectedAsset.FloorExitPosition == cell, "X", FloorExitColor);
            DrawMarker(cellRect, IsMarkerEnabled(selectedAsset.DungeonEntrancePosition) && selectedAsset.DungeonEntrancePosition == cell, "E", DungeonEntranceColor);

            if (TryGetWeaponSpawn(cell, out WeaponSpawnDefinition weaponSpawn))
            {
                string displayName = weaponSpawn.Weapon != null ? weaponSpawn.Weapon.DisplayName : string.Empty;
                string label = !string.IsNullOrEmpty(displayName)
                    ? displayName.Substring(0, 1).ToUpperInvariant()
                    : "W";
                DrawMarker(cellRect, true, label, WeaponColor);
            }

            if (TryGetEnemySpawn(cell, out EnemySpawnDefinition enemySpawn))
            {
                string displayName = enemySpawn.EnemyDefinition != null ? enemySpawn.EnemyDefinition.DisplayName : string.Empty;
                string label = !string.IsNullOrEmpty(displayName)
                    ? displayName.Substring(0, 1).ToUpperInvariant()
                    : "E";
                DrawMarker(cellRect, true, label, EnemyColor);
            }

            DrawDoorMarkers(cell, cellRect);
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
                     (selectedTool == MapTool.Walkable ||
                      selectedTool == MapTool.Erase ||
                      selectedTool == MapTool.Wall ||
                      selectedTool == MapTool.WallErase))
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
                    selectedAsset.SetCell(cell, true);
                    break;
                case MapTool.Wall:
                    selectedAsset.SetWall(cell, true);
                    RemoveMarkerIfMatches(ref selectedAsset.PlayerSpawn, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.FloorExitPosition, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.DungeonEntrancePosition, cell);
                    RemoveWeaponSpawnsAt(cell);
                    selectedAsset.EnemySpawns.RemoveAll(spawn => spawn != null && spawn.Position == cell);
                    selectedAsset.RemoveDoorsAt(cell);
                    break;
                case MapTool.WallErase:
                    selectedAsset.SetWall(cell, false);
                    break;
                case MapTool.Erase:
                    selectedAsset.RemoveCell(cell);
                    RemoveMarkerIfMatches(ref selectedAsset.PlayerSpawn, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.FloorExitPosition, cell);
                    RemoveMarkerIfMatches(ref selectedAsset.DungeonEntrancePosition, cell);
                    RemoveWeaponSpawnsAt(cell);
                    selectedAsset.EnemySpawns.RemoveAll(spawn => spawn != null && spawn.Position == cell);
                    selectedAsset.RemoveDoorsAt(cell);
                    break;
                case MapTool.Door:
                    EnsureWalkable(cell);
                    selectedAsset.SetDoor(cell, selectedDoorDirection);
                    break;
                case MapTool.DoorErase:
                    selectedAsset.RemoveDoorsAt(cell);
                    break;
                case MapTool.PlayerSpawn:
                    selectedAsset.PlayerSpawn = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.FloorExit:
                    selectedAsset.FloorExitPosition = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.DungeonEntrance:
                    selectedAsset.DungeonEntrancePosition = cell;
                    EnsureWalkable(cell);
                    break;
                case MapTool.WeaponSpawn:
                    EnsureWalkable(cell);
                    if (TryGetWeaponSpawn(cell, out _))
                    {
                        RemoveWeaponSpawnsAt(cell);
                    }
                    else if (selectedWeaponDefinition != null)
                    {
                        selectedAsset.WeaponSpawns.Add(new WeaponSpawnDefinition
                        {
                            Weapon = selectedWeaponDefinition,
                            Position = cell,
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[MapAssetEditor] Select a WeaponDefinition before placing a weapon spawn.");
                    }
                    break;
                case MapTool.EnemySpawn:
                    EnsureWalkable(cell);
                    if (TryGetEnemySpawn(cell, out _))
                    {
                        selectedAsset.EnemySpawns.RemoveAll(spawn => spawn != null && spawn.Position == cell);
                    }
                    else if (selectedEnemyDefinition != null)
                    {
                        selectedAsset.EnemySpawns.Add(new EnemySpawnDefinition
                        {
                            EnemyDefinition = selectedEnemyDefinition,
                            Position = cell,
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[MapAssetEditor] Select an EnemyDefinition before placing an enemy spawn.");
                    }
                    break;
            }

            MarkDirty();
        }

        private void EnsureWalkable(Vector2Int cell)
        {
            selectedAsset.SetCell(cell, true);
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

            return cellData.HasWall ? WallColor : WalkableColor;
        }

        private List<string> BuildValidationIssues(MapAsset asset)
        {
            List<string> issues = new List<string>();
            if (asset.Cells.Count == 0)
            {
                issues.Add("No walkable cells are defined.");
                return issues;
            }

            if (!IsNavigable(asset, asset.PlayerSpawn))
            {
                issues.Add("Player spawn is not on a navigable cell.");
            }

            AddReachabilityIssue(asset, asset.PlayerSpawn, asset.FloorExitPosition, "Floor exit", issues);
            AddReachabilityIssue(asset, asset.PlayerSpawn, asset.DungeonEntrancePosition, "Dungeon entrance", issues);

            for (int i = 0; i < asset.WeaponSpawns.Count; i++)
            {
                WeaponSpawnDefinition weaponSpawn = asset.WeaponSpawns[i];
                if (weaponSpawn == null)
                {
                    issues.Add($"Weapon spawn {i} is empty.");
                    continue;
                }

                if (weaponSpawn.Weapon == null)
                {
                    issues.Add($"Weapon spawn {weaponSpawn.Position} has no WeaponDefinition.");
                }

                if (!IsNavigable(asset, weaponSpawn.Position))
                {
                    issues.Add($"Weapon spawn {weaponSpawn.Position} is not on a navigable cell.");
                }
            }

            for (int i = 0; i < asset.EnemySpawns.Count; i++)
            {
                EnemySpawnDefinition enemySpawn = asset.EnemySpawns[i];
                if (enemySpawn == null)
                {
                    issues.Add($"Enemy spawn {i} is empty.");
                    continue;
                }

                if (enemySpawn.EnemyDefinition == null)
                {
                    issues.Add($"Enemy spawn {enemySpawn.Position} has no EnemyDefinition.");
                }

                if (!IsNavigable(asset, enemySpawn.Position))
                {
                    issues.Add($"Enemy spawn {enemySpawn.Position} is not on a navigable cell.");
                }
            }

            if (asset.Doors.Count == 0)
            {
                issues.Add("No doors are defined. Dungeon room placement uses explicit MapAsset doors.");
            }

            for (int i = 0; i < asset.Doors.Count; i++)
            {
                MapDoorData door = asset.Doors[i];
                if (door == null)
                {
                    issues.Add($"Door {i} is empty.");
                    continue;
                }

                if (!IsNavigable(asset, door.Position))
                {
                    issues.Add($"Door {door.Direction} at {door.Position} is not on a navigable cell.");
                }
            }

            return issues;
        }

        private static void AddReachabilityIssue(MapAsset asset, Vector2Int origin, Vector2Int target, string label, ICollection<string> issues)
        {
            if (!IsMarkerEnabled(target))
            {
                return;
            }

            if (!IsNavigable(asset, target))
            {
                issues.Add($"{label} is not on a navigable cell.");
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

        private static bool IsNavigable(MapAsset asset, Vector2Int cell)
        {
            return asset.TryGetCell(cell, out MapCellData cellData) && cellData.Walkable && !cellData.HasWall;
        }

        private static bool CanReach(MapAsset asset, Vector2Int origin, Vector2Int target)
        {
            if (origin == target)
            {
                return true;
            }

            if (!IsNavigable(asset, origin) || !IsNavigable(asset, target))
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
                    if (!visited.Add(next) || !IsNavigable(asset, next))
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

        private static bool IsMarkerEnabled(Vector2Int position)
        {
            return position != Vector2Int.zero;
        }

        private bool TryGetEnemySpawn(Vector2Int cell, out EnemySpawnDefinition enemySpawn)
        {
            for (int i = 0; i < selectedAsset.EnemySpawns.Count; i++)
            {
                enemySpawn = selectedAsset.EnemySpawns[i];
                if (enemySpawn != null && enemySpawn.Position == cell)
                {
                    return true;
                }
            }

            enemySpawn = null;
            return false;
        }

        private bool TryGetWeaponSpawn(Vector2Int cell, out WeaponSpawnDefinition weaponSpawn)
        {
            for (int i = 0; i < selectedAsset.WeaponSpawns.Count; i++)
            {
                weaponSpawn = selectedAsset.WeaponSpawns[i];
                if (weaponSpawn != null && weaponSpawn.Position == cell)
                {
                    return true;
                }
            }

            weaponSpawn = null;
            return false;
        }

        private void RemoveWeaponSpawnsAt(Vector2Int cell)
        {
            selectedAsset.WeaponSpawns.RemoveAll(spawn => spawn != null && spawn.Position == cell);
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

        private void DrawDoorMarkers(Vector2Int cell, Rect cellRect)
        {
            for (int i = 0; i < selectedAsset.Doors.Count; i++)
            {
                MapDoorData door = selectedAsset.Doors[i];
                if (door == null || door.Position != cell)
                {
                    continue;
                }

                Rect markerRect = GetDoorMarkerRect(cellRect, door.Direction);
                EditorGUI.DrawRect(markerRect, DoorColor);
                GUI.Label(markerRect, GetDoorLabel(door.Direction), CenteredMiniLabel());
            }
        }

        private static Rect GetDoorMarkerRect(Rect cellRect, DoorDirection direction)
        {
            const float size = 12f;
            switch (direction)
            {
                case DoorDirection.Up:
                    return new Rect(cellRect.center.x - size * 0.5f, cellRect.y + 2f, size, size);
                case DoorDirection.Down:
                    return new Rect(cellRect.center.x - size * 0.5f, cellRect.yMax - size - 2f, size, size);
                case DoorDirection.Left:
                    return new Rect(cellRect.x + 2f, cellRect.center.y - size * 0.5f, size, size);
                default:
                    return new Rect(cellRect.xMax - size - 2f, cellRect.center.y - size * 0.5f, size, size);
            }
        }

        private static string GetDoorLabel(DoorDirection direction)
        {
            switch (direction)
            {
                case DoorDirection.Up:
                    return "U";
                case DoorDirection.Down:
                    return "D";
                case DoorDirection.Left:
                    return "L";
                default:
                    return "R";
            }
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(selectedAsset);
            Repaint();
        }

        private enum MapTool
        {
            Walkable,
            Wall,
            WallErase,
            Erase,
            Door,
            DoorErase,
            PlayerSpawn,
            FloorExit,
            DungeonEntrance,
            WeaponSpawn,
            EnemySpawn,
        }
    }
}
#endif

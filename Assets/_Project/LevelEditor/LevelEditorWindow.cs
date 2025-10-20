using System.Collections.Generic;
using _Project.Data.GameData;
using _Project.Data.ScriptableObjects.Data;
using _Project.Scripts.Gameplay.Grid;
using _Project.Scripts.Gameplay.Human;
using UnityEditor;
using UnityEngine;

namespace _Project.LevelEditor
{
    public class LevelEditorWindow : EditorWindow
    {
        private const int CellButtonSize = 26;
        private const int CellButtonPadding = 2;

        private GameData _gameData;
        private Vector2 _levelsScroll;
        private List<bool> _levelFoldouts = new();

        [MenuItem("Tools/Level Editor")]
        public static void Open()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(700, 420);
            window.Show();
        }

        private void OnGUI()
        {
            Header();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if(Resources.Load<GameData>("GameConfig") != null && _gameData == null)
                {
                    _gameData = Resources.Load<GameData>("GameConfig");
                }
                else
                {
                    _gameData = (GameData)EditorGUILayout.ObjectField("GameConfig Asset", _gameData, typeof(GameData), false);
                }

                if (_gameData == null)
                {
                    if (GUILayout.Button("Create New GameConfig Asset"))
                    {
                        CreateNewConfigAsset();
                    }
                    return;
                }

                EditorGUI.BeginChangeCheck();

                // Global config
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
                _gameData.refillSeconds = EditorGUILayout.FloatField(new GUIContent("Life Refill (sn)"), _gameData.refillSeconds);

                EditorGUILayout.Space(8);
                DrawLevelsSection();

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_gameData);
                }
            }

            BottomBar();
        }

        private void Header()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("LEVEL EDITOR", new GUIStyle(EditorStyles.largeLabel)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            });
            EditorGUILayout.Space(4);
        }

        private void BottomBar()
        {
            EditorGUILayout.Space(8);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Save", GUILayout.Width(120), GUILayout.Height(24)))
                {
                    SaveAsset();
                }
            }
            EditorGUILayout.Space(6);
        }

        private void CreateNewConfigAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Game Config", "GameConfig", "asset", "");
            if (string.IsNullOrEmpty(path)) return;

            var asset = CreateInstance<GameData>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _gameData = asset;
            _levelFoldouts.Clear();
        }

        private void SaveAsset()
        {
            if (_gameData == null)
            {
                return;
            }
            
            EditorUtility.SetDirty(_gameData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            ShowNotification(new GUIContent("Saved!"));
        }

        private void DrawLevelsSection()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ Add New Level", GUILayout.Width(120)))
                {
                    _gameData.levels.Add(new LevelConfig(){ levelName = $"Level {_gameData.levels.Count + 1}" });
                    _levelFoldouts.Add(true);
                    MarkDirty();
                }
            }

            EditorGUILayout.Space(4);
            _levelsScroll = EditorGUILayout.BeginScrollView(_levelsScroll);

            for (var i = 0; i < _gameData.levels.Count; i++)
            {
                if (_levelFoldouts.Count != _gameData.levels.Count)
                {
                    while (_levelFoldouts.Count < _gameData.levels.Count) _levelFoldouts.Add(true);
                    while (_levelFoldouts.Count > _gameData.levels.Count) _levelFoldouts.RemoveAt(_levelFoldouts.Count - 1);
                }

                var level = _gameData.levels[i];

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _levelFoldouts[i] = EditorGUILayout.Foldout(_levelFoldouts[i], $"{i+1}. {level.levelName}", true);
                        GUILayout.FlexibleSpace();
                        level.enabled = EditorGUILayout.ToggleLeft("Enabled", level.enabled, GUILayout.Width(100));

                        if (GUILayout.Button("Sil", GUILayout.Width(60)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Level", $"Delete {level.levelName} level?", "Yes", "No"))
                            {
                                _gameData.levels.RemoveAt(i);
                                MarkDirty();
                                break;
                            }
                        }
                    }

                    if (_levelFoldouts[i])
                    {
                        EditorGUI.indentLevel++;

                        level.levelName = EditorGUILayout.TextField("Level Name", level.levelName);
                        var newWidth  = Mathf.Max(1, EditorGUILayout.IntField("Grid Width (X)", level.width));
                        var newHeight = Mathf.Max(1, EditorGUILayout.IntField("Grid Height (Y)", level.height));

                        if (newWidth != level.width || newHeight != level.height || level.cells == null || level.cells.Count == 0)
                        {
                            level.width = newWidth;
                            level.height = newHeight;
                            EnsureGridSize(level, GridType.Fixed);
                        }

                        EditorGUILayout.Space(4);
                        DrawGridMatrix(level);

                        EditorGUILayout.Space(6);
                        DrawGoldSection(level);
                        
                        EditorGUILayout.Space(6);
                        DrawHolderSection(level);
                        
                        EditorGUILayout.Space(6);
                        DrawBusSection(level);

                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawGoldSection(LevelConfig level)
        {
            EditorGUILayout.LabelField("Gold Amount", EditorStyles.boldLabel);
            level.goldCount = Mathf.Max(0, EditorGUILayout.IntField("Gold Amount", level.goldCount));
            EditorGUILayout.HelpBox("Defines how much gold you get when you complete this level.", MessageType.Info);
        }
        
        private void DrawHolderSection(LevelConfig level)
        {
            EditorGUILayout.LabelField("Holder Settings", EditorStyles.boldLabel);
            level.holderCount = Mathf.Max(0, EditorGUILayout.IntField("Holder Count", level.holderCount));
            EditorGUILayout.HelpBox("Defines how many holder slots exist in this level. (Only 5 fits now)", MessageType.Info);
        }
        
        private void DrawBusSection(LevelConfig level)
        {
            EditorGUILayout.LabelField("Bus Settings", EditorStyles.boldLabel);
            var newBusCount = Mathf.Max(0, EditorGUILayout.IntField("Bus Count", level.busCount));
            if (newBusCount != level.busCount)
            {
                level.busCount = newBusCount;
                EnsureBusList(level);
                MarkDirty();
            }

            // 1 human type per bus
            for (var b = 0; b < level.busCount; b++)
            {
                if (b >= level.busHumanTypes.Count)
                {
                    level.busHumanTypes.Add(default);
                }
                level.busHumanTypes[b] = (HumanType)EditorGUILayout.EnumPopup($"Bus {b+1} Human", level.busHumanTypes[b]);
            }

            EditorGUILayout.HelpBox("Assigns one HumanType per bus. The list auto syncs with the Bus Count.", MessageType.Info);
        }

        private void DrawGridMatrix(LevelConfig level)
        {
            EditorGUILayout.LabelField("Grid Cells", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Left Click: Toggle GridType (Fixed/Empty)\nShift + Left Click: Cycle HumanType", MessageType.None);
        
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLegendBox("F", Color.gray, "Fixed");
                DrawLegendBox("W", Color.white, "Empty");
                DrawLegendBox("H", new Color(0.25f, 0.7f, 0.25f), "Occupied (color varies by HumanType)");
                GUILayout.Space(12);
                EditorGUILayout.LabelField("Tags: F = Fixed, W = Empty H = Occupied", GUILayout.Height(18));
            }
        
            // Draw from bottom to top (so y=0 appears at the top visually)
            for (var y = 0; y < level.height; y++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var x = 0; x < level.width; x++)
                    {
                        var cell = GetCell(level, x, y);
                        if (cell == null)
                        {
                            cell = new GridCellConfig { x = x, y = y, gridType = GridType.Fixed, humanType = default };
                            level.cells.Add(cell);
                            MarkDirty();
                        }

                        var (label, bg) = GetCellVisual(cell);

                        var prevColor = GUI.backgroundColor;
                        GUI.backgroundColor = bg;

                        // left click
                        if (GUILayout.Button(label, GUILayout.Width(CellButtonSize), GUILayout.Height(CellButtonSize)))
                        {
                            var e = Event.current;
                            if (e.shift)
                            {
                                // HumanType toggle
                                cell.gridType = GridType.Occupied;
                                CycleHumanType(cell);
                                e.Use();
                                Repaint();
                            }
                            else
                            {
                                // GridType toggle
                                cell.gridType = (cell.gridType == GridType.Fixed) ? GridType.Empty : GridType.Fixed;
                                MarkDirty();
                            }
                        }

                        GUI.backgroundColor = prevColor;
                        GUILayout.Space(CellButtonPadding);
                    }
                }
            }
        }

        private (GUIContent label, Color bg) GetCellVisual(GridCellConfig cell)
        {
            var text = "";
            Color bg;
            
            switch (cell.gridType)
            {
                case GridType.Fixed:
                    text = "F";
                    bg = Color.gray;
                    break;
                case GridType.Empty:
                    text = "W";
                    bg = Color.white;
                    break;
                case GridType.Occupied:
                    text = "H";
                    bg = GetHumanColor(cell.humanType);
                    break;  
                default:
                    text = "?";
                    bg = Color.magenta;
                    break;
            }
            
            var tooltip = $"{cell.x},{cell.y} - {cell.gridType} - {cell.humanType}";
            return (new GUIContent(text, tooltip: tooltip), bg);
        }

        private void CycleHumanType(GridCellConfig cell)
        {
            // Blue -> Green -> Red -> ...
            var values = (HumanType[])System.Enum.GetValues(typeof(HumanType));
            var humanTypeIndex = System.Array.IndexOf(values, cell.humanType);
            humanTypeIndex = (humanTypeIndex + 1) % values.Length;
            cell.humanType = values[humanTypeIndex];
           
            MarkDirty();
        }

        private void DrawLegendBox(string text, Color bg, string tooltip)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = bg;
            GUILayout.Button(new GUIContent(text, tooltip), GUILayout.Width(24), GUILayout.Height(18));
            GUI.backgroundColor = prev;
            GUILayout.Space(4);
            EditorGUILayout.LabelField(tooltip, GUILayout.Width(70));
            GUILayout.Space(8);
        }

        private void EnsureGridSize(LevelConfig level, GridType defaultType)
        {
            if (level.cells == null)
            {
                level.cells = new List<GridCellConfig>();
            }
            
            var newList = new List<GridCellConfig>(level.width * level.height);

            for (var x = 0; x < level.width; x++)
            {
                for (var y = 0; y < level.height; y++)
                {
                    var existing = GetCell(level, x, y);
                    if (existing != null)
                    {
                        newList.Add(existing);
                    }
                    else
                    {
                        newList.Add(new GridCellConfig { x = x, y = y, gridType = defaultType, humanType = default });
                    }
                }
            }

            level.cells = newList;
            MarkDirty();
        }

        private GridCellConfig GetCell(LevelConfig level, int x, int y)
        {
            if (level.cells == null)
            {
                return null;
            }
            
            for (var i = 0; i < level.cells.Count; i++)
            {
                var c = level.cells[i];
                if (c.x == x && c.y == y)
                {
                    return c;
                }
            }
            return null;
        }

        // Randomized color cache per HumanType for Occupied cells
        private static readonly Dictionary<HumanType, Color> _humanTypeColors = new();

        private static Color GetHumanColor(HumanType humanType)
        {
            switch (humanType)
            {
                case HumanType.Blue:
                    return Color.blue;
                case HumanType.Green:
                    return Color.green;
                case HumanType.Red:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        private void EnsureBusList(LevelConfig level)
        {
            if (level.busHumanTypes == null)
            {
                level.busHumanTypes = new List<HumanType>();
            }
            
            while (level.busHumanTypes.Count < level.busCount) level.busHumanTypes.Add(default);
            while (level.busHumanTypes.Count > level.busCount) level.busHumanTypes.RemoveAt(level.busHumanTypes.Count - 1);
        }

        private void MarkDirty()
        {
            if (_gameData != null)
            {
                EditorUtility.SetDirty(_gameData);
            }
        }
    }
}
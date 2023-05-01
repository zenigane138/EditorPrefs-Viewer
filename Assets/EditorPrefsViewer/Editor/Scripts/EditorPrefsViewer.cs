#if UNITY_EDITOR_WIN
using Microsoft.Win32;
# endif
using System;
using UnityEditor;
using UnityEngine;

public class EditorPrefsViewer : EditorWindow
{
    private static readonly string WindowTitle = "EditorPrefs Viewer";
    private static readonly Vector2 WindowMinSize = new Vector2(350, 200);

    // Unity 2017 以降でも EditorPrefs の保存に使っているレジストリパスは Unity Editor 5.x
    private readonly string _registryPath = @"Software\Unity Technologies\Unity Editor 5.x";

    private Vector2 _scroll;
    private string _filterText = "";
    private bool _isLoaded = false;
    private string[] _keys = null;
    private string[] _values = null;

    private bool _caseSensitive = false;
    private bool _wordWrap = false;
    private bool _split = false;

#if UNITY_2017_1_OR_NEWER
    [MenuItem("Window/OkaneGames/", priority = Int32.MaxValue)]
#endif
    [MenuItem("Window/OkaneGames/EditorPrefs Viewer")]
    [MenuItem("OkaneGames/EditorPrefs Viewer")]
    private static void CreateWindow()
    {
        var window = CreateInstance<EditorPrefsViewer>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = WindowMinSize;
        window.Show();
    }

    private void LoadEditorPrefsData()
    {
#if UNITY_EDITOR_WIN
        using (var key = Registry.CurrentUser.OpenSubKey(_registryPath))
        {
            if (key == null) return;

            _keys = key.GetValueNames();
            _values = new string[_keys.Length];

            for (int i = 0; i < _keys.Length; i++)
            {
                _values[i] = key.GetValue(_keys[i]).ToString();
            }
        }
#endif

        _isLoaded = true;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Reload EditorPrefs"))
            {
                _isLoaded = false;
                LoadEditorPrefsData();
            }
        }
        EditorGUILayout.EndHorizontal();

        // フィルタ系
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter", GUILayout.Width(35));
            _filterText = GUILayout.TextField(_filterText);
            if (GUILayout.Button("Clear", GUILayout.Width(45)))
            {
                _filterText = "";
            }
            DrawToggleButton(ref _caseSensitive, new GUIContent("case-sensitive"));
        }
        EditorGUILayout.EndHorizontal();

        // UI調整系
        EditorGUILayout.BeginHorizontal();
        {
            DrawToggleButton(ref _wordWrap, new GUIContent("Word Wrap"));
            DrawToggleButton(ref _split, new GUIContent("Split"));
        }
        EditorGUILayout.EndHorizontal();

        // データ本体
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        {
            if (_isLoaded && _keys != null && _keys.Length > 0)
            {
                EditorGUILayout.BeginVertical();
                {
                    float splitWidth = position.width / 2;
                    GUIStyle style = new GUIStyle(EditorStyles.textField);
                    if (_wordWrap)
                    {
                        style.wordWrap = true;
                    }

                    for (int i = 0; i < _keys.Length; i++)
                    {
                        if (IsDisplayKeyValue(i))
                        {
                            if (_split)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.TextArea(_keys[i], style, GUILayout.Width(splitWidth));
                                    EditorGUILayout.TextArea(_values[i], style);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.TextArea("" + _keys[i] + ": " + _values[i] + "", style);
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                EditorGUILayout.LabelField("EditorPrefs data is not loaded.");
#if UNITY_EDITOR_OSX
                EditorGUILayout.LabelField("Current version does not work on Mac.");
#endif
            }
        }
        EditorGUILayout.EndScrollView();

        DrawZeniganeLink();
    }

    private bool IsDisplayKeyValue(int index)
    {
        if(_caseSensitive)
        {
            var result = 
                string.IsNullOrEmpty(_filterText) ||
                _keys[index].Contains(_filterText) ||
                _values[index].Contains(_filterText);
            return result;
        }
        else
        {
            var result = 
                string.IsNullOrEmpty(_filterText) ||
                _keys[index].ToLower().Contains(_filterText.ToLower()) ||
                _values[index].ToLower().Contains(_filterText.ToLower());
            return result;
        }
    }

    private void DrawToggleButton(ref bool settingFlag, GUIContent guiContent, bool enabledHorizontal = false)
    {
        if (enabledHorizontal) EditorGUILayout.BeginHorizontal();
        if (settingFlag != EditorGUILayout.Toggle(settingFlag, GUILayout.Width(15)) ||
            GUILayout.Button(guiContent, new GUIStyle(GUI.skin.label), GUILayout.ExpandWidth(false)))
        {
            settingFlag ^= true;
        }
        if (enabledHorizontal) EditorGUILayout.EndHorizontal();
    }

    private void DrawZeniganeLink()
    {
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("(C) 2023 OkaneGames / zenigane");
            if (GUILayout.Button(new GUIContent("GitHub", ""), GUILayout.Width(50)))
            {
                Application.OpenURL("https://github.com/zenigane138");
            }
            if (GUILayout.Button(new GUIContent("Blog", ""), GUILayout.Width(35)))
            {
                Application.OpenURL("https://zenigane138.hateblo.jp/?from=editorprefsviewer");
            }
            if (GUILayout.Button(new GUIContent("Twitter", ""), GUILayout.Width(55)))
            {
                Application.OpenURL("https://twitter.com/zenigane138");
            }
        }
        GUILayout.EndHorizontal();
    }
}
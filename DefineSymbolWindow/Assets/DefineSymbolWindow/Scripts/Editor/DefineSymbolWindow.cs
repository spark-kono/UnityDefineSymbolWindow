/* DefineSymbolWindow.cs      
 * Create : 2021/02/14  Masahiro Kono
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class DefineSymbolWindow : EditorWindow
{
    private static readonly string SAVE_DATA_DIRECTORY = "/DefineSymbolWindow/SaveData/";
    private static readonly string SAVE_DATA_PATH = SAVE_DATA_DIRECTORY + "SaveData.json";
    private static SaveData saveData = new SaveData();
    private Vector2 buildTargetGroupDataScrollPosition = Vector2.zero;
    private Vector2 defineSymbolDataScrollPosition = Vector2.zero;

    [Serializable]
    private class SaveData
    {
        public List<BuildTargetGroupData> buildTargetGroupDataList = new List<BuildTargetGroupData>();
        public List<DefineSymbolData> defineSymbolDataList = new List<DefineSymbolData>();

        public void DefaultSetting()
        {
            buildTargetGroupDataList.Clear();

            foreach (BuildTargetGroup value in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if(value == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var addData = new BuildTargetGroupData(value, false);
                bool isAdd = true;

                foreach(var buildTargetGroupData in buildTargetGroupDataList)
                {
                    if(buildTargetGroupData.buildTargetGroup == addData.buildTargetGroup)
                    {
                        isAdd = false;
                        break;
                    }
                }

                if(!isAdd)
                {
                    continue;
                }

                buildTargetGroupDataList.Add(addData);
            }

            defineSymbolDataList.Clear();
            AddDefineSymbol();
        }

        public void AddDefineSymbol()
        {
            defineSymbolDataList.Add(new DefineSymbolData());
        }

        public void RemoveDefineSymbol(DefineSymbolData _target)
        {
            defineSymbolDataList.Remove(_target);
        }

        [Serializable]
        public class DefineSymbolData
        {
            public bool flag = default;
            public string defineSymbol = "";

            [NonSerialized]
            public bool isRemove = default;
        }

        [Serializable]
        public class BuildTargetGroupData
        {
            public bool flag = default;
            public BuildTargetGroup buildTargetGroup = default;

            public BuildTargetGroupData(BuildTargetGroup _buildTargetGroup, bool _flag)
            {
                buildTargetGroup = _buildTargetGroup;
                flag = _flag;
            }
        }
    }

    [MenuItem("Tools/DefineSymbolWindow/Open", false, 0)]
    public static void Open()
    {
        var window = EditorWindow.GetWindow<DefineSymbolWindow>("DefineSymbol");
        window.maxSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        Load();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            BuildTargetGroupOnGUI();
            DefineSymbolOnGUI();
        }
        GUILayout.EndHorizontal();
    }

    private void BuildTargetGroupOnGUI()
    {
        GUILayout.BeginVertical(EditorStyles.toolbar, GUILayout.Width(220));
        {
            GUILayout.Label("BuildTargetGroupSetting");

            buildTargetGroupDataScrollPosition = GUILayout.BeginScrollView(buildTargetGroupDataScrollPosition, GUILayout.Height(500));
            {
                GUILayout.BeginVertical("box");
                {
                    foreach (var data in saveData.buildTargetGroupDataList)
                    {
                        data.flag = EditorGUILayout.ToggleLeft(data.buildTargetGroup.ToString(), data.flag);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
    }

    private void DefineSymbolOnGUI()
    {
        GUILayout.BeginVertical(EditorStyles.toolbar);
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("DefineSymbolSetting", GUILayout.Width(130));

                if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    saveData.AddDefineSymbol();
                }

                if (GUILayout.Button("Set", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    Setting();
                }

                if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog("DefineSymbolSetting", "Do you want to reset and return to the default settings?", "Yes", "No"))
                    {
                        DefalutSetting();
                    }
                }
            }
            GUILayout.EndHorizontal();

            defineSymbolDataScrollPosition = GUILayout.BeginScrollView(defineSymbolDataScrollPosition, GUILayout.Height(500));
            {
                GUILayout.BeginVertical("box");
                {
                    foreach (var data in saveData.defineSymbolDataList)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            data.flag = GUILayout.Toggle(data.flag, "", GUILayout.Width(20));
                            data.defineSymbol = GUILayout.TextField(data.defineSymbol);
                            data.isRemove = GUILayout.Button("Remove", GUILayout.Width(70));
                        }
                        GUILayout.EndHorizontal();
                    }

                    for (int i = saveData.defineSymbolDataList.Count - 1; i >= 0; i--)
                    {
                        var data = saveData.defineSymbolDataList[i];

                        if (data.isRemove)
                        {
                            saveData.RemoveDefineSymbol(data);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
    }

    private void Setting()
    {
        if (!EditorUtility.DisplayDialog("DefineSymbolSetting", "Do you want to reflect the define symbol settings?", "Yes", "No"))
        {
            return;
        }

        Save();

        foreach (var buildTargetGroupData in saveData.buildTargetGroupDataList)
        {
            if (!buildTargetGroupData.flag)
            {
                continue;
            }

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroupData.buildTargetGroup).Split(';').ToList();
            symbols.Clear();

            foreach (var defineSymbolData in saveData.defineSymbolDataList)
            {
                if (!defineSymbolData.flag)
                {
                    continue;
                }

                if(string.IsNullOrEmpty(defineSymbolData.defineSymbol))
                {
                    continue;
                }

                symbols.Add(defineSymbolData.defineSymbol);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroupData.buildTargetGroup, string.Join(";", symbols));
        }
    }

    

    private void Save()
    {
        var directoryPath = Application.dataPath + SAVE_DATA_DIRECTORY;

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var json = JsonUtility.ToJson(saveData);
        var savePath = Application.dataPath + SAVE_DATA_PATH;
        StreamWriter writer = new StreamWriter(savePath, false);
        writer.WriteLine(json);
        writer.Flush();
        writer.Close();
        AssetDatabase.Refresh();
    }

    private void Load()
    {
        if (File.Exists(Application.dataPath + SAVE_DATA_PATH))
        {
            FileStream stream = File.Open(Application.dataPath + SAVE_DATA_PATH, FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            var json = reader.ReadToEnd();
            reader.Close();
            stream.Close();

            var loadData = JsonUtility.FromJson<SaveData>(json);

            // デフォルト設定にしてBuildTargetGroupにプラットフォーム追加更新が無いか確認・シンボルはそのまま読み込んで代入
            saveData.DefaultSetting();
            saveData.defineSymbolDataList = loadData.defineSymbolDataList;

            // プラットフォームは増えることがあっても基本減ることは無いと思うのでデフォルト設定読み込んだデータにロードデータのフラグを順番に設定
            for (int i = 0; i < loadData.buildTargetGroupDataList.Count; i++)
            {
                saveData.buildTargetGroupDataList[i].flag = loadData.buildTargetGroupDataList[i].flag;
            }

            return;
        }

        DefalutSetting();
    }

    private void DefalutSetting()
    {
        saveData.DefaultSetting();
        Save();
    }
}

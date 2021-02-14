/* DefineSymbolWindow.cs
 * SparkCreative.inc      
 * Update : 2021/02/14  Masahiro Kono
 * 
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DefineSymbolWindow : EditorWindow
{
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
    
    private void Start()
    {
        
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

                if (GUILayout.Button("Setting", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    Setting();
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
        if(EditorUtility.DisplayDialog("DefineSymbolSetting", "Do you want to reflect the define symbol settings?", "Yes", "No"))
        {
            Save();
        }
    }

    private void Save()
    {

    }

    private void Load()
    {
        saveData.DefaultSetting();
    }
}

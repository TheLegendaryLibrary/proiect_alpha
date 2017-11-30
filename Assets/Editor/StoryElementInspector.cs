using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StoryElement))]
public class StoryElementInspector : Editor
{
    private static GUIContent insertContent = new GUIContent("+  添加状态", "添加新的状态到状态列表"), deleteContent = new GUIContent("删除", "删除当前状态"), pointContent = GUIContent.none;
    private static GUILayoutOption buttonWidth = GUILayout.MaxWidth(40f);

    //ClickElement element;
    private SerializedObject element;
    private SerializedProperty dolist;

    private bool[] showActionList;

    void OnEnable()
    {
        //获取当前编辑自定义Inspector的对象
        element = new SerializedObject(target);
        dolist = element.FindProperty("DoList");
        StoryElement _target = (StoryElement)target;
        showActionList = new bool[dolist.arraySize];
    }

    //自定义检视面板
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("播放故事脚本的工具。", MessageType.Info);
        element.Update();

        //状态列表标题
        GUILayout.Label("状态列表: ", EditorStyles.largeLabel);

        //状态列表内容
        for (int i = 0; i < dolist.arraySize; i++)
        {
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            SerializedProperty statedo = dolist.GetArrayElementAtIndex(i);
            GUILayout.Label("  状态ID");
            EditorGUILayout.PropertyField(statedo.FindPropertyRelative("StateID"), pointContent);
            //EditorGUILayout.PropertyField(action.FindPropertyRelative("ActionList"), pointContent);
            GUILayout.Label("     执行命令");
            EditorGUILayout.PropertyField(statedo.FindPropertyRelative("NextDo"), pointContent);
            if (statedo.FindPropertyRelative("NextDo").enumValueIndex == 2)
            {
                EditorGUILayout.PropertyField(element.FindProperty("jumpnum"), pointContent, GUILayout.Width(50));
                EditorGUILayout.Space();
            }
            else if (statedo.FindPropertyRelative("NextDo").enumValueIndex == 3)
            {
                EditorGUILayout.PropertyField(element.FindProperty("sceneName"), pointContent, GUILayout.Width(50));
                EditorGUILayout.Space();
            }

            if (GUILayout.Button(deleteContent, EditorStyles.miniButton, buttonWidth))
            {
                dolist.DeleteArrayElementAtIndex(i);
                SaveProperties();
                ChangeshowActionList();
                return;
            }
            EditorGUILayout.EndHorizontal();

            //状态列表内的动画列表
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("  故事脚本", GUILayout.MaxWidth(65f));
            EditorGUILayout.PropertyField(statedo.FindPropertyRelative("Story"), pointContent);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button(insertContent))
        {
            dolist.InsertArrayElementAtIndex(dolist.arraySize);
            SaveProperties();
            ChangeshowActionList();
            return;
        }
        SaveProperties();
    }

    void SaveProperties()
    {
        element.ApplyModifiedProperties();
    }

    void ChangeshowActionList()
    {
        bool[] oldlist = showActionList;
        showActionList = new bool[dolist.arraySize];
        for (int i = 0; i < Mathf.Min(dolist.arraySize, oldlist.Length); i++)
        {
            showActionList[i] = oldlist[i];
        }
    }
}

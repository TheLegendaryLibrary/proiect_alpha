using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleStoryElement))]
public class SimpleStoryElementInspector : Editor
{
    private static GUIContent insertContent = new GUIContent("+  添加状态", "添加新的状态到状态列表"), deleteContent = new GUIContent("删除", "删除当前状态"), insertAniContent = new GUIContent("+", "添加新的字幕到字幕列表"), deleteAniContent = new GUIContent("-", "删除字幕"), pointContent = GUIContent.none;
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
        SimpleStoryElement _target = (SimpleStoryElement)target;
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

            //状态列表内的字幕列表
            SerializedProperty actinlist = statedo.FindPropertyRelative("talks");
            showActionList[i] = EditorGUILayout.Foldout(showActionList[i], "  字幕列表: " + actinlist.arraySize + "个", true);
            if (showActionList[i])
            {
                for (int j = 0; j < actinlist.arraySize; j++)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    SerializedProperty talks = actinlist.GetArrayElementAtIndex(j);
                    EditorGUILayout.PropertyField(talks.FindPropertyRelative("character"), pointContent, GUILayout.Width(120f));
                    talks.FindPropertyRelative("talkstring").stringValue = EditorGUILayout.TextArea(talks.FindPropertyRelative("talkstring").stringValue);
                    if (GUILayout.Button(deleteAniContent, EditorStyles.miniButton, GUILayout.Width(20f)))
                    {
                        actinlist.DeleteArrayElementAtIndex(j);
                        SaveProperties();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                //添加动画按钮
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("");
                if (GUILayout.Button(insertAniContent, EditorStyles.miniButton, GUILayout.MinWidth(80f), GUILayout.MaxWidth(200f)))
                {
                    actinlist.InsertArrayElementAtIndex(actinlist.arraySize);
                    SaveProperties();
                    return;
                }
                GUILayout.Label("");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndFadeGroup();
            }
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

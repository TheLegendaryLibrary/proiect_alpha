using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemElement))]
public class ItemElementInspector : Editor
{
    ItemElement element;

    void OnEnable()
    {
        //获取当前编辑自定义Inspector的对象
        element = (ItemElement)target;
    }

    //自定义检视面板
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("道具物品使用的工具，道具物品点击后会放入道具栏。\n需要配合EventArea使用。", MessageType.Info);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("道具描述:");
        element.Name = EditorGUILayout.TextField(element.Name);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("道具描述:");
        element.Des = EditorGUILayout.TextArea(element.Des, GUILayout.MinHeight(100));
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("获得后显示描述面板：", GUILayout.Width(120));
        element.showInfo = EditorGUILayout.Toggle(element.showInfo);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}

using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DragElement))]
public class DragElementInspector : Editor {

    DragElement element;
    void OnEnable()
    {
        //获取当前编辑自定义Inspector的对象
        element = (DragElement)target;
    }

    void OnGUI()
    {
    }

    //自定义检视面板
    public override void OnInspectorGUI()
    {
        //设置整个界面是以垂直方向来布局
        EditorGUILayout.HelpBox("可拖拽物件使用的工具，拖动的范围是物件的父节点区域。", MessageType.Info);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("拖拽时物品居中：", GUILayout.Width(120));
        element.ToCenter = EditorGUILayout.Toggle(element.ToCenter);
        EditorGUILayout.EndHorizontal();
        if (element.ToCenter)
            EditorGUILayout.HelpBox("拖拽时物品的中心点会吸附到触摸位置", MessageType.None);
        EditorGUILayout.EndVertical();
    }
}

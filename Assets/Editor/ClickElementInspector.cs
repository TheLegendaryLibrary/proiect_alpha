using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ClickElement))]
public class ClickElementInspector : Editor
{
    private static GUIContent insertContent = new GUIContent("+  添加状态", "添加新的状态到状态列表"), deleteContent = new GUIContent("删除", "删除当前状态"), insertAniContent = new GUIContent("+", "添加新的动画到动画列表"), deleteAniContent = new GUIContent("-", "删除动画"), pointContent = GUIContent.none;
    private static GUILayoutOption buttonWidth = GUILayout.MaxWidth(40f);

    //ClickElement element;
    private SerializedObject element;
    private SerializedProperty dolist;

    private Transform transform;
    private bool[] showActionList;

    void OnEnable()
    {
        //获取当前编辑自定义Inspector的对象
        element = new SerializedObject(target);
        dolist = element.FindProperty("DoList");
        ClickElement _target = (ClickElement)target;
        transform = _target.transform;
        showActionList = new bool[dolist.arraySize];
    }

    //自定义检视面板
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("需要点击的物件使用的工具，物件点击效果依照状态列表中的配置来触发。\n状态ID为0的话，则是默认状态，即如果没有找到对应的状态ID就会触发这个状态中的动作。", MessageType.Info);
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
            EditorGUI.indentLevel = 1;
            SerializedProperty actinlist = statedo.FindPropertyRelative("ActionList");
            SerializedProperty animatorlist = statedo.FindPropertyRelative("AnimatorList");
            SerializedProperty animationActionlist = statedo.FindPropertyRelative("AnimationAction");
            showActionList[i] = EditorGUILayout.Foldout(showActionList[i], "  动画列表: " + actinlist.arraySize + "个", true);
            if (showActionList[i])
            {
                for (int j = 0; j < actinlist.arraySize; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    //动画控制器
                    SerializedProperty animator = animatorlist.GetArrayElementAtIndex(j);
                    EditorGUILayout.PropertyField(animator, pointContent);

                    //动画clip
                    SerializedProperty clip = actinlist.GetArrayElementAtIndex(j);
                    EditorGUILayout.PropertyField(clip, pointContent);

                    //播放方式
                    SerializedProperty animationtype = animationActionlist.GetArrayElementAtIndex(j);
                    EditorGUILayout.PropertyField(animationtype, pointContent, GUILayout.Width(70f));

                    if (GUILayout.Button(deleteAniContent, EditorStyles.miniButton, GUILayout.Width(20f)))
                    {
                        actinlist.DeleteArrayElementAtIndex(j);
                        animatorlist.DeleteArrayElementAtIndex(j);
                        animationActionlist.DeleteArrayElementAtIndex(j);
                        SaveProperties();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                    //动画归纳成组的判断
                    if (animationtype.enumValueIndex == 1)
                    {
                        EditorGUILayout.HelpBox("-----上面的动画播放完后在播放之后的动画------", MessageType.None);
                    }
                    //检查是否出错
                    //if (clip.objectReferenceValue != null)
                    //{
                    //    string rootname = clip.objectReferenceValue.name.Split('_')[0];
                    //    GetAnimator(rootname);
                    //}
                }
                //添加动画按钮
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("");
                if (GUILayout.Button(insertAniContent, EditorStyles.miniButton, GUILayout.MinWidth(80f), GUILayout.MaxWidth(200f)))
                {
                    actinlist.InsertArrayElementAtIndex(actinlist.arraySize);
                    animatorlist.InsertArrayElementAtIndex(animatorlist.arraySize);
                    animationActionlist.InsertArrayElementAtIndex(animationActionlist.arraySize);
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

    public bool GetAnimator(string rootname)
    {
        bool iserror = false;
        Transform t = transform.parent.Find(rootname);
        if (t == null && transform.parent.name.CompareTo(rootname) == 0)
            t = transform.parent;
        if (t == null)
        {
            EditorGUILayout.HelpBox("找不到 " + rootname + " ,请检查动画名称是否和物件对应!", MessageType.Error);
            return true;
        }

        Animator ani = t.GetComponent<Animator>();
        if (ani == null)
        {
            EditorGUILayout.HelpBox("找不到 " + rootname + " 的动画管理器,请检查动画名称是否和物件对应!", MessageType.Error);
            return true;
        }
        return iserror;
    }
}

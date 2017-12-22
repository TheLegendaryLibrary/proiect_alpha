using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementBase : MonoBehaviour {

    LevelManager levelManager;
    public int jumpnum = 0;
    public string sceneName = "";
    [System.Serializable]
    public enum StateAction
    {
        None = 0,
        Next,
        Jump,
        ChangeScene,
        Complete,
        Fail
    }
    public enum AnimationType
    {
        Same = 0,
        Next,
    }

    // Use this for initialization
    virtual public void Awake()
    {
        IntiElement();
    }

    //初始化当前状态
    public void IntiElement()
    {
        //获取关卡管理器
        levelManager = transform.Find("/Main Camera").GetComponent<LevelManager>();
        if (levelManager == null)
        {
            Debug.Log("获取不到关卡管理器! 物件名称为：" + transform);
            return;
        }

        Debug.LogFormat("物件<color=blue> {0} </color>初始化完成！", transform.name);
    }

    //确定是否下一步
    public void CheckAction(StateAction action,int jump)
    {
        if (action == StateAction.Next)
        {
            levelManager.AddNowState();
        }
        else if (action == StateAction.Jump)
        {
            levelManager.JumpState(jump);
        }
        else if (action == StateAction.ChangeScene)
        {
            levelManager.ChangeScene(sceneName);
        }
        else if (action == StateAction.Complete)
        {
            levelManager.CompleteLevel();
        }
        else if (action == StateAction.Fail)
        {
            levelManager.FailLevel();
        }
    }

    public LevelManager GetLevelManager()
    {
        return levelManager;
    }

    public Animator GetAnimator(string rootname)
    {
        Transform t = transform.parent.Find(rootname);
        if (t == null && transform.parent.name.CompareTo(rootname) == 0)
            t = transform.parent;
        if (t == null)
        {
            Debug.LogFormat(transform.name + " 找不到<color=red> {0} </color>,请检查动画名称是否和物件对应!", rootname);
            return null;
        }

        Animator ani = t.GetComponent<Animator>();
        if (ani == null)
        {
            Debug.LogFormat(transform.name + " 找不到<color=red> {0} </color>的动画管理器,请检查动画名称是否和物件对应!", rootname);
            return null;
        }
        return ani;
    }
}

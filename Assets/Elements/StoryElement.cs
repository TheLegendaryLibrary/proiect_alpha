using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryElement : ElementBase
{
    ChatSystemManager manager;

    [System.Serializable]
    public struct StateDo
    {
        public int StateID;
        public string Story;
        public StateAction NextDo;
    }

    public StateDo[] DoList;

    // Use this for initialization
    override public void Awake()
    {
        IntiElement();
        manager = transform.Find("/Main Camera").GetComponent<ChatSystemManager>();
        AddToLevelManger();
    }

    // Use this for initialization
    void Start () {

    }

    void AddToLevelManger()
    {
        GetLevelManager().AddStoryElement(this);
    }

    public bool CheckDoList()
    {
        int stateID = GetLevelManager().GetNowState();
        StateDo _do = GetStateDo(stateID);

        //如果找不到动作，则什么都不做
        if (_do.StateID == -1) return false;

        //播放故事
        GetLevelManager().SetLevelState(LevelManager.LevelStateType.PlayStory);
        if (manager != null)
        {
            manager.StartStory(_do.Story, () =>
            {
                GetLevelManager().SetLevelState(LevelManager.LevelStateType.Common);
                CheckAction(_do.NextDo, jumpnum);
            });
            return true;
        }
        else
        {
            Debug.Log("找不到故事管理器,请检查！");
            return false;
        }
    }


    //查找动作列表中对应的动作
    private StateDo GetStateDo(int stateID)
    {
        StateDo common = new StateDo();
        common.StateID = -1;

        //查找对应ID的动作
        foreach (StateDo _s in DoList)
        {
            if (_s.StateID == 0)
                common = _s;
            if (_s.StateID == stateID)
                return _s;
        }
        //如果找到默认动作，则返回默认动作，否则返回空
        if (common.StateID == 0)
            return common;
        else
        {
            //Debug.LogFormat("<color=red> {0} </color>的ActionList中找不到<color=red> {1}阶段 </color>的动作！", transform.name, stateID);
            return common;
        }
    }

}

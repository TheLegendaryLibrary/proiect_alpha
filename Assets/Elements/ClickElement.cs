using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickElement : ElementBase
{
    [System.Serializable]
    public struct StateDo
    {
        public int StateID;
        public AnimationClip[] ActionList;
        public StateAction NextDo;
    }

    public StateDo[] DoList;
    Animator ani;

    // Use this for initialization
    override public void Awake()
    {
        IntiElement();
        EventTriggerListener.Get(transform).onClick = CheckOnClick;
    }

    //执行点击动作
    private void CheckOnClick(GameObject go)
    {
        //如果正在播放动画则不响应点击
        if (!GetLevelManager().isCommonState()) return;

        int stateID = GetLevelManager().GetNowState();
        StateDo _do = GetStateDo(stateID);

        //如果找不到动作，则什么都不做
        if (_do.StateID == -1) return;

        //执行动作
        float maxTime = -1;
        //执行这个State下的多个动作
        foreach (AnimationClip action in _do.ActionList)
        {
            if (action != null)
                maxTime = Mathf.Max(maxTime, action.length);

            PlayAnimation(action);
        }
        WaitForCheckAction(maxTime, _do);
    }

    private void PlayAnimation(AnimationClip clip)
    {
        //执行动作
        if (clip != null)
        {
            string rootname = clip.name.Split('_')[0];
            ani = GetAnimator(rootname);
            if (ani == null) return;

            ani.Play(clip.name, 0, 0);
            GetLevelManager().SetLevelState(LevelManager.LevelStateType.PlayAnimation);
        }
    }

    private void WaitForCheckAction(float time, StateDo _do)
    {
        if (time > 0)
        {
            //等待动画播放完执行下一步判断
            TimeTool.SetWaitTime(time, gameObject, () =>
            {
                GetLevelManager().SetLevelState(LevelManager.LevelStateType.Common);
                CheckAction(_do.NextDo,jumpnum);
            });
        }
        else
        {
            CheckAction(_do.NextDo, jumpnum);
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

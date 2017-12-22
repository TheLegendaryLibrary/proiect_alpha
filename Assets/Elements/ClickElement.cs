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
        public Animator[] AnimatorList;
        public AnimationType[] AnimationAction;
        public StateAction NextDo;
    }

    public StateDo[] DoList;
    Animator ani;
    int animation_index = 0;

    // Use this for initialization
    override public void Awake()
    {
        IntiElement();
        EventTriggerListener.Get(transform).onClick = CheckOnClick;
        animation_index = 0;
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
        animation_index = 0;
        //执行这个State下的多个动作
        maxTime = CheckAnimationList(_do);
        WaitForCheckAction(maxTime, _do);
    }

    private void DoAnimationNext(StateDo _do)
    {
        //执行动作
        float maxTime = -1;
        //执行这个State下的多个动作
        maxTime = CheckAnimationList(_do);
        WaitForCheckAction(maxTime, _do);
    }

    float CheckAnimationList(StateDo _do)
    {
        float maxTime = -1;
        for (int i = animation_index; i < _do.ActionList.Length; i++)
        {
            AnimationClip action = _do.ActionList[i];
            if (action != null)
                maxTime = Mathf.Max(maxTime, action.length);

            PlayAnimation(action, GetAnimator(animation_index, _do));
            animation_index++;

            //如果是顺序执行则先等待动画播放完
            if (_do.AnimationAction[animation_index - 1] == AnimationType.Next)
                break;
        }
        return maxTime;
    }

    private void PlayAnimation(AnimationClip clip,Animator animator)
    {
        //执行动作
        if (clip != null)
        {
            string rootname = clip.name.Split('_')[0];
            ani = animator;
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
                if (animation_index >= _do.ActionList.Length)
                {
                    GetLevelManager().SetLevelState(LevelManager.LevelStateType.Common);
                    CheckAction(_do.NextDo, jumpnum);
                }
                else
                    DoAnimationNext(_do);

            });
        }
        else
        {
            if (animation_index >= _do.ActionList.Length)
            {
                CheckAction(_do.NextDo, jumpnum);
            }
            else
                DoAnimationNext(_do);
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

    public Animator GetAnimator(int index,StateDo _do)
    {
        if (_do.AnimatorList.Length > index)
        {
            return _do.AnimatorList[index];
        }
        else
            return null;
    }
}

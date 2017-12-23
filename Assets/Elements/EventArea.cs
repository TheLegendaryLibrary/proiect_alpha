using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class EventArea : ElementBase, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum CheckType
    {
        OnDrop,
        OnEnter,
        None
    }

    public bool ishideitem = false;
    private RectTransform Area;
    private CanvasGroup group;
    float prealpha = 1f;
    public CheckType checkType;

    [System.Serializable]
    public struct StateDo
    {
        public int StateID;
        public string DragName;
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
        IntiArea();
        animation_index = 0;
    }
	
    //初始化当前状态
    void IntiArea()
    {
        Area = transform as RectTransform;

        //创建画布组
        group = transform.GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnDrop(PointerEventData data)
    {
        if (checkType != CheckType.OnDrop)
            return;

        group.alpha = prealpha;
        MoveToCenter(data);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (GetDropObject(data) == null)
            return;

        //变色效果
        if (checkType == CheckType.OnDrop)
        {
            prealpha = group.alpha;
            if (group.alpha > 0.5f)
                group.alpha = group.alpha / 1.5f;
            else
                group.alpha = group.alpha * 1.5f;
        }

        if (checkType != CheckType.OnEnter)
            return;

        MoveToCenter(data);
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (GetDropObject(data) == null)
            return;

        //变色效果
        if (checkType == CheckType.OnDrop)
        {
            group.alpha = prealpha;
        }
    }

    private GameObject GetDropObject(PointerEventData data)
    {
        var originalObj = data.pointerDrag;
        if (originalObj == null)
            return null;

        var dragMe = originalObj.GetComponent<DragElement>();
        var dragMeItem = originalObj.GetComponent<ItemDragEffect>();
        if (dragMe == null && dragMeItem == null)
            return null;

        var srcImage = originalObj.GetComponent<Image>();
        if (srcImage == null)
            return null;

        var canvasgroup = originalObj.GetComponent<CanvasGroup>();
        if (canvasgroup == null)
            return null;

        return originalObj;
    }

    void MoveToCenter(PointerEventData data)
    {
        GameObject dropObject = GetDropObject(data);
        if (dropObject == null)
            return;
        //如果正在播放动画则不响应点击
        if (!GetLevelManager().isCommonState()) return;

        ArrayList stateList = GetStateDo(GetLevelManager().GetNowState());
        ArrayList donelist = new ArrayList();
        //遍历执行所有符合条件的动作
        foreach (StateDo _do in stateList)
        {
            //跳过已经执行过的物体
            if (donelist.Contains(_do.DragName)) break;
            donelist.Add(_do.DragName);

            if (_do.DragName.CompareTo(dropObject.name) == 0)
            {
                if (ishideitem)
                    dropObject.GetComponent<Image>().color = Color.clear;

                float maxTime = -1;
                animation_index = 0;
                //执行这个State下的多个动作
                maxTime = CheckAnimationList(_do);

                data.pointerDrag = null;
                dropObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
                MoveToCenter effect = GetMoveToCenter(dropObject);
                effect.SetPos(Area.position);
                WaitForCheckAction(maxTime, _do);
            }
        }
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

    private void PlayAnimation(AnimationClip clip, Animator animator)
    {
        //执行动作
        if (clip != null)
        {
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
    private ArrayList GetStateDo(int stateID)
    {
        ArrayList commonList = new ArrayList();
        ArrayList stateList = new ArrayList();

        //查找对应ID的动作
        foreach (StateDo _s in DoList)
        {
            if (_s.StateID == 0)
                commonList.Add(_s);
            if (_s.StateID == stateID)
                stateList.Add(_s);
        }
        if (stateList.Count > 0)
            return stateList;
        else
            return commonList;
    }

    MoveToCenter GetMoveToCenter(GameObject dropObject)
    {
        MoveToCenter moveeffect = dropObject.gameObject.GetComponent<MoveToCenter>();
        if (moveeffect == null)
            moveeffect = dropObject.gameObject.AddComponent<MoveToCenter>();
        return moveeffect;
    }

    public Animator GetAnimator(int index, StateDo _do)
    {
        if (_do.AnimatorList.Length > index)
        {
            return _do.AnimatorList[index];
        }
        else
            return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    int NowState = 1;   //当前状态，不同的状态对应不同的操作
    int level = 0;      //关卡id

    //游戏状态
    public enum LevelStateType
    {
        PlayAnimation,
        PlayStory,
        Common
    }
    LevelStateType LevelState = LevelStateType.Common;

    ArrayList StoryElements = new ArrayList();    //故事管理器
    ArrayList SimpleStoryElements = new ArrayList();    //故事管理器

    public GameObject CompleteBoard;
    public GameObject FailBoard;
    public string NextLevel;
    private RectTransform BagUI;

    private void Awake()
    {
        IntiManager();
    }

    // Use this for initialization
    void Start () {
        CheckElementsList();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //初始化当前状态
    void IntiManager()
    {
        NowState = 1;
        BagUI = transform.Find("/GameCanvas/UIlayer/bagUI") as RectTransform;

        Debug.LogFormat("初始化关卡<color=green> {0} </color>成功！", level);  
    }

    public int GetNowState()
    {
        return NowState;
    }

    public int AddNowState()
    {
        Debug.Log("当前状态: " + (NowState + 1));
        NowState++;
        CheckElementsList();
        return NowState;
    }

    public int JumpState(int jump)
    {
        Debug.Log("当前状态: " + jump);
        NowState = jump;
        CheckElementsList();
        return NowState;
    }

    public void ChangeScene(string scene)
    {
        GameManager.NextLevel(gameObject, scene);
    }

    public void CompleteLevel()
    {
        GameObject cp = Instantiate(CompleteBoard);
        cp.transform.SetParent(transform.Find("/GameCanvas/UIlayer"), false);

        EventTriggerListener.Get(cp.transform.Find("RetryButton")).onClick = GameManager.ReloadNowLevel;
        EventTriggerListener.Get(cp.transform.Find("NextButton")).onClick = GameManager.NextLevel;   
    }

    public void FailLevel()
    {
        GameObject fp = Instantiate(FailBoard);
        fp.transform.SetParent(transform.Find("/GameCanvas/UIlayer"), false);
        EventTriggerListener.Get(fp.transform.Find("RetryButton")).onClick = GameManager.ReloadNowLevel;
    }

    public bool isCommonState()
    {
        if (LevelState == LevelStateType.Common)
            return true;
        else
            return false;
    }

    public void AddItemInBagUI(GameObject item)
    {
        ItemElement itemscript = item.GetComponent<ItemElement>();
        if (itemscript != null) Destroy(itemscript);

        int childcount = BagUI.childCount;
        item.transform.SetParent(BagUI);
        RectTransform trans = item.transform as RectTransform;
        float ratio = 120 / trans.rect.width;
        trans.sizeDelta = new Vector2(trans.rect.size.x * ratio, trans.rect.size.y * ratio);

        MoveToCenter moveeffect = item.AddComponent<MoveToCenter>();
        moveeffect.SetLocalPos(GetBagPos(childcount));
        moveeffect.SetSpeed(3f);
        moveeffect.SetCallback(() =>
        {
            trans.localPosition = GetBagPos(childcount);
            item.AddComponent<ItemDragEffect>();
        });
    }

    Vector2 GetBagPos(int count)
    {
        count = count + 1;
        Vector2 pading = new Vector2(15, 45);
        Vector2 pos = new Vector2((120 + pading.x) * count - 60, 0);
        return pos;
    }

    //添加故事管理器
    public void AddStoryElement(StoryElement element)
    {
        if (!StoryElements.Contains(element))
        {
            StoryElements.Add(element);
            Debug.Log("已添加故事管理器。 " + element.transform);
        }
        else
        {
            Debug.Log("已经包含了这个故事管理器！ " + element.transform);
        }
    }

    //添加故事管理器
    public void AddSompleStoryElement(SimpleStoryElement element)
    {
        if (!SimpleStoryElements.Contains(element))
        {
            SimpleStoryElements.Add(element);
            Debug.Log("已添加故事管理器。 " + element.transform);
        }
        else
        {
            Debug.Log("已经包含了这个故事管理器！ " + element.transform);
        }
    }

    //检查故事管理器
    bool CheckStoryElement()
    {
        bool ishit = false;
        foreach (StoryElement element in StoryElements)
        {
            ishit = element.CheckDoList();

            if (ishit)
                break;
        }
        return ishit;
    }

    //检查字幕管理器
    bool CheckSimpleStoryElement()
    {
        bool ishit = false;
        foreach (SimpleStoryElement element in SimpleStoryElements)
        {
            ishit = element.CheckDoList();

            if (ishit)
                break;
        }
        return ishit;
    }

    //检查故事和字幕的方法
    void CheckElementsList()
    {
        if (!CheckStoryElement())
            CheckSimpleStoryElement();
        else
        {
            Debug.Log("触发故事工具，因此跳过检查字幕工具！");
        }

    }

    public LevelStateType GetLevelState()
    {
        return LevelState;
    }

    public void SetLevelState(LevelStateType _levelstate)
    {
        LevelState = _levelstate;
    }
}

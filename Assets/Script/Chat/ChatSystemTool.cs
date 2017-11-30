using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChatSystemTool : MonoBehaviour {

    public struct ChatConfig
    {
        public string Languege;
        public float speed;
        public bool showname;
    }
    public struct ChatActionBox
    {
        public ArrayList ActionList;
        public Dictionary<string, ChatAction.StoryCharacter> CharacterList;
        public string[] BG;
        public string[] BGM;
        public int NowIndex;
        public bool UseBG1;
        public object info;
    }
    public struct ResourcesBox
    {
        public Dictionary<string, Sprite[]> windowsSprites;
        public Dictionary<string, Dictionary<string, Sprite>> characterSprites;
        public Dictionary<string, Sprite> bgSprites;
        public Dictionary<string, AudioClip> bgms;
        public Dictionary<string, AudioClip> voices;
    }
    struct ChatBoard
    {
        public RectTransform WordsBacklayer;
        public RectTransform WordsOutLayer;

        public RectTransform NameBackLayer;
        public RectTransform NameOutLayer;

        public RectTransform ClickHintLayer;

        public Text WordsText;
        public Text NameText;
    }
    ChatConfig NowConfig;
    ChatActionBox NowStroyActionBox;
    ResourcesBox NowResourcesBox;

    RectTransform StoryBoardLayer;

    RectTransform BGLayer1;
    RectTransform BGLayer2;
    RectTransform CharacterLayer;
    //RectTransform MaskLayer;
    RectTransform ClickLayer;
    RectTransform SelectionLayer;
    GameObject SelectionPrefab;
    Button btn_Skip;
    ChatBoard TextBoardLayer;
    Dictionary<string, RectTransform> CharacterRects;
    AudioSource AudioManager;
    AudioSource SpeakerAudioManager;

    ArrayList StoryList;
    string storyName = "";
    string lastWords = "";  //用于保存上一个动作时说话的台词，在点击时在lastword中增加当前语句，来达到点击快速完成当前对话的功能。啊，这个方法我知道有点坑!
    System.Action CallBack;

    //对话状态
    bool IsSelection = false;
    ArrayList FlagList;

    void Awake()
    {
        
    }

    // Use this for initialization
    void Start()
    {

    }

	// Update is called once per frame
	void Update () {
        //Debug.Log("chatmanager update!!!!");

        //检测鼠标点击事件
        if (Input.GetKey(KeyCode.Escape))
        {
            SkipStory(gameObject);
        }

    }

    //添加故事列表
    public void PushStoryList(ArrayList list)
    {
        StoryList = new ArrayList();
        StoryList = list;
    }

    //获取故事列表
    public ArrayList GetStoryList()
    {
        return StoryList;
    }

    //读取故事配置
    public void LoadChatStory(string storyname)
    {
        storyName = storyname;
        Debug.Log("Start: " + storyname);

        //初始化
        NowConfig = new ChatConfig();
        NowStroyActionBox = new ChatActionBox();
        CharacterRects = new Dictionary<string, RectTransform>();

        NowConfig = Loading.GetInstance().GetstoryResource().NowConfig;
        NowStroyActionBox = Loading.GetInstance().GetstoryResource().NowStroyActionBox;
        NowResourcesBox = Loading.GetInstance().GetstoryResource().NowResourcesBox;
        Loading.GetInstance().CloseLoading();

        NowStroyActionBox.UseBG1 = true;

        CreateChatLayer();
        DoingAction(NowStroyActionBox.NowIndex);
    }


    //创建故事面板
    void CreateChatLayer()
    {
        //创建面板
        GameObject StoryLayerObj = Resources.Load<GameObject>("Prefab/Chat/StoryLayer");
        StoryLayerObj = Instantiate(StoryLayerObj);
        StoryLayerObj.transform.SetParent(transform.Find("/GameCanvas").transform, false);

        //获取组件
        GetLayerComponent(StoryLayerObj);
        //初始化故事面板
        InstChatLayer();
    }

    //获取组件方法
    void GetLayerComponent(GameObject StoryLayerObj)
    {
        StoryBoardLayer = StoryLayerObj.GetComponent<RectTransform>();
        //MaskLayer = StoryBoardLayer.Find("Mask").GetComponent<RectTransform>();
        ClickLayer = StoryBoardLayer.Find("clickLayer").GetComponent<RectTransform>();
        SelectionLayer = StoryBoardLayer.Find("SelectionLayer").GetComponent<RectTransform>();
        SelectionPrefab= Resources.Load<GameObject>("Prefab/Chat/Selection");
        btn_Skip = StoryBoardLayer.Find("skip").GetComponent<Button>();
        BGLayer1 = StoryBoardLayer.Find("BG1").GetComponent<RectTransform>();
        BGLayer2 = StoryBoardLayer.Find("BG2").GetComponent<RectTransform>();
        CharacterLayer = StoryBoardLayer.Find("Character").GetComponent<RectTransform>();
        TextBoardLayer.WordsBacklayer = StoryBoardLayer.Find("TextBoard").GetComponent<RectTransform>();
        TextBoardLayer.WordsOutLayer = StoryBoardLayer.Find("TextBoard/OutBoard").GetComponent<RectTransform>();
        TextBoardLayer.WordsText = StoryBoardLayer.Find("TextBoard/TextMask/Text").GetComponent<Text>();

        TextBoardLayer.NameBackLayer = StoryBoardLayer.Find("TextBoard/NameBoard").GetComponent<RectTransform>();
        TextBoardLayer.NameOutLayer = StoryBoardLayer.Find("TextBoard/NameBoard/OutBoard").GetComponent<RectTransform>();
        TextBoardLayer.NameText = StoryBoardLayer.Find("TextBoard/NameBoard/Text").GetComponent<Text>();

        TextBoardLayer.ClickHintLayer = StoryBoardLayer.Find("ClickHint").GetComponent<RectTransform>();

        SpeakerAudioManager = StoryBoardLayer.GetComponent<AudioSource>();

        EventTriggerListener.Get(ClickLayer.gameObject).onClick = ClickToDoing;
        EventTriggerListener.Get(btn_Skip.gameObject).onClick = SkipStory;
    }

    //初始化故事面板
    void InstChatLayer()
    {
        //调整大小/位置
        StoryBoardLayer.sizeDelta = new Vector2(0, 0);
        StoryBoardLayer.localPosition = new Vector3(0, 0, 0);
        TextBoardLayer.WordsBacklayer.localScale = new Vector3(1, 0, 1);

        TextBoardLayer.NameText.text = "";
        TextBoardLayer.WordsText.text = "";

        IsSelection = false;
        FlagList = new ArrayList();

        //BGLayer1.GetComponent<Image>().color = Color.clear;
        BGLayer2.GetComponent<Image>().color = Color.clear;

        for (int i = 0; i < CharacterLayer.childCount; i++)
        {
            Destroy(CharacterLayer.GetChild(i).gameObject);
        }

        if (AudioManager == null)
            AudioManager = transform.GetComponent<AudioSource>();

        if (NowConfig.showname == false)
            TextBoardLayer.NameBackLayer.gameObject.SetActive(false);
    }

    //关闭故事面板
    void EndChatLayer()
    {

        if (StoryList.Count > 0)
        {
            StoryList.RemoveAt(0);
        }
        if (StoryList.Count > 0)
        {
            string ct = (string)StoryList[0];
            ChangeStory(ct);
            return;
        }

        LeanTween.scaleY(TextBoardLayer.WordsBacklayer.gameObject, 0, 0.25f).setOnComplete(() =>
            {
                LeanTween.alphaCanvas(CharacterLayer.GetComponent<CanvasGroup>(), 0, 1f);
                LeanTween.alpha(GetBGLayer(), 0, 1f).setOnComplete(() =>
                {
                    Destroy(StoryBoardLayer.gameObject);
                    Destroy(this.gameObject);

                    //执行回调
                    CallBack();
                });
            });
    }

    //切换故事
    void ChangeStory(string story)
    {
        Loading.GetInstance().LoadingStoryScene(story, () =>
        {
            Destroy(StoryBoardLayer.gameObject);
            LoadChatStory(story);
        });
    }

    void DoingAction(int index)
    {
        //如果完成所有动作
        if (index >= NowStroyActionBox.ActionList.Count)
        {
            EndChatLayer();
            return;
        }

        ChatAction.StoryAction action = (ChatAction.StoryAction)NowStroyActionBox.ActionList[index];
        ChatAction.StoryAction preaction = new ChatAction.StoryAction();
        RectTransform character;
        if(index>0)
            preaction = (ChatAction.StoryAction)NowStroyActionBox.ActionList[index - 1];

        switch (action.Command)
        {
            case "setbg":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                RectTransform lastBG = GetBGLayer();
                character = GetNowBGLayer();
                Image Bg = character.GetComponent<Image>();
                Bg.sprite = GetBGSprit(action.CharacterID);
                Bg.SetNativeSize();
                character.SetAsFirstSibling();

                if (Bg.color == Color.clear)
                    Bg.color = new Color(1, 1, 1, 0);
                else
                    Bg.color = Color.white;

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.alpha(character, 1, float.Parse(action.Parameter[0]));
                    LeanTween.alpha(lastBG, 0, float.Parse(action.Parameter[0])).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            //WaitingForClick();
                            return;
                        }
                        if (character.name == "BG1")
                            NowStroyActionBox.UseBG1 = true;
                        else
                            NowStroyActionBox.UseBG1 = false;

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                    });
                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.alpha(character, 1, float.Parse(action.Parameter[0]));
                    LeanTween.alpha(lastBG, 0, float.Parse(action.Parameter[0]));
                    SetActionState(ChatAction.NOWSTATE.DONE, index);
                    if (character.name == "BG1")
                        NowStroyActionBox.UseBG1 = true;
                    else
                        NowStroyActionBox.UseBG1 = false;

                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.alpha(character, 1, float.Parse(action.Parameter[0]));
                    LeanTween.alpha(lastBG, 0, float.Parse(action.Parameter[0])).setOnComplete(() =>
                    {
                        if (character.name == "BG1")
                            NowStroyActionBox.UseBG1 = true;
                        else
                            NowStroyActionBox.UseBG1 = false;

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    });
                }
                break;
            case "stop":
                SetActionState(ChatAction.NOWSTATE.DOING, index);

                //获取id对象
                if (action.CharacterID == "WINDOW")
                    character = TextBoardLayer.WordsBacklayer;
                else if (action.CharacterID == "BG")
                    character = GetBGLayer();
                else
                    character = GetCharacterRectTransform(action.CharacterID);

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.cancel(character.gameObject);
                    //如果是最后一个动作，则停止自动
                    if (index >= NowStroyActionBox.ActionList.Count)
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        //WaitingForClick();
                        return;
                    }
                    SetActionState(ChatAction.NOWSTATE.DONE, index);
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.cancel(character.gameObject);
                    SetActionState(ChatAction.NOWSTATE.DONE, index);
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.cancel(character.gameObject);
                    SetActionState(ChatAction.NOWSTATE.DONE, index);
                }
                break;
            case "show":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetCharacterRectTransform(action.CharacterID);
                character.localPosition = new Vector2(CharacterLayer.rect.width * float.Parse(action.Parameter[0]), CharacterLayer.rect.height * float.Parse(action.Parameter[1]));
                character.transform.SetSiblingIndex(int.Parse(action.Parameter[3]));
                SetCharacterSprite(action.CharacterID, action.Parameter[5]);

                if (action.Parameter[4] == "left")
                {
                    if (character.localScale.x > 0)
                        character.localScale = new Vector3(character.localScale.x * -1, character.localScale.y, 1);
                }
                else
                {
                    if (character.localScale.x > 0)
                        character.localScale = new Vector3(character.localScale.x * -1, character.localScale.y, 1);
                }

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.alpha(character, 1, float.Parse(action.Parameter[2])).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            //WaitingForClick();
                            return;
                        }

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.alpha(character, 1, float.Parse(action.Parameter[2])).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.alpha(character, 1, float.Parse(action.Parameter[2])).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        //WaitingForClick();
                    });
                }
                break;
            case "hide":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetCharacterRectTransform(action.CharacterID);
                SetCharacterSprite(action.CharacterID, action.Parameter[1]);

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.alpha(character, 0, float.Parse(action.Parameter[0])).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            Destroy(character.gameObject);
                            CharacterRects.Remove(action.CharacterID);
                            return;
                        }                        
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        Destroy(character.gameObject);
                        CharacterRects.Remove(action.CharacterID);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.alpha(character, 0, float.Parse(action.Parameter[0])).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        Destroy(character.gameObject);
                        CharacterRects.Remove(action.CharacterID);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.alpha(character, 0, float.Parse(action.Parameter[0])).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        Destroy(character.gameObject);
                        CharacterRects.Remove(action.CharacterID);
                    });
                }
                break;
            case "move":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetCharacterRectTransform(action.CharacterID);
                Vector2 lastmove = character.localPosition;
                NowStroyActionBox.info = lastmove;
                Vector2 movevector = new Vector2(CharacterLayer.rect.width * float.Parse(action.Parameter[0]), CharacterLayer.rect.height * float.Parse(action.Parameter[1]));

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.moveLocal(character.gameObject, movevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setOnComplete(() =>
                     {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                         {
                             SetActionState(ChatAction.NOWSTATE.DONE, index);
                             return;
                         }

                         MoveLoopAction(action, lastmove, movevector, character, index);

                         SetActionState(ChatAction.NOWSTATE.DONE, index);
                         SetActionIndex(index + 1);
                         DoingAction(NowStroyActionBox.NowIndex);
                         return;
                     });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.moveLocal(character.gameObject, movevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setLoopPingPong(0).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        MoveLoopAction(action, lastmove, movevector, character, index);

                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.moveLocal(character.gameObject, movevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setOnComplete(() =>
                    {
                        MoveLoopAction(action, lastmove, movevector, character, index);
                    });
                }
                break;
            case "scale":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetCharacterRectTransform(action.CharacterID);
                Vector3 lastscale = character.localScale;
                NowStroyActionBox.info = lastscale;
                Vector3 scalevector = new Vector3(float.Parse(action.Parameter[0]), float.Parse(action.Parameter[1]), 1);
                if ((float.Parse(action.Parameter[0]) < 0 && character.localScale.x < 0)|| (float.Parse(action.Parameter[0]) > 0 && character.localScale.x < 0))
                {
                    scalevector = new Vector3(float.Parse(action.Parameter[0]) * -1, float.Parse(action.Parameter[1]), 1);
                }
                if ((float.Parse(action.Parameter[1]) < 0 && character.localScale.y < 0) || (float.Parse(action.Parameter[1]) > 0 && character.localScale.y < 0))
                {
                    scalevector = new Vector3(scalevector.x, float.Parse(action.Parameter[1]) * -1, 1);
                }

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.scale(character, scalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }
                        ScaleLoopAction(action, lastscale, scalevector, character, index);
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.scale(character, scalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        ScaleLoopAction(action, lastscale, scalevector, character, index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.scale(character, scalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setOnComplete(() =>
                    {
                        ScaleLoopAction(action, lastscale, scalevector, character, index);
                    });
                }
                break;
            case "rotate":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetCharacterRectTransform(action.CharacterID);
                Vector3 lastangle = character.localRotation.eulerAngles;
                NowStroyActionBox.info = lastangle;
                Vector3 angle = new Vector3(lastangle.x, lastangle.y, float.Parse(action.Parameter[0]));

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.rotateLocal(character.gameObject, angle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.Parameter[2])).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }
                        RotateLoopAction(action, lastangle, angle, character, index);

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.rotateLocal(character.gameObject, angle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.Parameter[2])).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        RotateLoopAction(action, lastangle, angle, character, index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.rotateLocal(character.gameObject, angle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.Parameter[2])).setOnComplete(() =>
                    {
                        RotateLoopAction(action, lastangle, angle, character, index);
                    });
                }
                break;
            case "windowmove":
                //因为读取配置表是一个通用方法，因此读取CharacterID的配置为了MoveType，不过为了方便就这样吧
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = TextBoardLayer.WordsBacklayer;
                Vector2 windowlastmove = character.localPosition;
                NowStroyActionBox.info = windowlastmove;
                Vector2 windowmovevector = new Vector2(windowlastmove.x + StoryBoardLayer.rect.width * float.Parse(action.Parameter[0]), windowlastmove.y + StoryBoardLayer.rect.height * float.Parse(action.Parameter[1]));

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.moveLocal(character.gameObject, windowmovevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }

                        MoveLoopActionWindowOrBg(action, windowlastmove, windowmovevector, character, index);

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.moveLocal(character.gameObject, windowmovevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setLoopPingPong(0).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        MoveLoopActionWindowOrBg(action, windowlastmove, windowmovevector, character, index);

                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.moveLocal(character.gameObject, windowmovevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        MoveLoopActionWindowOrBg(action, windowlastmove, windowmovevector, character, index);
                    });
                }
                break;
            case "windowscale":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = TextBoardLayer.WordsBacklayer;
                Vector3 windowlastscale = character.localScale;
                NowStroyActionBox.info = windowlastscale;
                Vector3 windowscalevector = new Vector3(float.Parse(action.Parameter[0]), float.Parse(action.Parameter[1]), 1);
                if ((float.Parse(action.Parameter[0]) < 0 && character.localScale.x < 0) || (float.Parse(action.Parameter[0]) > 0 && character.localScale.x < 0))
                {
                    windowscalevector = new Vector3(float.Parse(action.Parameter[0]) * -1, float.Parse(action.Parameter[1]), 1);
                }
                if ((float.Parse(action.Parameter[1]) < 0 && character.localScale.y < 0) || (float.Parse(action.Parameter[1]) > 0 && character.localScale.y < 0))
                {
                    windowscalevector = new Vector3(windowscalevector.x, float.Parse(action.Parameter[1]) * -1, 1);
                }

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.scale(character, windowscalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }
                        ScaleLoopActionWindowOrBg(action, windowlastscale, windowscalevector, character, index);
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.scale(character, windowscalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        ScaleLoopActionWindowOrBg(action, windowlastscale, windowscalevector, character,index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.scale(character, windowscalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        ScaleLoopActionWindowOrBg(action, windowlastscale, windowscalevector, character, index);
                    });
                }
                break;
            case "windowrotate":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = TextBoardLayer.WordsBacklayer;
                Vector3 windowlastangle = character.localRotation.eulerAngles;
                NowStroyActionBox.info = windowlastangle;
                Vector3 windowangle = new Vector3(windowlastangle.x, windowlastangle.y, float.Parse(action.Parameter[0]));

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.rotateLocal(character.gameObject, windowangle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }
                        RotateLoopActionWindowOrBg(action, windowlastangle, windowangle, character, index);

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.rotateLocal(character.gameObject, windowangle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        RotateLoopActionWindowOrBg(action, windowlastangle, windowangle, character, index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.rotateLocal(character.gameObject, windowangle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        RotateLoopActionWindowOrBg(action, windowlastangle, windowangle, character, index);
                    });
                }
                break;
            case "bgmove":
                //因为读取配置表是一个通用方法，因此读取CharacterID的配置为了MoveType，不过为了方便就这样吧
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetBGLayer();
                Vector2 bglastmove = character.localPosition;
                NowStroyActionBox.info = bglastmove;
                Vector2 bgvector = new Vector2(bglastmove.x + StoryBoardLayer.rect.width * float.Parse(action.Parameter[0]), bglastmove.y + StoryBoardLayer.rect.height * float.Parse(action.Parameter[1]));

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.moveLocal(character.gameObject, bgvector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }

                        MoveLoopActionWindowOrBg(action, bglastmove, bgvector, character, index);

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.moveLocal(character.gameObject, bgvector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setLoopPingPong(0).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        MoveLoopActionWindowOrBg(action, bglastmove, bgvector, character, index);

                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.moveLocal(character.gameObject, bgvector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        MoveLoopActionWindowOrBg(action, bglastmove, bgvector, character, index);
                    });
                }
                break;
            case "bgscale":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetBGLayer();
                Vector3 bglastscale = character.localScale;
                NowStroyActionBox.info = bglastscale;
                Vector3 bgscalevector = new Vector3(float.Parse(action.Parameter[0]), float.Parse(action.Parameter[1]), 1);
                if ((float.Parse(action.Parameter[0]) < 0 && character.localScale.x < 0) || (float.Parse(action.Parameter[0]) > 0 && character.localScale.x < 0))
                {
                    bgscalevector = new Vector3(float.Parse(action.Parameter[0]) * -1, float.Parse(action.Parameter[1]), 1);
                }
                if ((float.Parse(action.Parameter[1]) < 0 && character.localScale.y < 0) || (float.Parse(action.Parameter[1]) > 0 && character.localScale.y < 0))
                {
                    bgscalevector = new Vector3(bgscalevector.x, float.Parse(action.Parameter[1]) * -1, 1);
                }

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.scale(character, bgscalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }
                        ScaleLoopActionWindowOrBg(action, bglastscale, bgscalevector, character, index);
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.scale(character, bgscalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        ScaleLoopActionWindowOrBg(action, bglastscale, bgscalevector, character, index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.scale(character, bgscalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        ScaleLoopActionWindowOrBg(action, bglastscale, bgscalevector, character, index);
                    });
                }
                break;
            case "bgrotate":
                SetActionState(ChatAction.NOWSTATE.DOING, index);
                character = GetBGLayer();
                Vector3 bglastangle = character.localRotation.eulerAngles;
                NowStroyActionBox.info = bglastangle;
                Vector3 bgangle = new Vector3(bglastangle.x, bglastangle.y, float.Parse(action.Parameter[0]));

                if (action.SkipType == ChatAction.SKIPTYPE.AUTO)
                {
                    LeanTween.rotateLocal(character.gameObject, bgangle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            return;
                        }
                        RotateLoopActionWindowOrBg(action, bglastangle, bgangle, character, index);

                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        SetActionIndex(index + 1);
                        DoingAction(NowStroyActionBox.NowIndex);
                        return;
                    });

                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.rotateLocal(character.gameObject, bgangle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);

                        RotateLoopActionWindowOrBg(action, bglastangle, bgangle, character, index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.rotateLocal(character.gameObject, bgangle, float.Parse(action.Parameter[1])).setEase(GetLeanTweenType(action.CharacterID)).setOnComplete(() =>
                    {
                        RotateLoopActionWindowOrBg(action, bglastangle, bgangle, character, index);
                    });
                }
                break;
            case "talk":
                if (TextBoardLayer.NameText.text != NowStroyActionBox.CharacterList[action.CharacterID].Name)
                {
                    //改变窗口
                    SetChatWindow(action);
                    SpeakerAudioManager.clip = GetVoice(action.CharacterID);
                    return;
                }
                SetActionState(ChatAction.NOWSTATE.DOING, index);

                //对话最开始
                if ((preaction.Command == "talk" && preaction.Parameter[3] == "endpage" && TextBoardLayer.WordsText.text != "") ||(preaction.Command != "talk"))
                {
                    GameObject hideText = Instantiate(TextBoardLayer.WordsText.gameObject, TextBoardLayer.WordsText.transform.parent);
                    LeanTween.moveLocalY(hideText, 200, 0.5f).setOnComplete(()=>
                    {
                        Destroy(hideText);
                    });
                    TextBoardLayer.WordsText.text = "";
                }

                //获取上一条对话的语句
                lastWords = TextBoardLayer.WordsText.text;

                Regex reg = new Regex("(<.*?>)(.*?)(<.*?>)", RegexOptions.IgnoreCase);
                string replacestr = reg.Replace(action.Parameter[0], @"$2");
                int wordslengh = TextBoardLayer.WordsText.text.Length;
                int nextwordslength = wordslengh + replacestr.Length;

                string origText = TextBoardLayer.WordsText.text + action.Parameter[0];
                float speed = float.Parse(action.Parameter[1]);
                string face = action.Parameter[2];

                //设置角色表情
                SetCharacterSprite(action.CharacterID, face);

                //区分不同的动作方式
                if (action.SkipType == ChatAction.SKIPTYPE.AUTO || action.SkipType == ChatAction.SKIPTYPE.TimeAUTO)
                {
                    LeanTween.value(TextBoardLayer.WordsText.gameObject, wordslengh, (float)nextwordslength, speed * nextwordslength).setOnUpdate((float val) =>
                    {
                        SetTextBoardWords(origText, Mathf.RoundToInt(val), wordslengh, action.Richparamater);
                    }).setOnComplete(() =>
                    {
                        //如果是最后一个动作，则停止自动
                        if (index >= NowStroyActionBox.ActionList.Count - 1)
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            WaitingForClick(action.CharacterID);
                            return;
                        }

                        if (action.Parameter[3] == "endpage")
                        {
                            LeanTween.delayedCall(nextwordslength * NowConfig.speed, () =>
                            {
                                SetActionState(ChatAction.NOWSTATE.DONE, index);
                                SetActionIndex(index + 1);
                                DoingAction(NowStroyActionBox.NowIndex);
                            });
                        }
                        else 
                        {
                            SetActionState(ChatAction.NOWSTATE.DONE, index);
                            SetActionIndex(index + 1);
                            DoingAction(NowStroyActionBox.NowIndex);
                        }
                        return;
                    });
                }
                else if (action.SkipType == ChatAction.SKIPTYPE.SAMETIME)
                {
                    LeanTween.value(TextBoardLayer.WordsText.gameObject, wordslengh, (float)nextwordslength, speed * nextwordslength).setOnUpdate((float val) =>
                    {
                        SetTextBoardWords(origText, Mathf.RoundToInt(val), wordslengh, action.Richparamater);
                    }).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    });
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    LeanTween.value(TextBoardLayer.WordsText.gameObject, wordslengh, (float)nextwordslength, speed * nextwordslength).setOnUpdate((float val) =>
                    {
                        SetTextBoardWords(origText, Mathf.RoundToInt(val), wordslengh, action.Richparamater);
                    }).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                        WaitingForClick(action.CharacterID);
                    });
                }
                break;
            case "playbgm":
                SetActionState(ChatAction.NOWSTATE.DOING, index);

                if (AudioManager.clip != null)
                {
                    LeanTween.value(AudioManager.gameObject, 1, 0, 1f).setOnUpdate((float vol) =>
                    {
                        AudioManager.volume = vol;
                    }).setOnComplete(() =>
                    {
                        LeanTween.value(AudioManager.gameObject, 0, 1, 1f).setOnUpdate((float vol) =>
                        {
                            AudioManager.volume = vol;
                        });
                        AudioManager.clip = GetBGM(action.Parameter[0]);
                        AudioManager.Play();
                    });
                }
                else
                {
                    LeanTween.value(AudioManager.gameObject, 0, 1, 1).setOnUpdate((float vol) =>
                    {
                        AudioManager.volume = vol;
                    });
                    AudioManager.clip = GetBGM(action.Parameter[0]);
                    AudioManager.Play();
                }

                //如果是最后一个动作，则停止自动
                if (index >= NowStroyActionBox.ActionList.Count)
                {
                    SetActionState(ChatAction.NOWSTATE.DONE, index);
                    return;
                }
                SetActionState(ChatAction.NOWSTATE.DONE, index);
                SetActionIndex(index + 1);
                DoingAction(NowStroyActionBox.NowIndex);
                break;
            case "stopbgm":
                if (AudioManager.clip != null)
                {
                    LeanTween.value(AudioManager.gameObject, 1, 0, 1f).setOnUpdate((float vol) =>
                    {
                        AudioManager.volume = vol;
                    }).setOnComplete(() =>
                    {
                        AudioManager.clip = null;
                        AudioManager.Stop();
                    });
                }

                SetActionState(ChatAction.NOWSTATE.DONE, index);
                SetActionIndex(index + 1);
                DoingAction(NowStroyActionBox.NowIndex);
                break;
            case "loadstory":
                SetActionState(ChatAction.NOWSTATE.DONE, index);
                ChangeStory(action.Parameter[0]);
                break;
            case "changescene":
                SetActionState(ChatAction.NOWSTATE.DONE, index);
                SceneManager.LoadScene(action.Parameter[0]);
                break;
            case "selection":
                SetActionState(ChatAction.NOWSTATE.DONE, index);
                SetIsSelection(true);
                SelectionLayer.GetComponent<CanvasGroup>().blocksRaycasts = true;

                RectTransform selction = Instantiate(SelectionPrefab,SelectionLayer).GetComponent<RectTransform>();
                SetSelectionWindow(action, selction);

                ChatAction.StoryAction nextaction = (ChatAction.StoryAction)NowStroyActionBox.ActionList[index+1];
                if (nextaction.Command.CompareTo("selection") == 0)
                {
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                break;
            case "benginflag":
                SetActionState(ChatAction.NOWSTATE.DONE, index);
                if (FlagList.Contains(action.Parameter[0]))
                {
                    SetActionIndex(index + 1);
                    DoingAction(NowStroyActionBox.NowIndex);
                    return;
                }
                else
                {
                    //int checkindex = index + 1;
                    for (int checkindex = index + 1; checkindex < NowStroyActionBox.ActionList.Count; checkindex++)
                    {
                        SetActionIndex(checkindex);
                        SetActionState(ChatAction.NOWSTATE.DONE, checkindex);
                        ChatAction.StoryAction checkaction = (ChatAction.StoryAction)NowStroyActionBox.ActionList[checkindex];
                        if (checkaction.Command.CompareTo("endflag") == 0)
                        {
                            SetActionIndex(checkindex + 1);
                            DoingAction(NowStroyActionBox.NowIndex);
                            return;
                        }
                    }
                }
                break;
            case "endflag":
                SetActionState(ChatAction.NOWSTATE.DONE, index);
                SetActionIndex(index + 1);
                DoingAction(NowStroyActionBox.NowIndex);
                break;
            default:
                Debug.Log("Don't have Action: <color=red>" + action.Command + "</color> in <color=green>" + storyName + ".txt</color>!");
                SetActionState(ChatAction.NOWSTATE.DONE, index);
                SetActionIndex(index + 1);
                DoingAction(NowStroyActionBox.NowIndex);
                break;
        }
    }

    void SetIsSelection(bool boolen)
    {
        IsSelection = boolen;
    }

    void ClickToDoing(GameObject obj)
    {
        //如果是对话状态，则不响应点击事件
        if (IsSelection)
            return;

        //如果完成所有动作
        if (NowStroyActionBox.NowIndex >= NowStroyActionBox.ActionList.Count)
        {
            return;
        }

        //如果当前动作为Doing且下一步为Click，则遍历所有正在Doing的且状态不为Loop的动作，设置为完成状态。
        ChatAction.StoryAction action = (ChatAction.StoryAction)NowStroyActionBox.ActionList[NowStroyActionBox.NowIndex];
        if (action.NowState == ChatAction.NOWSTATE.DOING && (action.SkipType == ChatAction.SKIPTYPE.CLICK || action.SkipType == ChatAction.SKIPTYPE.TimeAUTO || action.SkipType == ChatAction.SKIPTYPE.SAMETIME))
        {
            for (int i = 0; i < NowStroyActionBox.ActionList.Count; i++)
            {
                ChatAction.StoryAction _action = (ChatAction.StoryAction)NowStroyActionBox.ActionList[i];
                if (_action.NowState == ChatAction.NOWSTATE.DOING && _action.LoopType != "loop")
                {
                    RectTransform rt;
                    RectTransform bgrt;
                    switch (_action.Command)
                    {
                        case "setbg":
                            rt = GetBGLayer();
                            bgrt = GetNowBGLayer();
                            LeanTween.cancel(rt.gameObject);
                            LeanTween.cancel(bgrt.gameObject);
                            rt.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                            bgrt.GetComponent<Image>().color = Color.white;
                            NowStroyActionBox.UseBG1 = !NowStroyActionBox.UseBG1;
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "stop":
                            //获取id对象
                            if (_action.CharacterID == "WINDOW")
                                rt = TextBoardLayer.WordsBacklayer;
                            else if (_action.CharacterID == "BG")
                                rt = GetBGLayer();
                            else
                                rt = GetCharacterRectTransform(_action.CharacterID);
                            LeanTween.cancel(rt.gameObject);
                            break;
                        case "show":
                            rt = GetCharacterRectTransform(_action.CharacterID);
                            LeanTween.cancel(rt.gameObject);
                            Color rtcshow = rt.GetComponent<Image>().color;
                            rt.GetComponent<Image>().color = new Color(rtcshow.r, rtcshow.g, rtcshow.b, 1);
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "hide":
                            rt = GetCharacterRectTransform(_action.CharacterID);
                            LeanTween.cancel(rt.gameObject);
                            Destroy(rt.gameObject);
                            CharacterRects.Remove(_action.CharacterID);
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "move":
                            rt = GetCharacterRectTransform(_action.CharacterID);
                            Vector2 movevector = new Vector2(CharacterLayer.rect.width * float.Parse(_action.Parameter[0]), CharacterLayer.rect.height * float.Parse(_action.Parameter[1]));
                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localPosition = movevector;
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localPosition = (Vector2)NowStroyActionBox.info;
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "scale":
                            rt = GetCharacterRectTransform(_action.CharacterID);
                            Vector3 scalevector = new Vector3(float.Parse(_action.Parameter[0]), float.Parse(_action.Parameter[1]), 1);
                            if ((float.Parse(_action.Parameter[0]) < 0 && rt.localScale.x < 0) || (float.Parse(_action.Parameter[0]) > 0 && rt.localScale.x < 0))
                            {
                                scalevector = new Vector3(float.Parse(_action.Parameter[0]) * -1, float.Parse(_action.Parameter[1]), 1);
                            }
                            if ((float.Parse(_action.Parameter[1]) < 0 && rt.localScale.y < 0) || (float.Parse(_action.Parameter[1]) > 0 && rt.localScale.y < 0))
                            {
                                scalevector = new Vector3(scalevector.x, float.Parse(_action.Parameter[1]) * -1, 1);
                            }

                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localScale = scalevector;
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localScale = (Vector3)NowStroyActionBox.info;
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "rotate":
                            rt = GetCharacterRectTransform(_action.CharacterID);
                            float angle = float.Parse(_action.Parameter[0]);
                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.rotation = Quaternion.Euler(0, 0, angle);
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.rotation = Quaternion.Euler((Vector3)NowStroyActionBox.info);
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "windowmove":
                            rt = TextBoardLayer.WordsBacklayer;
                            Vector2 windowsmovevector = new Vector2(rt.localPosition.x + StoryBoardLayer.rect.width * float.Parse(_action.Parameter[0]), rt.localPosition.y + StoryBoardLayer.rect.height * float.Parse(_action.Parameter[1]));
                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType!= "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localPosition = windowsmovevector;
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localPosition = (Vector2)NowStroyActionBox.info;
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "windowscale":
                            rt = TextBoardLayer.WordsBacklayer;
                            Vector3 windowscalevector = new Vector3(float.Parse(_action.Parameter[0]), float.Parse(_action.Parameter[1]), 1);
                            if ((float.Parse(_action.Parameter[0]) < 0 && rt.localScale.x < 0) || (float.Parse(_action.Parameter[0]) > 0 && rt.localScale.x < 0))
                            {
                                windowscalevector = new Vector3(float.Parse(_action.Parameter[0]) * -1, float.Parse(_action.Parameter[1]), 1);
                            }
                            if ((float.Parse(_action.Parameter[1]) < 0 && rt.localScale.y < 0) || (float.Parse(_action.Parameter[1]) > 0 && rt.localScale.y < 0))
                            {
                                windowscalevector = new Vector3(windowscalevector.x, float.Parse(_action.Parameter[1]) * -1, 1);
                            }

                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localScale = windowscalevector;
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localScale = (Vector3)NowStroyActionBox.info;
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "windowrotate":
                            rt = TextBoardLayer.WordsBacklayer;
                            float windowangle = float.Parse(_action.Parameter[0]);
                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.rotation = Quaternion.Euler(0, 0, windowangle);
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.rotation = Quaternion.Euler((Vector3)NowStroyActionBox.info);
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "bgmove":
                            rt = GetBGLayer();
                            Vector2 bgvector = new Vector2(rt.localPosition.x + CharacterLayer.rect.width * float.Parse(_action.Parameter[0]), rt.localPosition.y + CharacterLayer.rect.height * float.Parse(_action.Parameter[1]));
                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localPosition = bgvector;
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localPosition = (Vector2)NowStroyActionBox.info;
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "bgscale":
                            rt = GetBGLayer();
                            Vector3 bgscalevector = new Vector3(float.Parse(_action.Parameter[0]), float.Parse(_action.Parameter[1]), 1);
                            if ((float.Parse(_action.Parameter[0]) < 0 && rt.localScale.x < 0) || (float.Parse(_action.Parameter[0]) > 0 && rt.localScale.x < 0))
                            {
                                windowscalevector = new Vector3(float.Parse(_action.Parameter[0]) * -1, float.Parse(_action.Parameter[1]), 1);
                            }
                            if ((float.Parse(_action.Parameter[1]) < 0 && rt.localScale.y < 0) || (float.Parse(_action.Parameter[1]) > 0 && rt.localScale.y < 0))
                            {
                                windowscalevector = new Vector3(bgscalevector.x, float.Parse(_action.Parameter[1]) * -1, 1);
                            }

                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localScale = bgscalevector;
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.localScale = (Vector3)NowStroyActionBox.info;
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "bgrotate":
                            rt = GetBGLayer();
                            float bgangle = float.Parse(_action.Parameter[0]);
                            //如果是循环，则无视
                            if (_action.LoopType != "loop" && _action.LoopType != "pingpong")
                            {
                                if (!MathTool.isNumber(_action.LoopType))
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.rotation = Quaternion.Euler(0, 0, bgangle);
                                }
                                else
                                {
                                    LeanTween.cancel(rt.gameObject);
                                    rt.rotation = Quaternion.Euler((Vector3)NowStroyActionBox.info);
                                }
                            }
                            SetActionState(ChatAction.NOWSTATE.DONE, i);
                            break;
                        case "talk":
                            LeanTween.cancel(TextBoardLayer.WordsText.gameObject);
                            //如果是设置文字速度的自动模式，则可以点击加速
                            if (_action.SkipType == ChatAction.SKIPTYPE.TimeAUTO)
                            {
                                for (int wordsindex = NowStroyActionBox.NowIndex; wordsindex < NowStroyActionBox.ActionList.Count; wordsindex++)
                                {
                                    ChatAction.StoryAction talkaction = (ChatAction.StoryAction)NowStroyActionBox.ActionList[wordsindex];
                                    if (talkaction.Command == "talk")
                                    {
                                        SetActionState(ChatAction.NOWSTATE.DONE, wordsindex);
                                        SetActionIndex(wordsindex);
                                        lastWords += talkaction.Parameter[0];
                                        if (talkaction.Parameter[3] == "endpage")
                                            break;
                                    }
                                    else
                                    {
                                        SetActionState(ChatAction.NOWSTATE.DONE, wordsindex);
                                        SetActionIndex(wordsindex);
                                        Debug.Log("嗯。。一定是你的读取方式或者配置错了，总之先让他过去呗~");
                                    }
                                }
                                TextBoardLayer.WordsText.text = lastWords;
                            }
                            //如果是点击模式，则直接点击加速
                            else
                            {
                                lastWords += _action.Parameter[0];
                                TextBoardLayer.WordsText.text = lastWords;
                                SetActionState(ChatAction.NOWSTATE.DONE, i);
                            }
                            WaitingForClick(_action.CharacterID);
                            break;
                    }
                }
            }
        }//end if
        //如果为Done，则直接进入下一步
        else if (action.NowState == ChatAction.NOWSTATE.DONE && action.SkipType == ChatAction.SKIPTYPE.CLICK)
        {
            HideClickHint();

            if (NowStroyActionBox.NowIndex >= NowStroyActionBox.ActionList.Count - 1)
            {
                EndChatLayer();
            }
            else
            {
                SetActionIndex(NowStroyActionBox.NowIndex + 1);
                DoingAction(NowStroyActionBox.NowIndex);
            }
        }
        else if (action.NowState == ChatAction.NOWSTATE.DONE && NowStroyActionBox.NowIndex >= NowStroyActionBox.ActionList.Count - 1)
        {
            EndChatLayer();
        }
    }

    void WaitingForClick(string id)
    {
        //如果已经显示，则跳过
        if (!AniController.Get(TextBoardLayer.ClickHintLayer).IsPlaying())
        {
            Sprite[] hintsprite;
            if (NowResourcesBox.windowsSprites.TryGetValue(id, out hintsprite))
            {
                AniController.Get(TextBoardLayer.ClickHintLayer).AddSprite(hintsprite);
                AniController.Get(TextBoardLayer.ClickHintLayer).PlayAni(4, 7, AniController.AniType.Loop, 5);

                Color c = TextBoardLayer.ClickHintLayer.GetComponent<Image>().color;
                TextBoardLayer.ClickHintLayer.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
            }
            else
                Debug.Log("can't find " + id);
        }
    }

    void HideClickHint()
    {
        LeanTween.alpha(TextBoardLayer.ClickHintLayer, 0, 0.15f);
        AniController.Get(TextBoardLayer.ClickHintLayer).Stop();
    }

    int clipindex = -1;
    bool isfindText = false;
    void SetTextBoardWords(string origText, int val, int lastwordslengh, MatchCollection mac)
    {
        if (val == clipindex || val == lastwordslengh)
            return;

        //Debug.Log("clipindex:"+ clipindex);
        if (mac.Count > 0)
        {
            int offset = 0;
            isfindText = false;

            for (int i = 0; i < mac.Count; i++)
            {
                if (val > mac[i].Index - offset + lastwordslengh && val <= mac[i].Groups[3].Index - mac[i].Groups[1].Length - offset + lastwordslengh)
                {
                    //int nowlengh = (val - mac[i].Index + offset) * (mac[i].Groups[3].Length + mac[i].Groups[1].Length + 1);
                    TextBoardLayer.WordsText.text = origText.Substring(0, mac[i].Index + mac[i].Length + lastwordslengh);
                    
                    //TextBoardLayer.WordsText.text = origText.Substring(0, val);
                    //Debug.Log("okokokok" + val + "   " + nowlengh);

                    isfindText = true;
                    break;
                }

                offset += mac[i].Groups[1].Length + mac[i].Groups[3].Length;
            }

            if (!isfindText)
                TextBoardLayer.WordsText.text = origText.Substring(0, TextBoardLayer.WordsText.text.Length + val - clipindex);

            //string words = "";
            //if (val == mac[clipindex].Length)
            //{
            //    words = origText.Substring(0, val + mac[clipindex].Length);
            //    TextBoardLayer.WordsText.text = words;
            //    clipindex++;
            //}
            //else
            //    TextBoardLayer.WordsText.text = origText.Substring(0, val);
        }
        else
        {
            TextBoardLayer.WordsText.text = origText.Substring(0, val);
        }

        clipindex = val;
        SpeakerAudioManager.Play();
    }


    void SetActionIndex(int index)
    {
        NowStroyActionBox.NowIndex = index;
    }

    void SetActionState(ChatAction.NOWSTATE state, int index)
    {
        ChatAction.StoryAction action = (ChatAction.StoryAction)NowStroyActionBox.ActionList[index];
        action.NowState = state;
        NowStroyActionBox.ActionList[index] = action;

        clipindex = 0;
    }

    //获取window的图片
    Sprite[] GetWindowsSprit(string name)
    {
        if (name == null)
            return null;

        Sprite[] s;
        if (NowResourcesBox.windowsSprites.TryGetValue(name,out s))
            return s;
        return null;  //如果找不到则返回-1
    }

    //获取背景的图片
    Sprite GetBGSprit(string name)
    {
        if (name == null)
            return null;

        Sprite s;
        if (NowResourcesBox.bgSprites.TryGetValue(name, out s))
            return s;
        return null;  //如果找不到则返回-1
    }

    //获取BGM
    AudioClip GetBGM(string name)
    {
        if (name == null)
            return null;

        AudioClip s;
        if (NowResourcesBox.bgms.TryGetValue(name, out s))
            return s;
        return null;  //如果找不到则返回-1
    }

    //获取Voice
    AudioClip GetVoice(string name)
    {
        if (name == null)
            return null;

        AudioClip s;
        if (NowResourcesBox.voices.TryGetValue(name, out s))
            return s;
        return null;  //如果找不到则返回-1
    }

    //获取角色RectTransform
    RectTransform GetCharacterRectTransform(string id)
    {
        if (id == null)
            return null;

        RectTransform c;
        if (CharacterRects == null)
        {
            CharacterRects = new Dictionary<string, RectTransform>();
        }
        if (CharacterRects.TryGetValue(id, out c))
        {
            return c;
        }
        else
        {
            GameObject obj = new GameObject();
            obj.name = id;
            obj.layer = 5;
            obj.transform.SetParent(CharacterLayer,false);
            c = obj.AddComponent<RectTransform>();
            c.pivot = new Vector2(0.5f, 0);

            Image img = obj.AddComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0);

            CharacterRects.Add(id, c);

            return c;
        }
    }

    //更改图片
    void SetCharacterSprite(string id,string name)
    {
        RectTransform rt = new RectTransform();
        if (CharacterRects.TryGetValue(id, out rt))
        {
            Image objimg = rt.GetComponent<Image>();
            objimg.sprite = NowResourcesBox.characterSprites[id][name];
            objimg.SetNativeSize();
            //rt.localScale = new Vector3(rt.localScale.x, rt.localScale.y, 1);
        }
        else
        {
            Debug.Log("can't find " + id + ":" + name);
        }
    }

    //更改窗口
    void SetChatWindow(ChatAction.StoryAction action)
    {
        LeanTween.scaleY(TextBoardLayer.WordsBacklayer.gameObject, 0, 0.25f).setOnComplete(() =>
        {
            StoryBoardLayer.localPosition = new Vector3(0, 0, 0);
            TextBoardLayer.WordsBacklayer.localScale = new Vector3(1, 0, 1);

            Sprite[] s = GetWindowsSprit(action.CharacterID);
            TextBoardLayer.WordsBacklayer.GetComponent<Image>().sprite = s[0];
            TextBoardLayer.WordsOutLayer.GetComponent<Image>().sprite = s[1];
            TextBoardLayer.WordsText.text = "";

            TextBoardLayer.NameBackLayer.GetComponent<Image>().sprite = s[0];
            TextBoardLayer.NameOutLayer.GetComponent<Image>().sprite = s[1];
            TextBoardLayer.NameText.text = NowStroyActionBox.CharacterList[action.CharacterID].Name;

            RectTransform rt = GetCharacterRectTransform(action.CharacterID);
            TextBoardLayer.NameBackLayer.localPosition = new Vector3(rt.localPosition.x - TextBoardLayer.WordsBacklayer.rect.width / 2, TextBoardLayer.NameBackLayer.localPosition.y, 1);

            LeanTween.scaleY(TextBoardLayer.WordsBacklayer.gameObject, 1, 0.25f).setOnComplete(() =>
                {
                    DoingAction(NowStroyActionBox.NowIndex);
                });
        });
    }

    //更改选项对话框
    void SetSelectionWindow(ChatAction.StoryAction action,RectTransform rect)
    {
        rect.name = action.Parameter[0];
        Sprite[] s = GetWindowsSprit(action.CharacterID);
        rect.GetComponent<Image>().sprite = s[0];
        rect.Find("OutBoard").GetComponent<Image>().sprite = s[1];
        rect.Find("Text").GetComponent<Text>().text = action.Parameter[1];
        EventTriggerListener.Get(rect).onClick = ClickSelection;
    }

    //更改选项对话框
    void ClickSelection(GameObject go)
    {
        Debug.Log("添加flag：" + go.name);
        if(!FlagList.Contains(go.name))
            FlagList.Add(go.name);

        SelectionLayer.GetComponent<CanvasGroup>().blocksRaycasts = false;
        SelectionLayer.GetComponent<ChatSelectionList>().ClearSelection(go.name,() => 
        {
            SetIsSelection(false);
            ClickToDoing(go);
        }
        );
    }

    void MoveLoopAction(ChatAction.StoryAction action,Vector3 lastmove,Vector3 movevector, RectTransform character,int index)
    {
        //如果是循环，则继续播放动作
        if (action.LoopType == "loop")
        {
            character.position = lastmove;
            LeanTween.move(character.gameObject, movevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setLoopClamp();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (action.LoopType == "pingpong")
        {
            LeanTween.move(character.gameObject, lastmove, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.Parameter[3])).setLoopPingPong();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (MathTool.isNumber(action.LoopType))
        {
            int count = int.Parse(action.LoopType);
            LeanTween.move(character.gameObject, lastmove, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.Parameter[3])).setOnComplete(()=>
            {
                if (count >= 1)
                    LeanTween.move(character.gameObject, movevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setLoopPingPong(count - 1).setLoopPingPong(count - 1).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    }); ;
            });
        }
        else
            SetActionState(ChatAction.NOWSTATE.DONE, index);
    }

    void MoveLoopActionWindowOrBg(ChatAction.StoryAction action, Vector3 lastmove, Vector3 movevector, RectTransform character,int index)
    {
        //如果是循环，则继续播放动作
        if (action.LoopType == "loop")
        {
            character.position = lastmove;
            LeanTween.move(character.gameObject, movevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setLoopClamp();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (action.LoopType == "pingpong")
        {
            LeanTween.move(character.gameObject, lastmove, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.CharacterID)).setLoopPingPong();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (MathTool.isNumber(action.LoopType))
        {
            int count = int.Parse(action.LoopType);
            LeanTween.move(character.gameObject, lastmove, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.CharacterID)).setOnComplete(() =>
            {
                if (count >= 1)
                    LeanTween.move(character.gameObject, movevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setLoopPingPong(count - 1).setOnComplete(()=>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    });
            });
        }
        else
            SetActionState(ChatAction.NOWSTATE.DONE, index);
    }

    void ScaleLoopAction(ChatAction.StoryAction action, Vector3 lastscale, Vector3 scalevector, RectTransform character,int index)
    {
        //如果是循环，则继续播放动作
        if (action.LoopType == "loop")
        {
            character.localScale = lastscale;
            LeanTween.scale(character, scalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setLoopClamp();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (action.LoopType == "pingpong")
        {
            LeanTween.scale(character, lastscale, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.Parameter[3])).setLoopPingPong();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (MathTool.isNumber(action.LoopType))
        {
            int count = int.Parse(action.LoopType);
            LeanTween.scale(character, lastscale, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.Parameter[3])).setOnComplete(() =>
            {
                if (count >= 1)
                    LeanTween.scale(character, scalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.Parameter[3])).setLoopPingPong(count - 1).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    }); ;
            });
        }
        else
            SetActionState(ChatAction.NOWSTATE.DONE, index);
    }

    void ScaleLoopActionWindowOrBg(ChatAction.StoryAction action, Vector3 lastscale, Vector3 scalevector, RectTransform character,int index)
    {
        //如果是循环，则继续播放动作
        if (action.LoopType == "loop")
        {
            character.localScale = lastscale;
            LeanTween.scale(character, scalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setLoopClamp();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (action.LoopType == "pingpong")
        {
            LeanTween.scale(character, lastscale, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.CharacterID)).setLoopPingPong();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (MathTool.isNumber(action.LoopType))
        {
            int count = int.Parse(action.LoopType);
            LeanTween.scale(character, lastscale, float.Parse(action.Parameter[2])).setEase(GetAniLeanTweenType(action.CharacterID)).setOnComplete(() =>
            {
                if (count > 1)
                    LeanTween.scale(character, scalevector, float.Parse(action.Parameter[2])).setEase(GetLeanTweenType(action.CharacterID)).setLoopPingPong(count - 1).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    });
                else if (count ==1)
                    SetActionState(ChatAction.NOWSTATE.DONE, index);
            });
        }
        else
            SetActionState(ChatAction.NOWSTATE.DONE, index);
    }

    void RotateLoopAction(ChatAction.StoryAction action, Vector3 lastangle, Vector3 angle, RectTransform character,int index)
    {
        //如果是循环，则继续播放动作
        if (action.LoopType == "loop")
        {
            character.rotation = Quaternion.Euler(lastangle);
            LeanTween.rotateLocal(character.gameObject, angle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.Parameter[2])).setLoopClamp();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (action.LoopType == "pingpong")
        {
            LeanTween.rotateLocal(character.gameObject, lastangle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.Parameter[2])).setLoopPingPong();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (MathTool.isNumber(action.LoopType))
        {
            int count = int.Parse(action.LoopType);
            LeanTween.rotateLocal(character.gameObject, lastangle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.Parameter[2])).setOnComplete(() =>
            {
                if (count >= 1)
                    LeanTween.rotateLocal(character.gameObject, angle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.Parameter[2])).setLoopPingPong(count - 1).setLoopPingPong(count - 1).setLoopPingPong(count - 1).setLoopPingPong(count - 1).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    }); ;
            });
        }
        else
            SetActionState(ChatAction.NOWSTATE.DONE, index);
    }

    void RotateLoopActionWindowOrBg(ChatAction.StoryAction action, Vector3 lastangle, Vector3 angle, RectTransform character,int index)
    {
        //如果是循环，则继续播放动作
        if (action.LoopType == "loop")
        {
            character.rotation = Quaternion.Euler(lastangle);
            LeanTween.rotateLocal(character.gameObject, angle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.CharacterID)).setLoopClamp();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (action.LoopType == "pingpong")
        {
            LeanTween.rotateLocal(character.gameObject, lastangle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.CharacterID)).setLoopPingPong();
            SetActionState(ChatAction.NOWSTATE.DONE, index);
        }
        else if (MathTool.isNumber(action.LoopType))
        {
            int count = int.Parse(action.LoopType);
            LeanTween.rotateLocal(character.gameObject, lastangle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.CharacterID)).setOnComplete(() =>
            {
                if (count >= 1)
                    LeanTween.rotateLocal(character.gameObject, angle, float.Parse(action.Parameter[1])).setEase(GetAniLeanTweenType(action.CharacterID)).setLoopPingPong(count - 1).setLoopPingPong(count - 1).setLoopPingPong(count - 1).setLoopPingPong(count - 1).setOnComplete(() =>
                    {
                        SetActionState(ChatAction.NOWSTATE.DONE, index);
                    }); ;
            });
        }
        else
            SetActionState(ChatAction.NOWSTATE.DONE, index);
    }

    LeanTweenType GetLeanTweenType(string type)
    {
        if (type == "linear")
        {
            return LeanTweenType.linear;
        }
        else if (type == "easeInBack")
        {
            return LeanTweenType.easeInBack;
        }
        else if (type == "easeOutBack")
        {
            return LeanTweenType.easeOutBack;
        }
        else if (type == "easeInOutBack")
        {
            return LeanTweenType.easeInOutBack;
        }
        else if (type == "easeInBounce")
        {
            return LeanTweenType.easeInBounce;
        }
        else if (type == "easeOutBounce")
        {
            return LeanTweenType.easeOutBounce;
        }
        else if (type == "easeInOutBounce")
        {
            return LeanTweenType.easeInOutBounce;
        }
        else if (type == "easeInElastic")
        {
            return LeanTweenType.easeInElastic;
        }
        else if (type == "easeOutElastic")
        {
            return LeanTweenType.easeOutElastic;
        }
        else if (type == "easeInOutElastic")
        {
            return LeanTweenType.easeInOutElastic;
        }
        else if (type == "easeInQuad")
        {
            return LeanTweenType.easeInQuad;
        }
        else if (type == "easeOutQuad")
        {
            return LeanTweenType.easeOutQuad;
        }
        else if (type == "easeInOutQuad")
        {
            return LeanTweenType.easeInOutQuad;
        }
        else if (type == "easeInSine")
        {
            return LeanTweenType.easeInSine;
        }
        else if (type == "easeOutSine")
        {
            return LeanTweenType.easeOutSine;
        }
        else if (type == "easeInOutSine")
        {
            return LeanTweenType.easeInOutSine;
        }
        else if (type == "easeInCirc")
        {
            return LeanTweenType.easeInCirc;
        }
        else if (type == "easeOutCirc")
        {
            return LeanTweenType.easeOutCirc;
        }
        else if (type == "easeInOutCirc")
        {
            return LeanTweenType.easeInOutCirc;
        }
        else
        {
            return LeanTweenType.linear;
        }
    }

    LeanTweenType GetAniLeanTweenType(string type)
    {
        if (type == "linear")
        {
            return LeanTweenType.linear;
        }
        else if (type == "easeInBack")
        {
            return LeanTweenType.easeOutBack;
        }
        else if (type == "easeOutBack")
        {
            return LeanTweenType.easeInBack;
        }
        else if (type == "easeInOutBack")
        {
            return LeanTweenType.easeInOutBack;
        }
        else if (type == "easeInBounce")
        {
            return LeanTweenType.easeOutBounce;
        }
        else if (type == "easeOutBounce")
        {
            return LeanTweenType.easeInBounce;
        }
        else if (type == "easeInOutBounce")
        {
            return LeanTweenType.easeInOutBounce;
        }
        else if (type == "easeInElastic")
        {
            return LeanTweenType.easeOutElastic;
        }
        else if (type == "easeOutElastic")
        {
            return LeanTweenType.easeInElastic;
        }
        else if (type == "easeInOutElastic")
        {
            return LeanTweenType.easeInOutElastic;
        }
        else if (type == "easeInQuad")
        {
            return LeanTweenType.easeOutQuad;
        }
        else if (type == "easeOutQuad")
        {
            return LeanTweenType.easeInQuad;
        }
        else if (type == "easeInOutQuad")
        {
            return LeanTweenType.easeInOutQuad;
        }
        else if (type == "easeInSine")
        {
            return LeanTweenType.easeOutSine;
        }
        else if (type == "easeOutSine")
        {
            return LeanTweenType.easeInSine;
        }
        else if (type == "easeInOutSine")
        {
            return LeanTweenType.easeInOutSine;
        }
        else if (type == "easeInCirc")
        {
            return LeanTweenType.easeOutCirc;
        }
        else if (type == "easeOutCirc")
        {
            return LeanTweenType.easeInCirc;
        }
        else if (type == "easeInOutCirc")
        {
            return LeanTweenType.easeInOutCirc;
        }
        else
        {
            return LeanTweenType.linear;
        }
    }

    RectTransform GetBGLayer()
    {
        if (NowStroyActionBox.UseBG1)
            return BGLayer1;
        else
            return BGLayer2;
    }

    RectTransform GetNowBGLayer()
    {
        if (!NowStroyActionBox.UseBG1)
            return BGLayer1;
        else
            return BGLayer2;
    }

    public void SkipStory(GameObject obj)
    {
        btn_Skip.gameObject.SetActive(false);     
        for (int i = NowStroyActionBox.NowIndex; i < NowStroyActionBox.ActionList.Count; i++)
        {
            ChatAction.StoryAction action = (ChatAction.StoryAction)NowStroyActionBox.ActionList[i];
            if (action.Command == "loadstory")
            {
                ChangeStory(action.Parameter[0]);
                return;
            }
        }
        EndChatLayer();
    }

    public void SetCallBack(System.Action action)
    {
        CallBack = action;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class SimpleStoryElement : ElementBase
{

    [System.Serializable]
    public struct StateDo
    {
        public int StateID;
        public TalkBox[] talks;
        public StateAction NextDo;
    }
    [System.Serializable]
    public struct TalkBox
    {
        public Transform character;
        public string talkstring;
    }
    public float speed = 0.1f;
    public StateDo[] DoList;

    GameObject SimpleStoryLayer;
    GameObject UsingStoryLayer;
    Sprite[] hints;
    Text WordsText;
    GameObject Mask;
    GameObject HintLayer;
    StateDo Usingdo;

    int nowindex = 0;
    bool islast = false;

    ArrayList CharactersList;

    // Use this for initialization
    override public void Awake()
    {
        IntiElement();
        AddToLevelManger();

        CharactersList = new ArrayList();
        SimpleStoryLayer = (GameObject)Resources.Load("Prefab/Chat/SimpleStoryLayer");
        hints = Resources.LoadAll<Sprite>("Texture/story/board/none");
        if (hints == null) Debug.LogError("Can't find " + "Texture/story/board/none");
    }

    void Start()
    {
        GetAllCharacters();
    }

    private void Update()
    {
        if (islast)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                DoNextStory(Usingdo);
            }
        }
    }

    void resetState()
    {
        islast = false;
        nowindex = 0;
        Usingdo = new StateDo();
        UsingStoryLayer = null;
    }


    void AddToLevelManger()
    {
        GetLevelManager().AddSompleStoryElement(this);
    }

    public bool CheckDoList()
    {
        int stateID = GetLevelManager().GetNowState();
        Usingdo = GetStateDo(stateID);

        //如果找不到动作，则什么都不做
        if (Usingdo.StateID == -1) return false;

        //播放故事
        ShowStory();
        return true;
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
            return common;
        }
    }

    void ShowStory()
    {
        //播放故事
        if (SimpleStoryLayer == null)
        {
            Debug.Log("找不到故事面板，请检查！");
            return;
        }

        if (UsingStoryLayer == null)
        {
            UsingStoryLayer = Instantiate(SimpleStoryLayer, transform.parent);
            UsingStoryLayer.transform.localPosition = transform.localPosition;
        }

        WordsText = UsingStoryLayer.transform.Find("TextMask/Text").GetComponent<Text>();
        WordsText.text = "";
        Mask = UsingStoryLayer.transform.Find("Mask").gameObject;
        HintLayer = UsingStoryLayer.transform.Find("ClickHint").gameObject;
        HideHint();
        StopCharacterEffect();

        string talkstring = Usingdo.talks[nowindex].talkstring;
        Transform charater = Usingdo.talks[nowindex].character;
        float time = speed * talkstring.Length;
        EventTriggerListener.Get(Mask).onClick = QuickShowText;

        ShowHideCharacterEffect(charater);
        ShowCharacterEffect(charater);

        LeanTween.value(UsingStoryLayer, 0, talkstring.Length, time).setOnUpdate((float val) =>
        {
            WordsText.text = talkstring.Substring(0, (int)val);
        }).setOnComplete(()=>
        {
            ShowHint();
            EventTriggerListener.Get(Mask).onClick = WaitToClick;
        });
    }

    void QuickShowText(GameObject go)
    {
        LeanTween.cancel(UsingStoryLayer);
        if(WordsText==null)
            WordsText = UsingStoryLayer.transform.Find("TextMask/Text").GetComponent<Text>();
        if (Mask == null)
            Mask = UsingStoryLayer.transform.Find("Mask").gameObject;

        WordsText.text = Usingdo.talks[nowindex].talkstring;
        ShowHint();
        EventTriggerListener.Get(Mask).onClick = WaitToClick;
    }

    void WaitToClick(GameObject go)
    {
        DoNextStory(Usingdo);
    }

    void DoNextStory(StateDo _do)
    {
        if (_do.talks.Length - 1 > nowindex)
        {
            EventTriggerListener.Get(Mask).onClick = null;
            nowindex++;
            GameObject hideText = Instantiate(WordsText.gameObject, WordsText.transform.parent);
            WordsText.text = "";
            LeanTween.moveLocalY(hideText, 200, 0.5f).setOnComplete(() =>
            {
                Destroy(hideText);
                ShowStory();
            });
        }
        else
        {
            GameObject hideText = Instantiate(WordsText.gameObject, WordsText.transform.parent);
            WordsText.text = "";
            LeanTween.moveLocalY(hideText, 200, 0.5f).setOnComplete(() =>
            {
                Destroy(UsingStoryLayer);
                resetState();
                CheckAction(_do.NextDo, jumpnum);
            });
            StopAllCharacterEffect();
            StopHideCharacterEffect();
        }
    }

    void ShowHint()
    {
        HintLayer.SetActive(true);
        AniController.Get(HintLayer).AddSprite(hints);
        AniController.Get(HintLayer).PlayAni(4, 7, AniController.AniType.Loop, 5);

        if (Usingdo.talks.Length - 1 == nowindex)
        {
            Mask.SetActive(false);
            islast = true;
        }
    }

    void HideHint()
    {
        HintLayer.SetActive(false);
        AniController.Get(HintLayer).Stop();
    }

    void GetAllCharacters()
    {
        Transform root = transform.parent;
        foreach (Transform child in root)
        {
            if (child.name.Contains("character"))
            {
                CharactersList.Add(child);
            }
        }
    }

    void ShowCharacterEffect(Transform character)
    {
        if (character == null)
            return;

        //找到角色物件
        if (character.childCount != 0)
        {
            character = character.GetChild(0);
        }

        float speed = 0.5f;
        LeanTween.moveLocalY(character.gameObject, -10, speed).setLoopPingPong();           
    }

    void StopCharacterEffect()
    {
        Transform character;
        if (nowindex>=1)
            character = Usingdo.talks[nowindex-1].character;
        else
            character = Usingdo.talks[nowindex].character;

        if (character == null)
            return;
        //找到角色物件
        if (character.childCount != 0)
        {
            character = character.GetChild(0);
        }
        LeanTween.cancel(character.gameObject);
        character.transform.localPosition = new Vector3(0, 0, 0);
    }

    void StopAllCharacterEffect()
    {
        Transform character = Usingdo.talks[nowindex].character;
        if (character == null)
            return;
        //找到角色物件
        if (character.childCount != 0)
        {
            character = character.GetChild(0);
        }
        LeanTween.cancel(character.gameObject);
    }

    void ShowHideCharacterEffect(Transform character)
    {
        if (character == null)
            return;

        foreach (Transform c in CharactersList)
        {
            Transform _character = c;
            //找到角色物件
            if (_character.childCount != 0)
            {
                _character = c.GetChild(0);
            }

            if (c != character)
            {
                Image image = _character.GetComponent<Image>();
                if (image == null) return;

                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.5f);
            }
            else
            {
                Image image = _character.GetComponent<Image>();
                if (image == null) return;

                image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
            }
        }
    }

    void StopHideCharacterEffect()
    {
        foreach (Transform c in CharactersList)
        {
            Transform _character = c;
            //找到角色物件
            if (_character.childCount != 0)
            {
                _character = c.GetChild(0);
            }

            Image image = _character.GetComponent<Image>();
            if (image == null) return;

            image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        }
    }
}

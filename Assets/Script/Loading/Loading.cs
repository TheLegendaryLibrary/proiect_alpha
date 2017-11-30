using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Loading : MonoBehaviour {

    static Loading Instance;

    private WWW www;
    CanvasGroup LoadingLayer;

    Text progressText;
    RectTransform loadingIcon;
    RectTransform loadingBG;

    // Use this for initialization
    void Start () {
        loadingIcon = Instance.transform.Find("icon").GetComponent<RectTransform>();
        loadingBG = Instance.transform.Find("LoadingBG").GetComponent<RectTransform>();
        progressText = Instance.transform.Find("loadingText").GetComponent<Text>();
        LoadingLayer = Instance.transform.GetComponent<CanvasGroup>();
        progressText.text = "loading...";
        LoadingLayer.alpha = 0;
        loadingBG.position = new Vector2(640, 0);
        LeanTween.alphaCanvas(transform.GetComponent<CanvasGroup>(), 1, 0.5f);
        LeanTween.moveX(loadingBG, 0, 0.5f);
    }
	
	// Update is called once per frame
	void Update () {
        if (isLoadingDone)
        {
            progressText.color = Color.yellow;
            progressText.text = "complete!";
        }
        if (!isLoadingDone&& www != null)
        {
            string nowRES= Path.GetFileName(www.url);
            progressText.text = nowRES;  
        }
        loadingIcon.localEulerAngles = new Vector3(0, 0, loadingIcon.localEulerAngles.z + 1);
    }


   public struct StoryResourcePack
    {
        public ChatSystemTool.ChatConfig NowConfig;
        public ChatSystemTool.ChatActionBox NowStroyActionBox;
        public ChatSystemTool.ResourcesBox NowResourcesBox;
    }
    StoryResourcePack storyResPack;
    bool isLoadingDone = false;


    public static Loading GetInstance()
    {
        if (Instance == null)
        {
            GameObject loadingobj = (GameObject)Resources.Load("Prefab/Loading/LoadingLayer");
            loadingobj = Instantiate(loadingobj);
            Instance = loadingobj.transform.Find("Loading").GetComponent<Loading>();

            DontDestroyOnLoad(loadingobj);
        }
        return Instance;
    }

    public void CloseLoading()
    {
        LeanTween.alphaCanvas(transform.GetComponent<CanvasGroup>(), 0, 1f).setOnComplete(()=>
        {
            storyResPack = new StoryResourcePack();
            Destroy(gameObject.transform.parent.gameObject);
        });
    }

    public StoryResourcePack GetstoryResource()
    {
        return storyResPack;
    }

    public void LoadingStoryScene(string storyname,System.Action loadingDone)
    {
        ChatLoader loader = new ChatLoader();

        //初始化
        storyResPack = new StoryResourcePack();
        storyResPack.NowConfig = new ChatSystemTool.ChatConfig();
        storyResPack.NowStroyActionBox = new ChatSystemTool.ChatActionBox();

        storyResPack.NowConfig = loader.LoadNowConfig();
        storyResPack.NowStroyActionBox = loader.LoadStory(storyname, storyResPack.NowConfig);

#if _storyDebug
        StartCoroutine(LoadStroyResourcesOut(()=>
        {
            isLoadingDone = true;
            www = null;
            loadingDone();
        }));
#else
        StartCoroutine(LoadStoryResources(()=>
        {
            LeanTween.delayedCall(0.5f, () =>
            {
                isLoadingDone = true;
                loadingDone();
            });
        }));
#endif


    }


    IEnumerator LoadStoryResources(System.Action loadingDone)
    {
        isLoadingDone = false;
        storyResPack.NowResourcesBox.characterSprites = new Dictionary<string, Dictionary<string, Sprite>>();
        storyResPack.NowResourcesBox.windowsSprites = new Dictionary<string, Sprite[]>();
        storyResPack.NowResourcesBox.bgSprites = new Dictionary<string, Sprite>();
        storyResPack.NowResourcesBox.bgms = new Dictionary<string, AudioClip>();
        storyResPack.NowResourcesBox.voices = new Dictionary<string, AudioClip>();
        Dictionary<string, Sprite> tempResource;
        ResourceRequest rq;

        //查找角色资源
        foreach (KeyValuePair<string, ChatAction.StoryCharacter> character in storyResPack.NowStroyActionBox.CharacterList)
        {
            Sprite[] tempSprite;
            tempResource = new Dictionary<string, Sprite>();
            //读取角色立绘

            tempSprite = Resources.LoadAll<Sprite>("Texture/story/character/" + character.Value.Image);
            //Debug.Log("Texture/story/character/" + character.Value.Image);

            if (tempSprite.Length == 0) Debug.LogError("Can't find " + "Texture/story/character/" + character.Key);
            else
            {
                foreach (Sprite s in tempSprite)
                {
                    tempResource.Add(s.name, s);
                }
                storyResPack.NowResourcesBox.characterSprites.Add(character.Key, tempResource);
            }

            //读取窗口文件
            tempSprite = Resources.LoadAll<Sprite>("Texture/story/board/" + character.Value.Windows);
            if (tempSprite == null || tempSprite.Length == 0) Debug.LogError("Can't find " + "Texture/story/board/" + character.Value.Windows);
            storyResPack.NowResourcesBox.windowsSprites.Add(character.Key, tempSprite);

            //读取声音
            AudioClip tempAudio;

            //tempAudio = Resources.Load<AudioClip>("Sound/" + character.Value.Voice);
            rq = Resources.LoadAsync<AudioClip>("Sound/" + character.Value.Voice);
            yield return rq;
            tempAudio = (AudioClip)rq.asset;

            if (tempAudio == null) Debug.LogError("Can't find " + "Sound/" + character.Value.Voice);
            storyResPack.NowResourcesBox.voices.Add(character.Key, tempAudio);
        }

        //读取背景
        if (storyResPack.NowStroyActionBox.BG != null)
        {
            foreach (string name in storyResPack.NowStroyActionBox.BG)
            {
                Sprite tempSprite;

                //读取背景
                //tempSprite = Resources.Load<Sprite>("Texture/story/bg/" + name);
                rq = Resources.LoadAsync<Sprite>("Texture/story/bg/" + name);
                yield return rq;
                tempSprite = (Sprite)rq.asset;

                //tempSprite = Resources.Load<Sprite>("Texture/story/bg/" + name);
                if (tempSprite == null) Debug.LogError("Can't find " + "Texture/story/bg/" + name);
                storyResPack.NowResourcesBox.bgSprites.Add(name, tempSprite);
            }
        }

        //读取BGM
        if (storyResPack.NowStroyActionBox.BGM != null)
        {
            foreach (string name in storyResPack.NowStroyActionBox.BGM)
            {
                AudioClip tempAudio;

                //读取背景
                //tempAudio = Resources.Load<AudioClip>("Sound/" + name);
                rq = Resources.LoadAsync<AudioClip>("Sound/" + name);
                yield return rq;
                tempAudio = (AudioClip)rq.asset;

                if (tempAudio == null) Debug.LogError("Can't find " + "Sound/" + name);
                storyResPack.NowResourcesBox.bgms.Add(name, tempAudio);
            }
        }
        rq = null;
        loadingDone();
    }


    IEnumerator LoadStroyResourcesOut(System.Action loadFinish)
    {
        isLoadingDone = false;
        storyResPack.NowResourcesBox.characterSprites = new Dictionary<string, Dictionary<string, Sprite>>();
        storyResPack.NowResourcesBox.windowsSprites = new Dictionary<string, Sprite[]>();
        storyResPack.NowResourcesBox.bgSprites = new Dictionary<string, Sprite>();
        storyResPack.NowResourcesBox.bgms = new Dictionary<string, AudioClip>();
        storyResPack.NowResourcesBox.voices = new Dictionary<string, AudioClip>();

        foreach (KeyValuePair<string, ChatAction.StoryCharacter> character in storyResPack.NowStroyActionBox.CharacterList)
        {
            List<string> filePaths = new List<string>();
            string imgtype = "*.BMP|*.JPG|*.GIF|*.PNG";
            string[] ImageType = imgtype.Split('|');
            for (int i = 0; i < ImageType.Length; i++)
            {
                string[] dirs = Directory.GetFiles(GetDataPath("Texture/character/" + character.Value.Image), ImageType[i]);
                for (int j = 0; j < dirs.Length; j++)
                {
                    filePaths.Add(dirs[j]);
                }
            }

            Sprite[] tempSprite = new Sprite[filePaths.Count];
            Dictionary<string, Sprite> tempResource = new Dictionary<string, Sprite>();
            for (int i = 0; i < filePaths.Count; i++)
            {
                www = new WWW("file:///" + filePaths[i]);
                yield return www;
                Texture2D texture = www.texture;
                tempSprite[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                tempSprite[i].name = Path.GetFileNameWithoutExtension(filePaths[i]);
            }
            foreach (Sprite s in tempSprite)
            {
                tempResource.Add(s.name, s);
            }
            storyResPack.NowResourcesBox.characterSprites.Add(character.Key, tempResource);

            //读取窗口文件
            tempSprite = Resources.LoadAll<Sprite>("Texture/story/board/" + character.Value.Windows);
            if (tempSprite == null|| tempSprite.Length == 0) Debug.LogError("Can't find " + "Texture/story/board/" + character.Value.Windows);
            storyResPack.NowResourcesBox.windowsSprites.Add(character.Key, tempSprite);

            //读取声音
            AudioClip tempAudio;

            www = new WWW("file:///" + GetDataPath("Sound/" + character.Value.Voice) + ".wav");
            yield return www;
            //Debug.Log(www.url);
            tempAudio = www.GetAudioClip();

            if (tempAudio == null) Debug.LogError("Can't find " + "Sound/" + character.Value.Voice);
            storyResPack.NowResourcesBox.voices.Add(character.Key, tempAudio);
        }

        //读取背景
        if (storyResPack.NowStroyActionBox.BG != null)
        {
            Sprite tempSprite;
            foreach (string name in storyResPack.NowStroyActionBox.BG)
            {
                Debug.Log("loadingBG: " + name);
                //读取背景

                string bgpath = GetDataPath("Texture/bg/" + name + ".png");
                www = new WWW("file:///" + bgpath);
                yield return www;
                //Debug.Log(www.url);
                Texture2D texture = www.texture;
                tempSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                tempSprite.name = Path.GetFileNameWithoutExtension(bgpath);

                if (tempSprite == null) Debug.LogError("Can't find " + "Texture/story/bg/" + name);
                storyResPack.NowResourcesBox.bgSprites.Add(name, tempSprite);
            }
        }

        //读取BGM
        if (storyResPack.NowStroyActionBox.BGM != null)
        {
            foreach (string name in storyResPack.NowStroyActionBox.BGM)
            {
                AudioClip tempAudio;
                //读取背景

                www = new WWW("file:///" + GetDataPath("Sound/" + name + ".mp3"));
                yield return www;
                //Debug.Log(www.url);
                tempAudio = www.GetAudioClip();


                //tempSprite = Resources.Load<Sprite>("Texture/story/bg/" + name);
                if (tempAudio == null) Debug.LogError("Can't find " + "Sound/" + name);
                storyResPack.NowResourcesBox.bgms.Add(name, tempAudio);
            }
        }

        loadFinish();
    }

    //获取路径//
    string GetDataPath(string path)
    {
        return PathKit.GetResourcesPath() + "StoryResources/" + path;
    }
}

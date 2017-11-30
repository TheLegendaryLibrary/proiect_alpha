using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AniController : MonoBehaviour {

    Sprite[] aniSprite;

    SpriteRenderer currentSprite;
    Image currentUISprite;

    enum SpriteType
    {
        Sprite2D,
        SpriteUI,
    }
    SpriteType type = SpriteType.Sprite2D;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        if (isPlay)
        {
            PlayUpdate();
        }
	}

    static public AniController Get(GameObject go)
    {
        AniController controller = go.GetComponent<AniController>();
        if (controller == null) controller = go.AddComponent<AniController>();
        return controller;
    }
    static public AniController Get(Transform transform)
    {
        AniController controller = transform.GetComponent<AniController>();
        if (controller == null) controller = transform.gameObject.AddComponent<AniController>();
        return controller;
    }

    public void AddSprite(Sprite[] sprite)
    {
        if (currentSprite == null)
        {
            currentSprite = transform.GetComponent<SpriteRenderer>();
            if (currentSprite == null)
            {
                type = SpriteType.SpriteUI;
                currentUISprite = transform.GetComponent<Image>();
            }
        }
        aniSprite = new Sprite[sprite.Length];
        aniSprite = sprite;
    }

    public enum AniType
    {
        Loop,
        LoopBack,
        Once
    }

    float time = 0;
    int currentframe = 0;
    int[] frameLoop = new int[2];
    int[] nextframeLoop = new int[2];

    int anispeed = 10;
    AniType currentType = AniType.Loop;
    bool isPlay = false;
    bool isLoopBack = false;

    //播放动画需要等待当前动画播放完成
    public void PlayAni(int frist,int end, AniType type,int speed)
    {
        if (!isPlay)
        {
            frameLoop[0] = frist;
            frameLoop[1] = end;

            currentframe = frist;
        }
        else
        {
            nextframeLoop[0] = frist;
            nextframeLoop[1] = end;
        }

        currentType = type;
        anispeed = speed;
		
        isPlay = true;
    }

    //播放动画时会被打断
    public void PlayAniCanBreak(int frist, int end, AniType type, int speed)
    {
        frameLoop[0] = frist;
        frameLoop[1] = end;

        currentType = type;
        anispeed = speed;

        currentframe = frist;

        isPlay = true;
    }

    void PlayUpdate()
    {
        //计算限制帧的时间
        time += Time.deltaTime;
        if (currentType == AniType.Loop)
        {
            SetSprite(aniSprite[currentframe]);
            if (time >= 1.0f / anispeed)
            {
                //帧序列切换
                currentframe = currentframe + 1;
                //限制帧清空
                time = 0;
                //超过帧动画总数从第0帧开始
                if (currentframe > frameLoop[1])
                {
                    if (nextframeLoop[0] != 0 && nextframeLoop[1] != 0)
                    {
                        frameLoop[0] = nextframeLoop[0]; nextframeLoop[0] = 0;
                        frameLoop[1] = nextframeLoop[1]; nextframeLoop[1] = 0;
                    }
                    currentframe = frameLoop[0];
                }

            }
        }

        else if (currentType == AniType.LoopBack)
        {
            SetSprite(aniSprite[currentframe]);
            if (time >= 1.0f / anispeed)
            {
                //限制帧清空
                time = 0;

                if (isLoopBack)
                {
                    currentframe = currentframe - 1;
                    //超过帧动画总数从第0帧开始
                    if (currentframe <= frameLoop[0])
                    {
                        isLoopBack = false;
                        if (nextframeLoop[0] != 0 && nextframeLoop[1] != 0)
                        {
                            frameLoop[0] = nextframeLoop[0]; nextframeLoop[0] = 0;
                            frameLoop[1] = nextframeLoop[1]; nextframeLoop[1] = 0;
                        }
                        currentframe = frameLoop[0];
                    }
                }
                else
                {
                    currentframe = currentframe + 1;
                    //超过帧动画总数从第0帧开始
                    if (currentframe >= frameLoop[1])
                    {
                        isLoopBack = true;
                        currentframe = frameLoop[1];
                    }
                }
            }

        }

        else if (currentType == AniType.Once)
        {
            //TODO:
            return;
        }
    }

    void SetSprite(Sprite sp)
    {
        if (type == SpriteType.Sprite2D)
        {
            currentSprite.sprite = sp;
        }
        else if (type == SpriteType.SpriteUI)
        {
            currentUISprite.sprite = sp;
        }
    }

    public void Stop()
    {
        isPlay = false;
    }

    public bool IsPlaying()
    {
        return isPlay;
    }
}

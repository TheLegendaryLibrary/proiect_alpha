using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class SmallNoticeUI : MonoBehaviour {

    private GameObject smallNoticeObject;
    private GameObject smallNoticeList;
    float NoticeDuratrionTime = 2f;
    float ActionTime = 0.25f;

    //初始位置
    Vector3 FirstPosition = new Vector3(0, 0, 0);


	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	}

    public SmallNoticeUI INIT()
    {
        smallNoticeObject = Resources.Load<GameObject>("Prefab/Notice/SmallNotice");
        smallNoticeList = Resources.Load<GameObject>("Prefab/Notice/NoticeList");
        return this;
    }

    public void OpenNotice(string str, float durationtime,Transform t)
    {
        GameObject _notice = Instantiate(smallNoticeObject);
        Text info = _notice.transform.Find("Text").GetComponent<Text>();
        info.text = str;
        int line = Mathf.CeilToInt(info.preferredWidth / Screen.width);
        NoticeDuratrionTime = durationtime;

        //查找UI画布的最顶层
        Canvas c = _notice.GetComponent<Canvas>();
        while (t != null && c == null)
        {
            c = t.gameObject.GetComponent<Canvas>();
            t = t.parent;
        }
        Transform list = c.transform.Find("NoticeList");
        if (list == null)
        {
            list = Instantiate(smallNoticeList).transform;
            list.name = "NoticeList";
            list.transform.SetParent(c.transform, false);
            list.transform.localScale = new Vector3(1, 1, 1);
            list.transform.SetAsLastSibling();
        }

        _notice.transform.SetParent(list.transform, false);
        _notice.transform.localScale = new Vector3(1, 1, 1);
        _notice.transform.SetAsLastSibling();

        RectTransform rect = _notice.transform as RectTransform;

        rect.sizeDelta = new Vector2(0, Screen.height / 15 * line);
        rect.localScale = new Vector3(rect.localScale.x, 0, rect.localScale.z);
        rect.localPosition = FirstPosition;
        ShowNoticeAction(_notice);
        Destroy(this);
    }

    void ShowNoticeAction(GameObject notice)
    {
        LeanTween.scaleY(notice, 1, ActionTime).setOnComplete(
            () => {
                LeanTween.scaleY(notice, 0, ActionTime).setDelay(NoticeDuratrionTime).setDestroyOnComplete(true);
            }
            );
    }


    Transform GetList(Transform t)
    {
        //查找UI画布的最顶层
        Canvas c = t.GetComponent<Canvas>();
        while (t != null && c == null)
        {
            c = t.gameObject.GetComponent<Canvas>();
            t = t.parent;
        }
        Transform list = c.transform.Find("NoticeList");
        if (list == null)
        {
            list = Instantiate(smallNoticeList).transform;
            list.name = "NoticeList";
            list.transform.SetParent(c.transform, false);
            list.transform.SetAsLastSibling();
        }

        return list;
    }

    //设置显示最大的信息数
    public void SetMaxNotice(int count,Transform t)
    {
        Transform list = GetList(t);
        list.GetComponent<SmallNoticeList>().SetshowMax(count);
    }

    //设置初始位置
    public void SetFirstPosition(Vector3 position, Transform t)
    {
        Transform list = GetList(t);
        list.GetComponent<SmallNoticeList>().SetFirstPosition(position);
        FirstPosition = position;
    }

    //设置对齐模式
    public void SetAlignType(SmallNoticeList.Align type, Transform t)
    {
        Transform list = GetList(t);
        list.GetComponent<SmallNoticeList>().SetAligh(type);
    }


}

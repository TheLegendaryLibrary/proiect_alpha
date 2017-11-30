using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SmallNoticeList : MonoBehaviour {

    public float smooting = 8;
    public int showMax = 4;
    int noticeCount = 0;
    ArrayList TargetPosition = new ArrayList();

    //对齐模式
    public enum Align
    {
        UP,
        MID,
        DOWN
    }
    Align AlignType = Align.MID;

    //初始位置
    Vector3 FirstPosition = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start () {
        RectTransform rect = transform.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 0);
        rect.localPosition = new Vector3(0, 0, 0);
        UpdatePositon();
	}

	// Update is called once per frame
	void Update () {
        int newcount = transform.childCount;

        if (newcount != noticeCount)
            UpdatePositon();

        if (TargetPosition != null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                RectTransform _rect = transform.GetChild(i).transform as RectTransform;
                float _move = Mathf.Lerp(_rect.localPosition.y, (float)TargetPosition[TargetPosition.Count - i - 1], Time.deltaTime * smooting);
                _rect.localPosition = new Vector3(_rect.localPosition.x, _move, _rect.localPosition.z);
                
                //float alpha = 1 - (1 / (float)showMax) * (TargetPosition.Count - i - 1);
                //_rect.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                //_rect.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, alpha);
            }
        }

        if (newcount == 0)
            Destroy(this.gameObject);
	}


    void UpdatePositon()
    {
        if (transform.childCount == 0)
            return;

        noticeCount = transform.childCount;
        TargetPosition.Clear();

        float offset = 0;
        RectTransform last_rect = new RectTransform();
        for (int i = transform.childCount-1; i >= 0; i--)
        {
            RectTransform _rect = transform.GetChild(i).transform as RectTransform;
            if (i < transform.childCount - showMax)
            {
                LeanTween.scaleY(_rect.gameObject, 0, 0.25f);
            }

            if (i == transform.childCount - 1)
            {
                Vector3 Position = SetPositonByAlign(AlignType, _rect);
                TargetPosition.Add(Position.y);
                last_rect = _rect;
                offset = Position.y;
            }
            else
            {
                Vector3 Position = new Vector3(_rect.localPosition.x, offset + (last_rect.sizeDelta.y + _rect.sizeDelta.y) / 2, _rect.localPosition.z);
                TargetPosition.Add(Position.y);
                last_rect = _rect;
                offset = Position.y;
            }
        }
    }

    Vector3 SetPositonByAlign(Align type, RectTransform _rect)
    {
        Vector3 Position = new Vector3();
        if (AlignType == Align.UP)
        {
            Position = new Vector3(FirstPosition.x, FirstPosition.y - _rect.sizeDelta.y / 2, FirstPosition.z);
        }
        else if (AlignType == Align.MID)
        {
            Position = new Vector3(FirstPosition.x, FirstPosition.y, FirstPosition.z);
        }
        else if (AlignType == Align.DOWN)
        {
            Position = new Vector3(FirstPosition.x, FirstPosition.y + _rect.sizeDelta.y / 2, FirstPosition.z);
        }

        return Position;
    }


    //设置最大显示条数
    public void SetshowMax(int count)
    {
        showMax = count;
    }

    //设置对齐方式
    public void SetAligh(Align type)
    {
        AlignType = type;
    }

    //设置初始位置
    public void SetFirstPosition(Vector3 position)
    {
        FirstPosition = position;
    }
}

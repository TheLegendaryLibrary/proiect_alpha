using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChatSelectionList : MonoBehaviour {

    public float smooting = 8;
    int noticeCount = 0;
    ArrayList TargetPosition = new ArrayList();

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
                
            }
        }
	}


    void UpdatePositon()
    {
        if (transform.childCount == 0)
            return;

        noticeCount = transform.childCount;
        TargetPosition.Clear();

        float offset = 0;
        float dis = 30f;
        RectTransform last_rect = new RectTransform();
        for (int i = transform.childCount-1; i >= 0; i--)
        {
            RectTransform _rect = transform.GetChild(i).transform as RectTransform;

            if (i == transform.childCount - 1)
            {
                Vector3 Position = new Vector3(FirstPosition.x, FirstPosition.y, FirstPosition.z);
                TargetPosition.Add(Position.y);
                last_rect = _rect;
                offset = Position.y + dis;
            }
            else
            {
                Vector3 Position = new Vector3(_rect.localPosition.x, offset + (last_rect.sizeDelta.y + _rect.sizeDelta.y) / 2, _rect.localPosition.z);
                TargetPosition.Add(Position.y);
                last_rect = _rect;
                offset = Position.y + dis;
            }
        }
    }

    public void ClearSelection(string select,System.Action callback)
    {
        foreach (Transform t in transform)
        {
            if (t.name.CompareTo(select) == 0)
            {
                LeanTween.scaleY(t.gameObject, 0, 0.4f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
                {
                    foreach (Transform tt in transform)
                        Destroy(tt.gameObject);
                    callback();
                });
            }
            else
            {
                LeanTween.scaleY(t.gameObject, 0, 0.25f);
            }
        }
    }
}

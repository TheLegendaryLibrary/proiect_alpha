using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCenter : MonoBehaviour {

    private Vector3 Pos;
    private float speed = 2f;
    private bool isLocal = false;
    System.Action CallBack;

    // Update is called once per frame
    void Update () {
        float _sp = speed * 0.05f;
        Vector2 nowpos = transform.position;
        if (isLocal)
        {
            transform.localPosition = new Vector2(Mathf.Lerp(transform.localPosition.x, Pos.x, _sp), Mathf.Lerp(transform.localPosition.y, Pos.y, _sp));
            nowpos = transform.localPosition;
        }
        else
        {
            transform.position = new Vector2(Mathf.Lerp(transform.position.x, Pos.x, _sp), Mathf.Lerp(transform.position.y, Pos.y, _sp));
            nowpos = transform.position;
        }

        if (Vector2.Distance(nowpos, Pos) <= 0.1)
        {
            if (CallBack != null) CallBack();
            Destroy(this);
        }
	}

    public void SetPos(Vector2 setpos)
    {
        isLocal = false;
        Pos = setpos;
    }

    public void SetLocalPos(Vector2 setpos)
    {
        isLocal = true;
        Pos = setpos;
    }

    public void SetSpeed(float setspeed)
    {
        speed = setspeed;
    }

    public void SetCallback(System.Action action)
    {
        CallBack = action;
    }
}

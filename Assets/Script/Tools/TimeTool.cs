using UnityEngine;
using System.Collections;

public class TimeTool: MonoBehaviour{

    //添加计时操作：静态计时器方便嘛，但是有点乱
    static bool isTiming = false;
    static float startime = 0;

    //定义Callback
    public delegate void VoidDelegate();

    /// <summary>
    /// 开始计时，及时完成后需要调用EndTiming()来获取时间
    /// </summary>
    public static IEnumerator StartTiming()
    {
        startime = 0;
        isTiming = true;
        while (isTiming)
        {
            startime += Time.deltaTime;
            yield return null;
        }
    }

    public static float EndTiming()
    {
        isTiming = false;
        return startime;
    }

    /// <summary>
    /// 设定等待的时间，需确定等待的对象，可以设定callback
    /// </summary>
    public static void SetWaitTime(float time,GameObject obj,VoidDelegate callback)
    {
        if (obj.GetComponent<TimeTool>() == null)
        {
            TimeTool tt = CreatTimeObject(obj);
            tt.StartWait(time, callback);
        }
    }

    //创建计时器
    static TimeTool CreatTimeObject(GameObject obj)
    {
        TimeTool timetool = obj.AddComponent<TimeTool>();

        return timetool;
    }

    //开始计时
    void StartWait(float time, VoidDelegate callback)
    {
        StartCoroutine(TimeIE(time, callback));
    }
    public IEnumerator TimeIE(float time, VoidDelegate callback)
    {
        yield return new WaitForSeconds(time);
        callback();
        TimeTool timetool = gameObject.GetComponent<TimeTool>();
        Destroy(timetool);
    }

}

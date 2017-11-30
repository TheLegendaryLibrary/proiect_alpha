using UnityEngine;
using System.Collections;

public class NoticeTest : MonoBehaviour {

    SmallNoticeUI sNotice;
	// Use this for initialization
	void Start () {
        sNotice = new SmallNoticeUI();
        sNotice = sNotice.INIT();
	}

    public Transform Plan;

	// Update is called once per frame
	void Update () {
	
	}


    public void Test()
    {


        string[] str = {"Test!Test!!Test!!<color=red>Test!!</color>Test!"
                           ,"Test!Test!!Test!!Test!!Test!Test!Test!!Test!!Test!!Test!"
                       ,"Test!Test!!Test!!Test!Test!!Test!!Test!!Test!"};
        sNotice.OpenNotice(str[Random.Range(0,str.Length)], 2f, Plan);
    }
}

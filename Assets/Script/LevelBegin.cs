using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelBegin : MonoBehaviour {

    ArrayList imagelist = new ArrayList();
	// Use this for initialization
	void Start () {
        FindImage(transform);
        //foreach (Transform t in imagelist)
        //{
        //    Debug.Log(t.name);
        //}
        //imagelist = ShuffleList(imagelist);
        Debug.Log(imagelist.Count);
        float dely = 0;
        foreach (Transform t in imagelist)
        {
            t.localScale = new Vector3(1, 0, 1);
            LeanTween.scaleY(t.gameObject, 1, 1f).setEase(LeanTweenType.easeOutBack).setDelay(dely);
            dely += 0.1f;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void FindImage(Transform transform)
    {
        foreach (Transform t in transform)
        {
            if (t.name == "playerLayer" || t.name.Contains("studentLayer") || t.name.Contains("teacherLayer"))
            {
                imagelist.Add(t);
            }
            else if((t.name.CompareTo("UIlayer")==0))
            {
                continue;
            }
            else
            {
                if (t.GetComponent<Image>() != null)
                    imagelist.Add(t);
                if (t.childCount > 0)
                    FindImage(t);
            }
        }
    }

    ArrayList ShuffleList(ArrayList list)
    {
        ArrayList newlist = new ArrayList();
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, list.Count);
            newlist.Add(list[index]);
            list.RemoveAt(index);
        }
        return newlist;
    }
}

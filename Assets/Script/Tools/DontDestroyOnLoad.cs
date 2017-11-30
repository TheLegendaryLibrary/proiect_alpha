using UnityEngine;
using System.Collections;

public class DontDestroyOnLoad : MonoBehaviour {

    //public GameObject Object;
    //public static bool IsClone = false;
    //private GameObject clone;

    // Use this for initialization

    void Start()
    {
        //if (!IsClone)
        //{

        //    clone = Instantiate(Object, transform.position, transform.rotation) as GameObject;

        //    IsClone = true;

        //}
        DontDestroyOnLoad(this.gameObject);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

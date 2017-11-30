using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public static void ReloadNowLevel(GameObject go)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void NextLevel(GameObject go)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public static void NextLevel(GameObject go,string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

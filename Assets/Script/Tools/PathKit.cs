using UnityEngine;
using System.Collections;

public class PathKit{

    static public string GetResourcesPath()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return Application.dataPath + "/Raw/";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            return Application.persistentDataPath + "/";
        }
        else
        {
            return Application.streamingAssetsPath + "/";
        }
    }
}

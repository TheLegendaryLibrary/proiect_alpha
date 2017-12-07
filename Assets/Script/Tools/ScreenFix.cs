using UnityEngine;
using System.Collections;

public class ScreenFix : MonoBehaviour {

    public bool useFix = false;
    public float fixSize = 0.6f;
    void Start()
    {
        if (useFix)
        {
            int width = (int)(640 * fixSize);
            int height = (int)(1136 * fixSize);
            Screen.SetResolution(width, height, false);

            Debug.Log("<b>Fixed Screen:</b>\n   fixwidth:  " + width +"   screenwidth:  " + Screen.width+ "\n   fixheight: " + height + "   screenheight: " + Screen.height + "\n");
        }
        else
        {
            Debug.Log("<b>Not Fix Screen!</b>\n   width:  " + Screen.width + "\n   height: " + Screen.height + "\n");
        }
    }
}

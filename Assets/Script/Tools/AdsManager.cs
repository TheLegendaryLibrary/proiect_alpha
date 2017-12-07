using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS || UNITY_ANDROID
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class AdsManager : MonoBehaviour
{
#if UNITY_IOS
    private string gameId = "1627609";
#elif UNITY_ANDROID
    private string gameId = "1627608";
#endif

    public string placementId1 = "video";
    public string placementId2 = "rewardedVideo";
    public Button m_Button1;
    public Button m_Button2;
    public Text text;

    void Start()
    {
        if (m_Button1) m_Button1.onClick.AddListener(showNormalAds);
        if (m_Button2) m_Button2.onClick.AddListener(ShowAd);

        //---------- ONLY NECESSARY FOR ASSET PACKAGE INTEGRATION: ----------//

        if (Advertisement.isSupported)
        {
            Advertisement.Initialize(gameId, true);
        }

        //-------------------------------------------------------------------//

    }
    void Update()
    {
        if (m_Button1) m_Button1.interactable = Advertisement.IsReady();
        if (m_Button2) m_Button2.interactable = Advertisement.IsReady(placementId2);
    }

    void ShowAd()
    {
        ShowOptions options = new ShowOptions();
        options.resultCallback = HandleShowResult;

        Advertisement.Show(placementId2, options);
    }

    void HandleShowResult(ShowResult result)
    {
        if (result == ShowResult.Finished)
        {
            Debug.Log("观看完成！");
            text.text = "观看完成！";

        }
        else if (result == ShowResult.Skipped)
        {
            Debug.LogWarning("跳过视频！");
            text.text = "跳过视频！";

        }
        else if (result == ShowResult.Failed)
        {
            Debug.LogError("失败！");
            text.text = "失败！";
        }
    }

    public void showNormalAds()
    {
        Advertisement.Show();
    }
}
#endif
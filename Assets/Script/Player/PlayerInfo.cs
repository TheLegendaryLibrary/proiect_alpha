using UnityEngine;
using System;
using System.Collections;

public class PlayerInfo : MonoBehaviour {

    [Serializable]
    public struct Info
    {
        public int Money;   //金钱
        public string Languege;
    }

    static Info playerinfo;

    // Use this for initialization
    void Awake () {
        playerinfo = new Info();
        LoadPlayerInfo();
	}

    //读取角色信息
    void LoadPlayerInfo()
    {
        ArrayList _info = PlayerData.PlayerInfoData.Load();
        if (_info != null)
        {
            playerinfo = (Info)_info[0];
        }
        else
        {
            playerinfo = new Info();
            InitPlayerInfoData();
            PlayerData.PlayerInfoData.Save(playerinfo);
        }
    }

    void InitPlayerInfoData()
    {
        playerinfo.Languege = "zh";
        playerinfo.Money = 0;
    }


    //外部获取角色信息
    static public Info GetPlayerInfo()
    {
        return playerinfo;
    }

    //改变语言
    static public void SetLanguege(string lan)
    {
        playerinfo.Languege = lan;
        PlayerData.PlayerInfoData.Save(playerinfo);
    }

    //改变金钱
    static public void ChangeMoney(int num)
    {
        playerinfo.Money += num;
        PlayerData.PlayerInfoData.Save(playerinfo);
    }
}

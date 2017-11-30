using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerData : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
    public class DataBase
    {
        static public ArrayList datas = new ArrayList();

        /// <summary>
        /// 这个是需要保存的方法模板，不要直接用这个保存，需要使用每个class里面重写的保存方法
        /// </summary>
        /// <param name="name">保存的文件名称</param>
        static public void SaveData(string name)
        {
            try
            {
                IFormatter serializer = new BinaryFormatter();

                string path = PathKit.GetResourcesPath() + name;
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);

                serializer.Serialize(fs, datas);
                fs.Close();
                Debug.Log("Save!!   " + path);
            }
            catch (IOException e)
            {
                Debug.Log(e.ToString());
                return;
            }
        }
    }

    //玩家数据
    public class PlayerInfoData : DataBase
    {

        static public void Save(PlayerInfo.Info info)
        {
            datas = new ArrayList();
            datas.Add(info);

            SaveData("PlayerInfoData.sav");
        }

        static public ArrayList Load()
        {
            string name = "PlayerInfoData.sav";
            try
            {
                IFormatter serializer = new BinaryFormatter();
                string path = PathKit.GetResourcesPath() + name;
                if (File.Exists(path))
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                    datas = serializer.Deserialize(fs) as ArrayList;
                    fs.Close();

                    Debug.Log("loaded playerinfo");
                    return datas;
                }
                else
                {
                    Debug.Log("找不到角色存档!");
                    return null;
                }
            }
            catch (IOException e)
            {
                Debug.Log(e.ToString());
                return null;
            }
        }

    }


    //事件类
    public class EventData
    {
 
    }
}

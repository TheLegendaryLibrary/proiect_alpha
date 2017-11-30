using UnityEngine;
using System.Collections;
using System.Xml;
using System.Text;
using System;

public class XmlTool
{
    //读取文本配置表
    public ArrayList loadChatConfigXmlToArray()
    {
        //保存路径
        string filepath = "Config/Story/ChatConfig";

        string _result = Resources.Load(filepath).ToString();

        ArrayList ChatConfig = new ArrayList();

        XmlDocument xmlDoc = new XmlDocument();

        xmlDoc.LoadXml(_result);

        XmlNodeList nodeList = xmlDoc.SelectSingleNode("ChatConfig").ChildNodes;

        foreach (XmlElement config in nodeList)
        {
            ChatSystemTool.ChatConfig _chatconfig = new ChatSystemTool.ChatConfig();

            //读取node内属性，把string转化为对应的属性
            if (config.GetAttribute("Languege") != "")
                _chatconfig.Languege = config.GetAttribute("Languege");
            if (config.GetAttribute("Speed") != "")
                _chatconfig.speed = float.Parse(config.GetAttribute("Speed"));
            if (config.GetAttribute("ShowNameBoard") != "")
            {
                if (config.GetAttribute("ShowNameBoard").CompareTo("true") == 0)
                    _chatconfig.showname = true;
                else
                    _chatconfig.showname = false;
            }      
            //添加进itemList中
            ChatConfig.Add(_chatconfig);
        }
        return ChatConfig;
    }
  
    //获取路径//
    private static string GetDataPath()
    {
        return Application.dataPath + "/Resources";
    }
}
using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Text;
using System;

public class TxtTool{

    public string[] ReadFile(string languege, string name)
    {
        string filepath = "Config/Story/" + languege + "/" + name;
        string tempstr = Resources.Load(filepath).ToString();

        string str1 = System.Text.RegularExpressions.Regex.Unescape(tempstr);
        str1 = str1.Replace("\r", "");
        string[] textflie = System.Text.RegularExpressions.Regex.Split(str1, "\n");
        return textflie;
    }
	
}

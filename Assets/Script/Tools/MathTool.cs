using UnityEngine;
using System.Collections;

public class MathTool {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void SelectionSortAscendingByProperty(ArrayList arr)
    {
        for (int i = 0; i < arr.Count - 1; i++)
        {
            //int min = i;
            //for (int j = i + 1; j < arr.Count; j++)
            //{
            //    if (arr[j] < arr[min])
            //    {
            //        min = j;
            //    }
            //}
            //if (min != i)
            //{
            //    int temp = arr[i];
            //    arr[i] = arr[min];
            //    arr[min] = temp;
            //}
        }
    }

    //判断是字符串是否是数字
    public static bool isNumber(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (char.IsNumber(str, i))
                continue;
            else
                return false;
        }
        return true;
    }


}

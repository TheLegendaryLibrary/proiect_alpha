using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemElement : ElementBase, IPointerClickHandler
{

    [Header("道具名称：")]
    public string Name = "";
    [Header("道具描述：")]
    [Multiline(5)]
    public string Des = "";
    [Header("获得后显示描述面板：")]
    public bool showInfo = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerClick(PointerEventData data)
    {
        GetLevelManager().AddItemInBagUI(data.pointerPress);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ItemDragEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler
{
    private Vector2 originalLocalPointerPosition;
    private Vector2 returnPosition;
    private Vector3 originalPanelLocalPosition;
    private RectTransform dragObject;
    private RectTransform dragArea;
    private CanvasGroup group;

    public bool ToCenter = true;

    void Awake()
    {
        IntiElement();
    }

    //初始化当前状态
    void IntiElement()
    {
        dragObject = transform as RectTransform;
        dragArea = transform.Find("/GameCanvas/bgLayer") as RectTransform;
        returnPosition = dragObject.localPosition;
        //创建画布组
        group = transform.GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
    }


    public void OnPointerDown(PointerEventData data)
    {
        //RemoveMoveCenterEffect();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, data.position, data.pressEventCamera, out originalLocalPointerPosition);
        if (ToCenter)
        {
            Vector2 centerpos = new Vector2();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragObject.parent as RectTransform, data.position, data.pressEventCamera, out centerpos);
            AddPointMoveCenterEffect(centerpos);
            originalPanelLocalPosition = centerpos;
        }
        else
            originalPanelLocalPosition = dragObject.localPosition;
    }

    public void OnPointerUp(PointerEventData data)
    {
        group.blocksRaycasts = true;
        AddretrunEffect();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RemoveMoveCenterEffect();
        group.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData data)
    {
        if (dragObject == null || dragArea == null)
        {
            Debug.Log("找不到拖动物体，或者拖动区域！");
            return;
        }

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, data.position, data.pressEventCamera, out localPointerPosition))
        {
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
            dragObject.localPosition = originalPanelLocalPosition + offsetToOriginal;
        }
    }

    void AddPointMoveCenterEffect(Vector2 centerpos)
    {
        MoveToCenter moveeffect = GetMoveToCenter();
        moveeffect.SetSpeed(6.0f);
        moveeffect.SetLocalPos(centerpos);
    }

    void AddretrunEffect()
    {
        MoveToCenter moveeffect = GetMoveToCenter();
        moveeffect.SetLocalPos(returnPosition);
        moveeffect.SetSpeed(3f);
    }

    void RemoveMoveCenterEffect()
    {
        MoveToCenter movescript = GetComponent<MoveToCenter>();
        if (movescript != null) Destroy(movescript);
    }

    MoveToCenter GetMoveToCenter()
    {
        MoveToCenter moveeffect = dragObject.gameObject.GetComponent<MoveToCenter>();
        if(moveeffect==null)
            moveeffect = dragObject.gameObject.AddComponent<MoveToCenter>();
        return moveeffect;
    }

}

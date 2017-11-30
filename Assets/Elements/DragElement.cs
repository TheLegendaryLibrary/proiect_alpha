using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DragElement : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 originalLocalPointerPosition;
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
        dragArea = dragObject.parent as RectTransform;

        //创建画布组
        group = transform.GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
    }


    public void OnPointerDown(PointerEventData data)
    {
        RemoveMoveCenterEffect();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, data.position, data.pressEventCamera, out originalLocalPointerPosition);
        if (ToCenter)
        {
            AddMoveCenterEffect();
        }
        else
            originalPanelLocalPosition = dragObject.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RemoveMoveCenterEffect();
        group.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts = true;
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

        ClampToWindow();
    }

    // Clamp panel to area of parent
    void ClampToWindow()
    {
        Vector3 pos = dragObject.localPosition;

        Vector3 minPosition = dragArea.rect.min - dragObject.rect.min;
        Vector3 maxPosition = dragArea.rect.max - dragObject.rect.max;

        pos.x = Mathf.Clamp(dragObject.localPosition.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(dragObject.localPosition.y, minPosition.y, maxPosition.y);

        dragObject.localPosition = pos;
    }

    void AddMoveCenterEffect()
    {
        MoveToCenter moveeffect = GetMoveToCenter(dragObject.gameObject);
        moveeffect.SetSpeed(6.0f);
        moveeffect.SetLocalPos(originalLocalPointerPosition);
        originalPanelLocalPosition = originalLocalPointerPosition;
    }

    void RemoveMoveCenterEffect()
    {
        MoveToCenter movescript = GetComponent<MoveToCenter>();
        if (movescript != null) Destroy(movescript);
    }

    MoveToCenter GetMoveToCenter(GameObject dropObject)
    {
        MoveToCenter moveeffect = dropObject.gameObject.GetComponent<MoveToCenter>();
        if (moveeffect == null)
            moveeffect = dropObject.gameObject.AddComponent<MoveToCenter>();
        return moveeffect;
    }
}

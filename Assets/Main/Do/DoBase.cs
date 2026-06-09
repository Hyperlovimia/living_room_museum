using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoBase : MonoBehaviour, IXrSelectable
{
    public void Select(XrSelectContext context)
    {
        OnSelected(context);
    }

    protected virtual void OnSelected(XrSelectContext context)
    {
    }

    protected virtual void OnMouseDown()
    {
        if (IsPointerOverUI())
        {
            return;
        }

        Select(XrSelectContext.MouseFallback(gameObject));
    }

    protected bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        //创建一个点击事件
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        //向点击位置发射一条射线，检测是否点击UI
        EventSystem.current.RaycastAll(eventData, raycastResults);
        if (raycastResults.Count > 0)
            return true;
        else
            return false;
    }
}

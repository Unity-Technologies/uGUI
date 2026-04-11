using System;
using UnityEngine;
using UnityEngine.EventSystems;

internal class DragCallbackCheck : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler
{
    private bool loggedOnDrag = false;
    [NonSerialized] public bool onBeginDragCalled = false;
    [NonSerialized] public bool onDragCalled = false;
    [NonSerialized] public bool onEndDragCalled = false;
    [NonSerialized] public bool onDropCalled = false;
    [NonSerialized] public int? pointerId = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (pointerId.HasValue && eventData.pointerId != pointerId)
            return;

        onBeginDragCalled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pointerId.HasValue && eventData.pointerId != pointerId)
            return;

        if (loggedOnDrag)
            return;

        loggedOnDrag = true;
        onDragCalled = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (pointerId.HasValue && eventData.pointerId != pointerId)
            return;

        onEndDragCalled = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (pointerId.HasValue && eventData.pointerId != pointerId)
            return;

        onDropCalled = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Empty to ensure we get the drop if we have a pointer handle as well.
    }
}

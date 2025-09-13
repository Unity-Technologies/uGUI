using UnityEngine;
using UnityEngine.EventSystems;

internal class DragCallbackCheck : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler
{
    private bool loggedOnDrag = false;
    public bool onBeginDragCalled = false;
    public bool onDragCalled = false;
    public bool onEndDragCalled = false;
    public bool onDropCalled = false;
    public int? pointerId = null;

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

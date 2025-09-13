using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
internal class PointerClickCallbackCheck : MonoBehaviour, IPointerDownHandler
{
    public bool pointerDown = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
    }
}

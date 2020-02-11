using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseInOut : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool mouseEntered = false;

    void Start() {
        HoverTextCS.hideHoverStatic();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        mouseEntered = true;
        HoverTextCS.showHoverStatic("blah");
    }

    public void OnPointerExit(PointerEventData eventData) {
        mouseEntered = false;
        HoverTextCS.hideHoverStatic();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    [SerializeField] private GameObject thing;
    private bool isOpen = false;
    private float prevX;
    void Start() {
        prevX = thing.transform.localPosition.x;
    }
    public void OpenThing() {
        if (isOpen) {
            LeanTween.moveLocalX(thing, prevX, 1);
        } else {
            LeanTween.moveLocalX(thing, prevX-140.27f, 1);
        }
        isOpen = !isOpen;
    }
}

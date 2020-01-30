using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticBag : MonoBehaviour
{
    void OnMouseDown() {
        LeanTween.scale(this.gameObject, new Vector3(3,3,0), 1.0f);
        LeanTween.scale(this.gameObject, new Vector3(1,1,0), 1.0f);
        Destroy(this.gameObject, 0.5f);
    }
}

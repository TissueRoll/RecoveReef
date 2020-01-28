using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticBag : MonoBehaviour
{
    void OnMouseDown() {
        LeanTween.scale(this.gameObject, new Vector3(2,2,0), 0.3f);
        Destroy(this.gameObject, 0.5f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticBag : MonoBehaviour
{
    void Start() {
        Vector2 randomPositionOnScreen = Camera.main.ViewportToWorldPoint(new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        Vector3 pos = new Vector3(randomPositionOnScreen.x, randomPositionOnScreen.y+10, 0);
        this.gameObject.transform.position = pos;
        LTSeq seq = LeanTween.sequence();
        for (int i = 0; i < 10; i++) {
            seq.append(LeanTween.move(this.gameObject, pos+new Vector3((-1*(i%2))*2*(i > 0 && i < 9 ? 2 : 1), -1*i, 0), 0.45f));
        }
    }
    void OnMouseDown() {
        GameManager.instance.adjustTotalPlasticBags(-1);
        Vector3 tmp = this.gameObject.GetComponent<RectTransform>().localScale;
        LeanTween.scale(this.gameObject, tmp*2, 0.3f).setLoopPingPong(1);
        Destroy(this.gameObject, 0.65f);
    }
}

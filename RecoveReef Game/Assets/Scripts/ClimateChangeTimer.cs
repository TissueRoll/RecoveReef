using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimateChangeTimer : MonoBehaviour
{
    [SerializeField] private Sprite climateChangeNotYet;
    [SerializeField] private Sprite climateChangeHasWarned;
    [SerializeField] private Sprite climateChangeHasHappened;
    private UnityEngine.UI.Image image;

    void Awake() {
        image = this.GetComponent<UnityEngine.UI.Image>();
    }

    public void climateChangeChill () {
        image.sprite = climateChangeNotYet;
    }

    public void climateChangeIsWarn () {
        image.sprite = climateChangeHasWarned;
    }

    public void climateChangeIsHappen () {
        image.sprite = climateChangeHasHappened;
    }
}

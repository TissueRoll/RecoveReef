using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverTextCS : MonoBehaviour
{
    private static HoverTextCS instance;
    [SerializeField] private Camera uiCamera;

    private TMPro.TextMeshProUGUI hText;
    private RectTransform bgRT;

    void Awake () {
        instance = this;
        bgRT = transform.Find("HoverImage").GetComponent<RectTransform>();
        hText = transform.Find("HoverText").GetComponent<TMPro.TextMeshProUGUI>();
    }

    void Update () {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
        transform.localPosition = localPoint;
    }

    private void ShowHover(string hoverString) {
        gameObject.SetActive(true);

        hText.text = hoverString;
        float textPaddingSize = 4f;
        Vector2 bgSize = new Vector2(hText.preferredWidth + textPaddingSize*2f, hText.preferredHeight + textPaddingSize * 2f);
        bgRT.sizeDelta = bgSize;
    }

    private void HideHover() {
        gameObject.SetActive(false);
    }

    public static void showHoverStatic (string hoverString) {
        instance.ShowHover(hoverString);
    }

    public static void hideHoverStatic () {
        instance.HideHover();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
    public static bool PopupOpen = false;

    public GameObject popupCanvas;
    public GameObject outText;
    public GameObject popupTitle;
    public UnityEngine.UI.Image popupSprite;

    void Update() {
        if (!GameEnd.gameHasEnded && !PauseScript.GamePaused && Input.GetKeyDown(KeyCode.Return)) {
            ClosePopup();
        }
    }

    public void SetPopupSprite(Sprite sprite) {
        popupSprite.sprite = sprite;
    }

    public void SetPopupTitle(string e) {
        popupTitle.GetComponent<TMPro.TextMeshProUGUI>().text = e;
    }

    public void SetPopupMessage(string e) {
        outText.GetComponent<TMPro.TextMeshProUGUI>().text = e;
    }

    public void OpenPopup() {
        popupCanvas.SetActive(true);
        Time.timeScale = 0f;
        PopupOpen = true;
    }

    public void ClosePopup() {
        popupCanvas.SetActive(false);
        Time.timeScale = 1f;
        PopupOpen = false;
    }
}

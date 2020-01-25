using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
    public static bool PopupOpen = false;

    public GameObject popupCanvas;
    public GameObject outText;

    void Update() {
        if (!PauseScript.GamePaused && Input.GetKeyDown(KeyCode.Return)) {
            ClosePopup();
        }
    }

    public void SetPopupMessage(string e) {
        outText.GetComponent<TMPro.TextMeshProUGUI>().text = e;
    }

    public void OpenPopup() {
        popupCanvas.SetActive(true);
        Time.timeScale = 0f;
        PopupOpen = true;
    }

    void ClosePopup() {
        popupCanvas.SetActive(false);
        Time.timeScale = 1f;
        PopupOpen = false;
    }
}

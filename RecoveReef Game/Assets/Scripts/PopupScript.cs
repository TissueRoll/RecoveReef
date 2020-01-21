using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
    public static bool PopupOpen = false;

    public GameObject popupPanel;
    public GameObject outText;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            ClosePopup();
        }
    }

    public void SetPopupMessage(string e) {
        outText.GetComponent<TMPro.TextMeshProUGUI>().text = e;
    }

    public void OpenPopup() {
        popupPanel.SetActive(true);
        Time.timeScale = 0f;
        PopupOpen = true;
    }

    void ClosePopup() {
        popupPanel.SetActive(false);
        Time.timeScale = 1f;
        PopupOpen = false;
    }
}

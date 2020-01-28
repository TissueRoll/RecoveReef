using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    public static bool gameHasEnded = false;
    public GameObject gameEndCanvas;

    public void gameEndReached () {
        gameHasEnded = true;
        gameEndCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void endMessage(string s) {
        gameEndCanvas.transform.Find("Panel/Image/Message").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = s;
    }
}

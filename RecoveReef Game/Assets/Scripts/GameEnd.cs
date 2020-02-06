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

    public void setCongrats(Sprite wordArt) {
        gameEndCanvas.transform.Find("Panel/ScreenOrganizer/ResultGreeting").gameObject.GetComponent<UnityEngine.UI.Image>().sprite = wordArt;
    }

    public void finalStatistics(int fishIncome, string timeLeft) {
        gameEndCanvas.transform.Find("Panel/ScreenOrganizer/Texts/StatText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Fish Income: " + fishIncome + "\nTime Left: " + timeLeft;
    }

    public void endMessage(string s) {
        gameEndCanvas.transform.Find("Panel/ScreenOrganizer/Texts/FlavorText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = s;
    }
}

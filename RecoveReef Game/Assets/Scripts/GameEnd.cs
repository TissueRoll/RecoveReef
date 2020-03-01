using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void resetEndScreen() {
        gameHasEnded = false;
        gameEndCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    public void goToMainMenu() {
        resetEndScreen();
        SceneManager.LoadScene("MainMenu");
    }

    public void restartGame() {
        resetEndScreen();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void quitGame() {
        print("game has been quit");
        Application.Quit();
    }
}

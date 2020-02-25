using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void getNewGame() {
        // build index method
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.LoadScene("Test");
    }

    public void getTrained() {
        SceneManager.LoadScene("TestCopy");
    }

    public void getExit() {
        print("Game has been quit.");
        Application.Quit();
    }
}

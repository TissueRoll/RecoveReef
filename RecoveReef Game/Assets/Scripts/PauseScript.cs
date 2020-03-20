using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public static bool GamePaused = false;
    private bool SettingsActive = false;

    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    // Update is called once per frame
    void Update()
    {
        if (!GameEnd.gameHasEnded && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))) {
            if (GamePaused && SettingsActive) {
                SettingsOFF();
            } else if (GamePaused && !SettingsActive) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    public void SettingsON() {
        settingsMenuUI.SetActive(true);
        SettingsActive = true;
    }

    void SettingsOFF() {
        settingsMenuUI.SetActive(false);
        SettingsActive = false;
    }

    public void Resume() {
        SettingsOFF();
        pauseMenuUI.SetActive(false);
        if (!PopupScript.PopupOpen)
            Time.timeScale = 1f;
        GamePaused = false;
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GamePaused = true;
    }

    public void LoadMainMenu () {
        SettingsOFF();
        Time.timeScale = 1f;
        GamePaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}

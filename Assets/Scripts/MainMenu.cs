using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TransitionController transitionController;
    public SettingsPanel settingsPanel;

    public void StartGame()
    {
        StartCoroutine(StartGameWithFade());
    }

    private IEnumerator StartGameWithFade()
    {
        //1 Sekunde Fade to Black
        yield return transitionController.FadeIn(1f);
        
        //Scene laden
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        settingsPanel.OpenSettings();
    }
}
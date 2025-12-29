using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TransitionController transitionController;

    public void StartGame()
    {
        StartCoroutine(StartGameWithFade());
    }

    private IEnumerator StartGameWithFade()
    {
        // 1 Sekunde Fade to Black
        yield return transitionController.FadeIn(1f);
        
        // Scene laden
        SceneManager.LoadScene("Game");
        
        // 1 Sekunde Fade from Black (passiert automatisch im GameManager)
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        Debug.Log("Settings in Bearbeitung");
    }
}
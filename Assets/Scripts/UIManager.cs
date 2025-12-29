using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Text loopText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public TransitionController transitionController;


    public void SetLoopText(int loopIndex)
    {
        loopText.text = "Loop: " + loopIndex;
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);

        //Cursor freigeben und sichtbar machen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        StartCoroutine(RestartGameWithFade());
    }

    private IEnumerator RestartGameWithFade()
    {
        yield return transitionController.FadeIn(1f);
        Time.timeScale = 1f; //Spiel läuft wieder
        
        //Cursor wieder locken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.Instance.StartNewGame(); //neues Game starten
        gameOverPanel.SetActive(false); //Overlay deaktivieren
        pausePanel.SetActive(false);
        GameManager.Instance.StartNewGame();
    
        // Kurze Pause damit alles spawnt
        yield return new WaitForSeconds(0.1f);
        
        // MANUELL Fade Out triggern
        yield return transitionController.FadeOut(1f);
    }

    //zum Hauptmenü
    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuWithFade());
    }

    private IEnumerator LoadMainMenuWithFade()
    {
        // Fade to Black
        yield return transitionController.FadeIn(1f);
        Time.timeScale = 1f;
        // Cursor freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Scene laden
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowPause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePause()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ResumeGame()
    {
        HidePause();
    }
}

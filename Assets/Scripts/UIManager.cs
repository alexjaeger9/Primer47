using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text loopText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

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
        Time.timeScale = 1f; //Spiel läuft wieder
        
        //Cursor wieder locken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.Instance.StartNewGame(); //neues Game starten
        gameOverPanel.SetActive(false); //Overlay deaktivieren
        pausePanel.SetActive(false);
    }

    //zum Hauptmenü
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; 
        // TODO: Später Scene laden
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

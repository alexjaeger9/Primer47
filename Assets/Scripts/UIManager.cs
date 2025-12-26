using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text loopText;
    public GameObject gameOverPanel;

    public void SetLoopText(int loopIndex)
    {
        loopText.text = "Loop: " + loopIndex;
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; //Spiel läuft wieder
        GameManager.Instance.StartNewGame(); //neues Game starten
        gameOverPanel.SetActive(false); //Overlay deaktivieren
    }

    //zum Hauptmenü
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; 
        // TODO: Später Scene laden
    }
}

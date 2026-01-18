using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public TransitionController transitionController;
    public SettingsPanel settingsPanel;

    public Text highScoreText;
    public Text lastScoreText;

     private void Start()
    {
        displayScores();
    }

    private void displayScores()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
      
        highScoreText.text = "High Score: " + highScore;
        lastScoreText.text = "Last Score: " + lastScore;
    }

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
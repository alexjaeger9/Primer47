using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject hud;
    public TransitionController transitionController;

    //HUD Elemente
    public Text scoreText;
    public Text currentLoopText; //kleine oben links
    public Text timerText;
    public Text bigLoopText; //große in der Mitte
    public Text lastScoreText;
    public Text highScoreText;
    public Text remainingGhostText;

    //Hud Updates
     public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void UpdateLoopCounter(int loopIndex)
    {
        currentLoopText.text = "Loop: " + loopIndex;
    }

    public void UpdateTimer(float timeRemaining)
    {
        timeRemaining = Mathf.Max(0, timeRemaining);
        timerText.text = timeRemaining.ToString("F2") + "s";
    }

    public void UpdateGhostsRemaining(int remaining)
    {
        remainingGhostText.text = "Ghosts Remaining: " + remaining;
    }

    //großer Loop text
    public void ShowBigLoopText(int loopIndex)
    {
        bigLoopText.text = "LOOP " + loopIndex;
        bigLoopText.gameObject.SetActive(true);
    }

    public void HideBigLoopText()
    {
        bigLoopText.gameObject.SetActive(false);
    }

    //Game over
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        hud.SetActive(false);

        int currentScore = PlayerPrefs.GetInt("LastScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        lastScoreText.text = "Score: " + currentScore;
        highScoreText.text = "High Score: " + highScore;

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
        
        //Panels deaktivieren
        gameOverPanel.SetActive(false); 
        pausePanel.SetActive(false);

        hud.SetActive(true);

        Time.timeScale = 1f; //Spiel läuft wieder
        PauseManager.isGameOver = false;

        yield return GameManager.Instance.StartNewGame();
    }


    //main menu methode
    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuWithFade());
    }

    private IEnumerator LoadMainMenuWithFade()
    {
        //Fade to Black
        yield return transitionController.FadeIn(1f);
        
        //Cursor freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        //Scene laden
        SceneManager.LoadScene("MainMenu");
    }
}

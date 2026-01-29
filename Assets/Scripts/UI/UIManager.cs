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
    public GameObject scoreCalculation;
    public Text sC_currentScore;
    public Text sC_timeLeft;
    public Text sC_newScore;
    public Text sC_coinScore;
    public GameObject[] coinTexts;
    public Text coinsCounter;

    //Hud Updates
     public void UpdateScore(float score)
    {
        scoreText.text = "Score: " + score.ToString("F2");
    }

    public void UpdateLoopCounter(int loopIndex)
    {
        currentLoopText.text = "Loop: " + loopIndex;
    }

    public void UpdateCoinCounter(int coins)
    {
        coinsCounter.gameObject.SetActive(coins > 0);
        if (coins > 0) coinsCounter.text = "Coins: " + coins;
    }

    public void UpdateTimer(float timeRemaining)
    {
        timeRemaining = Mathf.Max(0, timeRemaining);
        timerText.text = timeRemaining.ToString("F2") + "s";
    }

    public void UpdateGhostsRemaining(int remaining)
    {
        remainingGhostText.text = "Targets left: " + remaining;
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

        float currentScore = PlayerPrefs.GetFloat("LastScore", 0f);
        float highScore = PlayerPrefs.GetFloat("HighScore", 0f);
        lastScoreText.text = "Your Score: " + currentScore.ToString("F2");
        highScoreText.text = "High Score: " + highScore.ToString("F2");

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

        Time.timeScale = 1f;

        //Cursor freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        //Scene laden
        SceneManager.LoadScene("MainMenu");
    }

    public void showScoreScalculation(float currentScore, float timeLeft, float newScore, int coins, float coinValue)
    {
        sC_currentScore.text = currentScore.ToString("F2");
        sC_timeLeft.text = timeLeft.ToString("F2");
        sC_newScore.text = newScore.ToString("F2");
        scoreCalculation.SetActive(true);
        if (coins > 0)
        {
            sC_coinScore.text = (coins * coinValue).ToString();
        }
        foreach (GameObject coinText in coinTexts)
        {
            coinText.SetActive(coins > 0);
        }
        
    }

    public void hideScoreCalculation() 
    {
        scoreCalculation.SetActive(false);
    }

}

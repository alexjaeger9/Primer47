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

    [Header("HUD Elemente")]
    public Text timerText;
    public Text bigLoopText;
    public Text lastScoreText;
    public Text remainingGhostText;
    
    [Header("Score Calculation Logic")]
    public GameObject scoreCalculation;
    
    //Texte für die Werte
    public Text sC_currentScore;
    public Text sC_timeLeft; 
    public Text sC_newScore;
    public Text sC_coinScore;

    //Gruppen für die Animation
    public GameObject sC_timeLeft_Group; 
    public GameObject sC_newScore_Group; 
    public GameObject coinBonusGroup;    

    [Header("Audio")]
    public AudioClip loopTransitionSound;   
    public AudioClip scoreCalculationSound;

    private void Start()
    {
        if (coinBonusGroup != null) coinBonusGroup.SetActive(false);
    }

    public void hideScoreCalculation() 
    {
        scoreCalculation.SetActive(false);
        if (coinBonusGroup != null) coinBonusGroup.SetActive(false);
    }

    public void UpdateTimer(float timeRemaining)
    {
        timeRemaining = Mathf.Max(0, timeRemaining);
        timerText.text = timeRemaining.ToString("F2") + "s";
        
        HUDAnimationController hudAnim = FindFirstObjectByType<HUDAnimationController>();
        if(hudAnim == null) return;

        if (timeRemaining <= 5f && timeRemaining > 0f) hudAnim.SetTimerColor(Color.red);
        else if (timeRemaining <= 15f && timeRemaining > 10f) hudAnim.SetTimerColor(Color.yellow);
        else if (timeRemaining > 15f) hudAnim.SetTimerColor(Color.white);
        
        if (timeRemaining <= 5f && timeRemaining > 0f)
        {
            float fractional = timeRemaining - Mathf.Floor(timeRemaining);
            if (fractional > 0.98f) hudAnim.Shake("Timer");
        }
        if (timeRemaining == 0f) hudAnim.BigShake("Timer");
    }

    public void UpdateGhostsRemaining(int remaining) { remainingGhostText.text = "targets left: " + remaining; }

    public void ShowBigLoopText(int loopIndex)
    {
        bigLoopText.text = "Loop " + loopIndex;
        bigLoopText.gameObject.SetActive(true);
    }
    public void HideBigLoopText() { bigLoopText.gameObject.SetActive(false); }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        hud.SetActive(false);
        float currentScore = PlayerPrefs.GetFloat("LastScore", 0f);
        lastScoreText.text = "Your Score: " + currentScore.ToString("F0");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame() { StartCoroutine(RestartGameWithFade()); }
    private IEnumerator RestartGameWithFade()
    {
        yield return transitionController.FadeIn(1f);
        gameOverPanel.SetActive(false); 
        pausePanel.SetActive(false);
        hud.SetActive(true);
        Time.timeScale = 1f; 
        PauseManager.isGameOver = false;
        yield return GameManager.Instance.StartNewGame();
    }

    public void LoadMainMenu() { StartCoroutine(LoadMainMenuWithFade()); }
    private IEnumerator LoadMainMenuWithFade()
    {
        yield return transitionController.FadeIn(1f);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void showScoreScalculation(float currentScore, float timeLeft, float newScore, int coins, float coinValue)
    {
        sC_currentScore.text = currentScore.ToString("F0");
        sC_timeLeft.text = "+ " + timeLeft.ToString("F0");
        sC_newScore.text = newScore.ToString("F0");
        
        if (coins > 0)
        {
            sC_coinScore.text = "+ " + (coins * coinValue).ToString("F0");
        }
            
        scoreCalculation.SetActive(true);
        ResetScoreCalculationElements(coins > 0);
        
        StartCoroutine(ScoreCalculationZoomIn(coins > 0));
    }

    private void ResetScoreCalculationElements(bool hasCoins)
    {
        SetGroupAlpha(sC_timeLeft_Group, 0f);
        SetGroupAlpha(coinBonusGroup, 0f);
        if (coinBonusGroup != null) coinBonusGroup.SetActive(false); 
        SetGroupAlpha(sC_newScore_Group, 0f);
    }

    private void SetGroupAlpha(GameObject group, float alpha)
    {
        if (group == null) return;
        group.transform.localScale = Vector3.one;
        Text[] texts = group.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            Color c = t.color;
            c.a = alpha;
            t.color = c;
        }
    }

    private IEnumerator ScoreCalculationZoomIn(bool hasCoins)
    {
        yield return new WaitForSecondsRealtime(0.2f);

        //Time Left
        PlaySoundWithPitch(scoreCalculationSound, 1.0f); // Normaler Pitch
        yield return StartCoroutine(ZoomInEffect(sC_timeLeft_Group.transform, 4f, 1f, 0.4f));
        yield return new WaitForSecondsRealtime(0.2f);


        //Coin Bonus (falls aktiv)
        coinBonusGroup.SetActive(true);
        PlaySoundWithPitch(scoreCalculationSound, 1.1f); // Etwas höherer Pitch
        yield return StartCoroutine(ZoomInEffect(coinBonusGroup.transform, 3f, 1f, 0.4f));
        yield return new WaitForSecondsRealtime(0.2f);

        
        //New Score

        PlaySoundWithPitch(scoreCalculationSound, 1.2f); // Noch höherer Pitch (Finale!)
        yield return StartCoroutine(ZoomInEffect(sC_newScore_Group.transform, 5f, 1.2f, 0.5f));
    }

    public IEnumerator AnimateLoopNumber(int fromLoop, int toLoop)
    {
        //UI zeigt +1
        bigLoopText.text = "Loop " + (fromLoop + 1);

        for (int i = fromLoop; i < toLoop; i++)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            int nextLoop = i + 1;
            bigLoopText.text = "Loop " + (nextLoop + 1);

            PlaySoundWithPitch(loopTransitionSound, 1.0f);
            StartCoroutine(ZoomInEffect(bigLoopText.transform, 4f, 1f, 0.4f));
        }
    }


    private IEnumerator ZoomInEffect(Transform target, float startScale, float endScale, float duration)
    {

        Vector3 originalScale = Vector3.one;
        float elapsed = 0f;
        
        Text[] texts = target.GetComponentsInChildren<Text>();
        
        // Farben vorbereiten
        Color[] targetColors = new Color[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            Color c = texts[i].color;
            c.a = 1f; //sichtbar
            targetColors[i] = c;
            
            c.a = 0f; //unsichtbar
            texts[i].color = c;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            float currentScale = Mathf.Lerp(startScale, endScale, easedT);
            target.localScale = originalScale * currentScale;
            
            float alphaT = Mathf.Clamp01(t * 3f); 
            
            for (int i = 0; i < texts.Length; i++)
            {
                Color c = targetColors[i];
                c.a = Mathf.Lerp(0f, targetColors[i].a, alphaT);
                texts[i].color = c;
            }
            
            yield return null;
        }
        
        target.localScale = originalScale * endScale;
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].color = targetColors[i];
        }
    }

    //Hilfsmethode für Sound mit Pitch (ohne extra AudioSource am Objekt zu brauchen)
    private void PlaySoundWithPitch(AudioClip clip, float pitch)
    {
        if (clip == null) return;

        //erstelle temporäres Objekt
        GameObject soundObj = new GameObject("TempAudio");
        soundObj.transform.position = Camera.main.transform.position; // Sound bei der Kamera
        
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.volume = 1f; //Oder variabel

        audioSource.Play();

        // Objekt zerstören wenn fertig
        Destroy(soundObj, clip.length + 0.1f);
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Panels and HUD")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject hud;
    public TransitionController transitionController;

    [Header("HUD Elements")]
    public Text timerText;
    public Text loopText;
    public Text lastScoreText;
    public Text remainingGhostText;
    
    [Header("Score Calculation")]
    public GameObject scoreCalculation;
    public Text sC_currentScore;
    public Text sC_timeLeft; 
    public Text sC_newScore;
    public Text sC_coinScore;
    public GameObject sC_timeLeft_Group; 
    public GameObject sC_newScore_Group; 
    public GameObject coinBonusGroup;

    [Header("Audio")]
    public AudioClip loopTransitionSound;
    public AudioClip scoreCalculationSound;
    public UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;

    //Cached References
    private HUDAnimationController hudAnimController;

    private void Start()
    {
        //Cache HUD Animation Controller
        hudAnimController = FindFirstObjectByType<HUDAnimationController>();
    }

    public void hideScoreCalculation() 
    {
        scoreCalculation.SetActive(false);
        coinBonusGroup.SetActive(false);
    }

    public void UpdateTimer(float timeRemaining)
    {
        timeRemaining = Mathf.Max(0, timeRemaining);
        timerText.text = timeRemaining.ToString("F2") + "s";
        
        //Timer Color: weiß sonst, 15s = gelb , 5s = rot
        if (timeRemaining <= 5f) 
            hudAnimController.SetTimerColor(Color.red);
        else if (timeRemaining <= 15f) 
            hudAnimController.SetTimerColor(Color.yellow);
        else 
            hudAnimController.SetTimerColor(Color.white);
        
        //Shake bei jeder vollen Sekunde ab 5s
        if (timeRemaining <= 5f)
        {
            float fractional = timeRemaining - Mathf.Floor(timeRemaining);
            if (fractional > 0.98f) 
                hudAnimController.ShakeTimer();
        }
        
        //Big Shake bei 0s
        if (timeRemaining == 0f) 
            hudAnimController.BigShakeTimer();
    }

    public void UpdateGhostsRemaining(int remaining) 
    { 
        remainingGhostText.text = "targets left: " + remaining; 
    }

    public void ShowLoopText(int loopIndex)
    {
        loopText.text = "Loop " + loopIndex;
        loopText.gameObject.SetActive(true);
    }
    
    public void HideLoopText() 
    { 
        loopText.gameObject.SetActive(false); 
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        hud.SetActive(false);
        
        float currentScore = PlayerPrefs.GetFloat("LastScore", 0f);
        lastScoreText.text = "Your Score: " + currentScore.ToString("F0");
        
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
        
        gameOverPanel.SetActive(false); 
        pausePanel.SetActive(false);
        hud.SetActive(true);
        Time.timeScale = 1f; 
        PauseManager.isGameOver = false;
        
        yield return GameManager.Instance.StartNewGame();
    }

    public void LoadMainMenu() 
    { 
        StartCoroutine(LoadMainMenuWithFade()); 
    }
    
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
        sC_coinScore.text = coins > 0 ? "+ " + (coins * coinValue).ToString("F0") : "+ 0"; //?
        
        scoreCalculation.SetActive(true);
        ResetScoreCalculationElements();
        
        StartCoroutine(ScoreCalculationZoomIn());
    }

    private void ResetScoreCalculationElements()
    {
        SetGroupAlpha(sC_timeLeft_Group, 0f);
        SetGroupAlpha(sC_newScore_Group, 0f);
        
        coinBonusGroup.SetActive(false);
        SetGroupAlpha(coinBonusGroup, 0f);
    }

    private void SetGroupAlpha(GameObject group, float alpha)
    {
        group.transform.localScale = Vector3.one;
        
        Text[] texts = group.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            Color c = t.color;
            c.a = alpha;
            t.color = c;
        }
    }

    private IEnumerator ScoreCalculationZoomIn()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        //Time Left Bonus
        PlaySoundWithPitch(scoreCalculationSound, 1.4f);
        yield return ZoomInEffect(sC_timeLeft_Group.transform, 4f, 1f, 0.4f);
        yield return new WaitForSecondsRealtime(0.2f);

        //Coin Bonus
        coinBonusGroup.SetActive(true);
        PlaySoundWithPitch(scoreCalculationSound, 1.6f);
        yield return ZoomInEffect(coinBonusGroup.transform, 3f, 1f, 0.4f);
        yield return new WaitForSecondsRealtime(0.2f);
        
        //New Score
        PlaySoundWithPitch(scoreCalculationSound, 1.8f);
        yield return ZoomInEffect(sC_newScore_Group.transform, 5f, 1.2f, 0.5f);
    }

    public IEnumerator AnimateLoopNumber(int fromLoop, int toLoop)
    {
        
        loopText.gameObject.SetActive(true);
        //alte Nummer anzeigen
        //loopText.text = "Loop " + fromLoop;
        //yield return new WaitForSecondsRealtime(0.5f);

        //hochzählen
        for (int i = fromLoop; i < toLoop; i++)
        {
            int nextLoop = i + 1;
            loopText.text = "Loop " + nextLoop;

            PlaySoundWithPitch(scoreCalculationSound, 1f + (0.1f * i));
            yield return ZoomInEffect(loopText.transform, 4f, 1f, 0.4f);

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private IEnumerator ZoomInEffect(Transform target, float startScale, float endScale, float duration)
    {
        Vector3 originalScale = Vector3.one;
        float elapsed = 0f;
        
        Text[] texts = target.GetComponentsInChildren<Text>();
        Color[] targetColors = new Color[texts.Length];
        
        // Farben vorbereiten
        for (int i = 0; i < texts.Length; i++)
        {
            Color c = texts[i].color;
            c.a = 1f;
            targetColors[i] = c;
            
            c.a = 0f;
            texts[i].color = c;
        }

        // Animation
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Eased Scale
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            float currentScale = Mathf.Lerp(startScale, endScale, easedT);
            target.localScale = originalScale * currentScale;
            
            // Fade In
            float alphaT = Mathf.Clamp01(t * 3f);
            for (int i = 0; i < texts.Length; i++)
            {
                Color c = targetColors[i];
                c.a = Mathf.Lerp(0f, targetColors[i].a, alphaT);
                texts[i].color = c;
            }
            
            yield return null;
        }
        
        //final Values
        target.localScale = originalScale * endScale;
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].color = targetColors[i];
        }
    }

    private void PlaySoundWithPitch(AudioClip clip, float pitch)
    {
        GameObject soundObj = new GameObject("TempAudio");
        soundObj.transform.position = Camera.main.transform.position;
        
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.outputAudioMixerGroup = sfxMixerGroup; 

        audioSource.Play();

        Destroy(soundObj, clip.length + 0.1f);
    }
}
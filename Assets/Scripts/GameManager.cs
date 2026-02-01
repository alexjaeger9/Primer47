using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefabs & Spawns")]
    public GameObject playerPrefab;
    public GameObject ghostPrefab;
    public SpawnArea spawnArea; 
    public Transform playerSpawnPoint;
    public Transform targetSpawnPoint;

    [Header("UI")]
    public UIManager uiManager;
    public HUDAnimationController hudAnim;
    public VignetteController vignetteController;
    private float lastTimerSecond = -1;

    [Header("Audio Effects")]
    public AudioSource tickAudioSource;
    public AudioClip timeOverClip;
    public AudioClip startClip;
    public AudioClip gameOverClip;

    [Header("Game State")]
    public int currentLoopIndex;
    public List<RunData> allRuns = new List<RunData>();
    public List<GhostHealth> activeGhosts = new List<GhostHealth>();
    public TransitionController transitionController;

    private const float LOOP_TIME_LIMIT = 30f;
    private const float TICK_START_TIME = 8f;
    private const float MIN_PITCH = 1.0f;
    private const float MAX_PITCH = 4.0f;
    private const float VIGNETTE_TIME_THRESHOLD = 8f;
    private const float TIMER_SHAKE_THRESHOLD = 5f;
    private const float DEATH_Y_POSITION = -10f;
    
    private readonly List<GameObject> allSpawnedGhosts = new List<GameObject>();
    private GameObject player;
    private PlayerRecorder playerRecorder;
    private PlayerHealth playerHealth;
    
    private float currentLoopTime = 0f;
    private bool timerRunning = false;
    private float lastScore = 0f;
    private int coinsThisRun = 0;
    private float coinValue = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartNewGame());   
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            HandleTickingSound(0); 
            return; 
        }

        if (timerRunning)
        {
            currentLoopTime += Time.deltaTime;
            float timeRemaining = LOOP_TIME_LIMIT - currentLoopTime;
            
            uiManager.UpdateTimer(timeRemaining);
            HandleTickingSound(timeRemaining);
            
            // Vignette Logik (Blinkt jetzt schneller, je weniger Zeit bleibt)
            if (timeRemaining <= VIGNETTE_TIME_THRESHOLD && timeRemaining > 0)
            {
                vignetteController.SetActive(true);
                // Wir berechnen, wie weit die Zeit abgelaufen ist (0 bis 1)
                float progress = 1 - (timeRemaining / VIGNETTE_TIME_THRESHOLD);
                vignetteController.UpdatePulseSpeed(progress);
            }
            else
            {
                vignetteController.SetActive(false);
            }

            if (timeRemaining <= TIMER_SHAKE_THRESHOLD && timeRemaining > 0f)
            {
                int currentSecond = Mathf.FloorToInt(timeRemaining);
                if (currentSecond != lastTimerSecond)
                {
                    lastTimerSecond = currentSecond;
                    hudAnim.ShakeTimer();
                }
            }
            
            if (timeRemaining <= 0)
            {
                timerRunning = false;
                hudAnim.BigShakeTimer();
                HandlePlayerDeath(); 
            }

            if (player.transform.position.y < DEATH_Y_POSITION)
            {
                timerRunning = false;
                HandlePlayerDeath(); 
            }
        }
    }

    private void HandleTickingSound(float timeRemaining)
    {
        if (Time.timeScale == 0f)
        {
            if (PauseManager.isGameOver) return; 

            if (tickAudioSource.isPlaying) 
                tickAudioSource.Pause();
            
            return;
        }
        else
        {
            tickAudioSource.UnPause();
        }

        if (timeRemaining <= TICK_START_TIME && timeRemaining > 0)
        {
            if (!tickAudioSource.isPlaying) 
                tickAudioSource.Play();

            float progress = 1 - (timeRemaining / TICK_START_TIME);
            tickAudioSource.pitch = Mathf.Lerp(MIN_PITCH, MAX_PITCH, progress);
        }
        else
        {
            if (tickAudioSource.isPlaying) 
                tickAudioSource.Stop();
        }
    }

    public void AddCoinPoints(float amount)
    {
        coinsThisRun++;
        coinValue = amount;
    }

    public IEnumerator StartNewGame()
    {
        tickAudioSource.Stop();
        tickAudioSource.pitch = 1f;
        
        if (!tickAudioSource.enabled)
            tickAudioSource.enabled = true;
        
        tickAudioSource.PlayOneShot(startClip);

        vignetteController.SetActive(false);

        PauseManager.isGameOver = false;
        Time.timeScale = 1f;
        transitionController.SetAlpha(1f);

        SpawnPlayer();
        ClearGhosts();
        ClearBullets();
        allRuns.Clear();
        currentLoopIndex = 0;
        lastScore = 0f;
        uiManager.scoreCalculation.SetActive(false);

        spawnArea.ResetSpawnHistory();
        PauseManager.canPause = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        uiManager.ShowLoopText(1);
        yield return new WaitForSecondsRealtime(2f);
        uiManager.HideLoopText();

        StartLoop();
        yield return transitionController.FadeOut(1f);
    }

    private void ClearGhosts()
    {
        foreach (GameObject ghost in allSpawnedGhosts)
        {
            Destroy(ghost);
        }
        allSpawnedGhosts.Clear();
        activeGhosts.Clear();

        if (CoinPool.Instance != null) 
            CoinPool.Instance.DeactivateAll();
    }

    private void SpawnPlayer()
    {
        Destroy(player);
        
        Vector3 spawnPosition = spawnArea.GetRandomSpawnPosition();
        
        player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerRecorder = player.GetComponent<PlayerRecorder>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.OnPlayerDeath += HandlePlayerDeath;

        SettingsManager.Instance.ApplySensitivityToGame();
    }

    private void StartLoop()
    {
        if (currentLoopIndex == 0) 
            SpawnInitialTarget();
        else 
            SpawnGhostsFromRuns();
        
        UpdateGhostNumbers();
        playerRecorder.StartRecording();

        currentLoopTime = 0f;
        timerRunning = true;
        lastTimerSecond = -1;

        uiManager.UpdateGhostsRemaining(activeGhosts.Count);
        hudAnim.SlideInAll();

        playerHealth.UpdatePlayerNumbers(currentLoopIndex + 1);

        ExplosiveBarrel[] barrels = Object.FindObjectsByType<ExplosiveBarrel>(FindObjectsSortMode.None);
        if (barrels != null)
        {
            foreach (ExplosiveBarrel barrel in barrels)
            {
                barrel.Respawn();
            }
        }

        if (CoinPool.Instance != null) 
            CoinPool.Instance.SpawnCoins();
    }

    private void SpawnGhostsFromRuns()
    {
        SpawnInitialTarget();
        foreach (RunData run in allRuns)
        {
            Vector3 ghostSpawnPos = run.startPosition;
        
            GameObject ghostGO = Instantiate(ghostPrefab, ghostSpawnPos, Quaternion.identity);
            allSpawnedGhosts.Add(ghostGO);
            GhostController controller = ghostGO.GetComponent<GhostController>();
            GhostHealth health = ghostGO.GetComponent<GhostHealth>();
            controller.Init(run);
            activeGhosts.Add(health);
        }
    }

    private void SpawnInitialTarget()
    {
        GameObject ghostZero = Instantiate(ghostPrefab, targetSpawnPoint.position, targetSpawnPoint.rotation);
        allSpawnedGhosts.Add(ghostZero);
        GhostHealth health = ghostZero.GetComponent<GhostHealth>();
        activeGhosts.Add(health);
    }

    private void UpdateGhostNumbers()
    {
        int count = 0;
        foreach (GhostHealth health in activeGhosts)
        {
            health.UpdateGhostNumbers(count);
            count++;
        }
    }

    private IEnumerator LoopTransition()
    {
        PauseManager.canPause = false;
        
        ScoreCalculation();
        yield return SmoothSlowMo(0.1f, 0.5f);
        yield return new WaitForSecondsRealtime(2f);
        yield return transitionController.FadeIn(0.3f);
        
        uiManager.hideScoreCalculation();
        
        int previousLoop = currentLoopIndex + 1; 
        EndLoop(); 
        int nextLoop = currentLoopIndex + 1;

        ClearGhosts();
        ClearBullets();
        SpawnPlayer();
        
        yield return uiManager.AnimateLoopNumber(previousLoop, nextLoop);
        yield return new WaitForSecondsRealtime(0.5f);
        
        StartCoroutine(transitionController.FadeOut(0.2f));
        uiManager.HideLoopText();
        
        StartLoop();
        
        Time.timeScale = 1f;
        PauseManager.canPause = true;
    }

    private IEnumerator SmoothSlowMo(float slowAmount, float duration)
    {
        float startTimeScale = Time.timeScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(startTimeScale, slowAmount, elapsed / duration);
            yield return null;
        }
        
        Time.timeScale = slowAmount;
    }

    private void EndLoop()
    {
        RunData data = playerRecorder.StopRecording();
        allRuns.Add(data);
        currentLoopIndex++;
        coinsThisRun = 0;
    }

    private void ClearBullets()
    {
        TracerMovement[] tracers = FindObjectsByType<TracerMovement>(FindObjectsSortMode.None);
        foreach (TracerMovement tracer in tracers)
        {
            if (tracer.gameObject.activeInHierarchy)
            {
                tracer.StopAllCoroutines();
                BulletPool.Instance.ReturnBullet(tracer.gameObject);
            }
        }
    }

    private void ScoreCalculation()
    {
        float timeLeft = Mathf.Max(0, LOOP_TIME_LIMIT - currentLoopTime);
        float newScore = lastScore + timeLeft + (coinsThisRun * coinValue);
        uiManager.showScoreScalculation(lastScore, timeLeft, newScore, coinsThisRun, coinValue);
        lastScore = newScore;
    }

    private void HandlePlayerDeath()
    {
        timerRunning = false;
        vignetteController.TriggerPermanentVignette();

        tickAudioSource.Stop();
        tickAudioSource.PlayOneShot(timeOverClip);
        tickAudioSource.pitch = 1f;

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        PauseManager.canPause = false;
        PauseManager.isGameOver = true;
        playerRecorder.enabled = false;
        
        Animator animator = player.GetComponent<Animator>();
        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerShooter shooter = player.GetComponent<PlayerShooter>();
        CharacterController charController = player.GetComponent<CharacterController>();
        
        controller.enabled = false;
        shooter.enabled = false;
        animator.enabled = false;

        ThirdPersonCamera cam = FindAnyObjectByType<ThirdPersonCamera>();
        if (cam != null)
            cam.EnableFreeCamera();

        Time.timeScale = 0.2f;
        float elapsed = 0f;
        Vector3 velocity = Vector3.zero;

        while (elapsed < 4f)
        {
            velocity.y += -9.81f * Time.unscaledDeltaTime;
            charController.Move(velocity * Time.unscaledDeltaTime);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (GhostHealth activeGhost in activeGhosts)
        {
            activeGhost.StopMovement();
        }

        if (cam != null)
            cam.enabled = false;

        hudAnim.SlideOutAll();
        yield return new WaitForSecondsRealtime(0.6f); 
        
        Time.timeScale = 0f;
        tickAudioSource.pitch = 1f; 
        tickAudioSource.PlayOneShot(gameOverClip);
        SaveScore();
        uiManager.ShowGameOver();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SaveScore()
    {
        PlayerPrefs.SetFloat("LastScore", lastScore);
        
        float highScore = PlayerPrefs.GetFloat("HighScore", 0);
        if (lastScore > highScore)
        {
            PlayerPrefs.SetFloat("HighScore", lastScore);
        }
        
        PlayerPrefs.Save();
    }

    public void OnGhostKilled(GhostHealth ghost)
    {
        activeGhosts.Remove(ghost);
        uiManager.UpdateGhostsRemaining(activeGhosts.Count);

        if (activeGhosts.Count <= 3 && activeGhosts.Count >= 1)
            hudAnim.ShakeAndFlashTargets();
        else
            hudAnim.ShakeTargets();
        
        if (activeGhosts.Count == 0)
        {
            timerRunning = false; 
            tickAudioSource.Stop();
            tickAudioSource.pitch = 1f;

            StartCoroutine(LoopTransition());
        }
    }
}
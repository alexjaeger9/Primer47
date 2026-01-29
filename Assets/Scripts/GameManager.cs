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

    [Header("Game State")]
    public int currentLoopIndex;
    public List<RunData> allRuns = new List<RunData>();
    public List<GhostHealth> activeGhosts = new List<GhostHealth>();
    private readonly List<GameObject> allSpawnedGhosts = new List<GameObject>();
    private GameObject player;
    private PlayerRecorder playerRecorder;
    private PlayerHealth playerHealth;
    public TransitionController transitionController;

    private float loopTimeLimit = 30f;
    private float currentLoopTime = 0f;
    private bool timerRunning = false;
    private float lastScore = 0f; // Das sind deine Gesamtpunkte


    private void Awake()
    {
        //GameManager zuweisen
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartNewGame());   
    }

    private void Update()
    {
        if (timerRunning)
        {
            currentLoopTime += Time.deltaTime;
            float timeRemaining = loopTimeLimit - currentLoopTime;
            
            //Timer updaten
            uiManager.UpdateTimer(timeRemaining);
            
            //Zeit abgelaufen
            if (timeRemaining <= 0)
            {
                timerRunning = false;
                HandlePlayerDeath(); //Game Over
            }

            //Von Map gesprungen
            if (player.transform.position.y < -10f)
            {
                timerRunning = false;
                HandlePlayerDeath(); //Game Over
            }
        }
    }

    // --- NEU: COIN LOGIC ---
    // Diese Methode wird vom Coin aufgerufen
    public void AddCoinPoints(float amount)
    {
        lastScore += amount;
        // Optional: UI direkt updaten, damit man sieht dass man Punkte bekommen hat
        // (Da dein UIManager.UpdateScore(float) existiert, nutzen wir das direkt)
        // Aber Achtung: ScoreCalculation rechnet später nochmal Zeit drauf. 
        // Fürs Feedback ist es aber gut.
        // Falls du eine separate UI für "Coins" hast, hier nutzen.
    }
    // -----------------------

    //der erste Start
    public IEnumerator StartNewGame()
    {
        Time.timeScale = 1f;
        transitionController.SetAlpha(1f);

        SpawnPlayer();
        ClearGhosts();
        ClearBullets();
        allRuns.Clear();
        currentLoopIndex = 0;
        lastScore = 0f; //Score resetten
        uiManager.UpdateScore(0); //UI-Score updaten  
        uiManager.hideScoreCalculation();

        spawnArea.ResetSpawnHistory();
        PauseManager.canPause = true; //pausieren erlauben

        //Cursor locken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //großen Loop text 1 anzeigen
        uiManager.ShowBigLoopText(1); //Loop Text anzeigen
        yield return new WaitForSecondsRealtime(2f); //Dauer der Anzeige
        uiManager.HideBigLoopText(); //Loop Text hiden

        StartLoop();
        yield return transitionController.FadeOut(1f);
    }


    private void ClearGhosts()
    {
        foreach (GameObject ghost in allSpawnedGhosts)
        {
            Destroy(ghost.gameObject);
        }
        allSpawnedGhosts.Clear();
        activeGhosts.Clear();
        
        // --- NEU: COIN LOGIC ---
        // Wenn Geister gelöscht werden, resetten wir auch die Coins
        if (CoinPool.Instance != null) CoinPool.Instance.DeactivateAll();
        // -----------------------
    }

    private void SpawnPlayer()
    {
        if(player != null) Destroy(player); // Sicherheitscheck
        
        //Random Position von SpawnArea
        Vector3 spawnPosition = spawnArea.GetRandomSpawnPosition();
        
        player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerRecorder = player.GetComponent<PlayerRecorder>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    private void StartLoop()
    {
        if (currentLoopIndex == 0) SpawnInitialTarget();
        else SpawnGhostsFromRuns();
        UpdateGhostNumbers();
        playerRecorder.StartRecording();

        //Timer starten
        currentLoopTime = 0f;
        timerRunning = true;

        //UI Updaten
        uiManager.UpdateLoopCounter(currentLoopIndex + 1);
        uiManager.UpdateGhostsRemaining(activeGhosts.Count);

        //Player Number Update
        playerHealth.UpdatePlayerNumbers(currentLoopIndex + 1);

        //Explosive Barrels respawnen
        ExplosiveBarrel[] barrels = Object.FindObjectsByType<ExplosiveBarrel>(FindObjectsSortMode.None);
        foreach (ExplosiveBarrel barrel in barrels)
        {
            barrel.Respawn();
        }

        // --- NEU: COIN LOGIC ---
        // Neue Coins verteilen für diesen Loop
        if (CoinPool.Instance != null) CoinPool.Instance.SpawnCoins();
        // -----------------------
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
        PauseManager.canPause = false; //pausieren blocken

        ScoreCalculation();

        //Slow Mo (auf 0,1 verlangsamen in 1s)
        yield return SmoothSlowMo(0.1f, 0.5f);
        
        //kurz in Slow Mo warten
        yield return new WaitForSecondsRealtime(2f);

        uiManager.hideScoreCalculation();

        //Fade to Black
        StartCoroutine(transitionController.FadeIn(1f));
        
        EndLoop();

        uiManager.UpdateScore(lastScore);

        //Loop Text Anzeige
        uiManager.ShowBigLoopText(currentLoopIndex + 1);
        yield return new WaitForSecondsRealtime(1.2f); //Dauer in der der Text angezeigt wird        
        uiManager.HideBigLoopText();
        
        //Loop Clearen
        ClearGhosts();
        ClearBullets();
        SpawnPlayer();
        StartLoop();
        
        //Slow Mo beenden
        Time.timeScale = 1f;

        PauseManager.canPause = true; //pausieren wieder erlauben

        //Fade from Black
        yield return transitionController.FadeOut(1f);
    }

    private IEnumerator SmoothSlowMo(float slowAmount, float duration)
    {
        float startTimeScale = Time.timeScale; //Anfangsspeed
        float elapsed = 0f; //Zeit vergangen
        
        while (elapsed < duration) //solange vergangene Zeit kleiner ist als Duration
        {
            elapsed += Time.unscaledDeltaTime; //zählt Zeit hoch
            Time.timeScale = Mathf.Lerp(startTimeScale, slowAmount, elapsed / duration);
            yield return null;
        }
        
        Time.timeScale = slowAmount;
    }

    private void EndLoop()
    {
        if(playerRecorder != null)
        {
             RunData data = playerRecorder.StopRecording();
             allRuns.Add(data);
        }
        currentLoopIndex++;
    }

    private void ClearBullets()
    {
        TracerMovement[] tracers = FindObjectsByType<TracerMovement>(FindObjectsSortMode.None);
        foreach (TracerMovement tracer in tracers)
        {
            if (tracer.gameObject.activeInHierarchy)
            {
                tracer.StopAllCoroutines();
                if(BulletPool.Instance != null) BulletPool.Instance.ReturnBullet(tracer.gameObject);
                else Destroy(tracer.gameObject);
            }
        }
    }

    private void ScoreCalculation()
    {
        float timeLeft = Mathf.Max(0, loopTimeLimit - currentLoopTime);
        float newScore = lastScore + timeLeft; // Hier wird Zeit UND gesammelte Coins (die schon in lastScore sind) addiert
        uiManager.showScoreScalculation(lastScore, timeLeft, newScore);
        lastScore = newScore;
    }


    private void HandlePlayerDeath()
    {
        timerRunning = false;
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        PauseManager.canPause = false; //pausieren blocken
        PauseManager.isGameOver = true;
        playerRecorder.enabled = false;
        
        // Checken ob Player existiert bevor wir Components holen
        if(player != null)
        {
            Animator animator = player.GetComponent<Animator>();
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerShooter shooter = player.GetComponent<PlayerShooter>();
            CharacterController charController = player.GetComponent<CharacterController>();

            if(controller) controller.enabled = false;
            if(shooter) shooter.enabled = false;
            if(animator) animator.enabled = false;

            //Kamera freischalten für 360 Blick
            ThirdPersonCamera cam = FindAnyObjectByType<ThirdPersonCamera>();
            if(cam) cam.EnableFreeCamera();

            //Slow Mo (4 Sekunden)
            Time.timeScale = 0.2f;
            float elapsed = 0f;
            Vector3 velocity = Vector3.zero; //für Gravitation

            while (elapsed < 4f)
            {
                //Gravity anwenden während Slow Mo
                velocity.y += -9.81f * Time.unscaledDeltaTime;
                if(charController) charController.Move(velocity * Time.unscaledDeltaTime);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            if(cam) cam.enabled = false;
        }

        //stop Movement of all Ghosts
        foreach (GhostHealth activeGhost in activeGhosts)
        {
            activeGhost.StopMovement();
        }

        //GameOver Panel zeigen
        Time.timeScale = 0f;
        SaveScore();
        uiManager.ShowGameOver();

        //Cursor freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SaveScore()
    {
        //aktueller Score wird zu "Last Score"
        PlayerPrefs.SetFloat("LastScore", lastScore);
        
        //High Score updaten wenn besser
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
        //Counter updaten
        uiManager.UpdateGhostsRemaining(activeGhosts.Count);
        
        if (activeGhosts.Count == 0)
        {
            timerRunning = false; //Timer stoppen
            StartCoroutine(LoopTransition());
        }
    }
}
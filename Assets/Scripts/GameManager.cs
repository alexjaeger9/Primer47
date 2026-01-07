using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefabs & Spawns")]
    public GameObject playerPrefab;
    public GameObject ghostPrefab;
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
    private int lastScore = 0;


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
        }
    }

    //der erste Start
    public IEnumerator StartNewGame()
    {
        transitionController.SetAlpha(1f);

        SpawnPlayer();
        ClearGhosts();
        ClearBullets();
        allRuns.Clear();
        currentLoopIndex = 0;
        lastScore = 0; //Score resetten
        uiManager.UpdateScore(0); //UI-Score updaten  
        
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
    }

    private void SpawnPlayer()
    {
        Destroy(player);
        player = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        playerRecorder = player.GetComponent<PlayerRecorder>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    private void StartLoop()
    {
        if (currentLoopIndex == 0) SpawnInitialTarget();
        else SpawnGhostsFromRuns();
        playerRecorder.StartRecording();

        //Timer starten
        currentLoopTime = 0f;
        timerRunning = true;

        //UI Updaten
        uiManager.UpdateLoopCounter(currentLoopIndex + 1);
        uiManager.UpdateGhostsRemaining(activeGhosts.Count);
    }

    private void SpawnGhostsFromRuns()
    {
        SpawnInitialTarget();
        foreach (RunData run in allRuns)
        {
            GameObject ghostGO = Instantiate(ghostPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
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

    private IEnumerator LoopTransition()
    {
        PauseManager.canPause = false; //pausieren blocken
        
        //Slow Mo (auf 0,1 verlangsamen in 1s)
        yield return SmoothSlowMo(0.1f, 0.5f);
        
        //kurz in Slow Mo warten
        yield return new WaitForSecondsRealtime(0.5f);
        
        //Fade to Black
        StartCoroutine(transitionController.FadeIn(1f));
        
        EndLoop();
        
        //Loop Text Anzeige
        uiManager.ShowBigLoopText(currentLoopIndex + 1);
        yield return new WaitForSecondsRealtime(1.2f); //Dauer in der der Text angezeigt wird        
        uiManager.HideBigLoopText();
        
        //Loop Clearen
        ClearGhosts();
        ClearBullets();
        StartLoop();
        
        //Slow Mo beenden & Score berechnen
        Time.timeScale = 1f;
        CalculateScore();

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
        RunData data = playerRecorder.StopRecording();
        allRuns.Add(data);
        currentLoopIndex++;
    }

    private void ClearBullets()
    {
        TracerMovement[] tracers = FindObjectsByType<TracerMovement>(FindObjectsSortMode.None);
        foreach (TracerMovement tracer in tracers)
        {
            Destroy(tracer.gameObject);
        }
    }

    private void CalculateScore()
    {
        //je schneller, desto mehr Punkte
        float timeBonus = Mathf.Max(0, loopTimeLimit - currentLoopTime);
        int timeBonusPoints = Mathf.RoundToInt(timeBonus * 10); //bsp.: 15sek -> 150pt
        
        //jeder Loop gibt mehr Punkte
        int loopBonusPoints = currentLoopIndex * 100;
        
        //gesamt
        lastScore += timeBonusPoints + loopBonusPoints;
        
        //UI updaten
        uiManager.UpdateScore(lastScore);
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
        Animator animator = player.GetComponent<Animator>();
        
        //Player Movement, schießen, Animation stoppen
        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerShooter shooter = player.GetComponent<PlayerShooter>();
        CharacterController charController = player.GetComponent<CharacterController>();
        controller.enabled = false;
        shooter.enabled = false;
        animator.enabled = false;

        // Kamera freischalten für 360° Blick
        ThirdPersonCamera cam = FindAnyObjectByType<ThirdPersonCamera>();
        cam.EnableFreeCamera();
        
        //Slow Mo (4 Sekunden)
        Time.timeScale = 0.2f;
        float elapsed = 0f;
        Vector3 velocity = Vector3.zero; //für Gravitation
        
        while (elapsed < 4f)
        {
            //Gravity anwenden während Slow Mo
            velocity.y += -9.81f * Time.unscaledDeltaTime;
            charController.Move(velocity * Time.unscaledDeltaTime);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cam.enabled = false;
        
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
        PlayerPrefs.SetInt("LastScore", lastScore);
        
        //High Score updaten wenn besser
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (lastScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", lastScore);
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
        else
        {
            //Ghosts übrig -> Hitstop
            StartCoroutine(HitstopEffect());
        }
    }

    private IEnumerator HitstopEffect()
    {
        //Timer pausieren
        timerRunning = false;
        
        //Slow Mo
        Time.timeScale = 0.3f;
        
        //0.1 Sekunden warten (realtime weil timeScale verändert)
        yield return new WaitForSecondsRealtime(0.3f);
        
        //zurück zu normal
        Time.timeScale = 1f;
        timerRunning = true;
    }
}

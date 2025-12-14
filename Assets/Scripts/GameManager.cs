using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefabs & Spawns")]
    public GameObject playerPrefab;
    public GameObject ghostPrefab;
    public Transform playerSpawnPoint;
    public Transform targetSpawnPoint;

    [Header("UI")]
    //public UIManager uiManager;

    [Header("Game State")]
    public int currentLoopIndex;
    public List<RunData> allRuns = new List<RunData>();
    public List<GhostHealth> activeGhosts = new List<GhostHealth>();
    private List<GameObject> allSpawnedGhosts = new List<GameObject>();

    private PlayerRecorder playerRecorder;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        allRuns.Clear();
        currentLoopIndex = 0;
        SpawnPlayer();
        StartLoop();
    }

    private void SpawnPlayer()
    {
        GameObject player = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        playerRecorder = player.GetComponent<PlayerRecorder>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    private void StartLoop()
    {
        //uiManager.SetLoopText(currentLoopIndex);
        if (currentLoopIndex == 0)
        {
            // --- LOOP 0: Initiales Ziel spawnen ---
            SpawnInitialTarget();
        }
        else
        {
            // --- LOOP 1+: Ghosts aus Runs spawnen ---
            SpawnGhostsFromRuns();
        }

        // Starte die Aufnahme in jedem Loop
        // Der Player wird in StartNewGame() gespawnt, daher ist playerRecorder verfügbar.
        if (playerRecorder != null)
        {
            playerRecorder.StartRecording();
        }
    }

    private void EndLoop()
    {
        RunData data = playerRecorder.StopRecording();
        if (data != null)
        {
            allRuns.Add(data);
        }

        currentLoopIndex++;
        ClearGhosts();
        StartLoop();
    }

    private void SpawnGhostsFromRuns()
    {
        ClearGhosts();
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

    private void ClearGhosts()
    {
        foreach (GameObject ghost in allSpawnedGhosts)
        {
            if (ghost != null) Destroy(ghost.gameObject);
        }
        allSpawnedGhosts.Clear();
        activeGhosts.Clear();
    }

    public void OnGhostKilled(GhostHealth ghost)
    {
        if (activeGhosts.Contains(ghost))
        {
            activeGhosts.Remove(ghost);
        }

        if (activeGhosts.Count == 0)
        {
            EndLoop();
        }
    }

    private void HandlePlayerDeath()
    {
        GameOver();
    }

    private void GameOver()
    {
        // TODO: GameOver-UI, Restart-Option
        //uiManager.ShowGameOver();
    }

    private void SpawnInitialTarget()
    {
        ClearGhosts(); // Stellt sicher, dass die Liste leer ist

        if (ghostPrefab == null || targetSpawnPoint == null)
        {
            Debug.LogError("InitialTargetPrefab oder TargetSpawnPoint fehlt im GameManager!");
            return;
        }

        // Spawne das Ziel an der TargetSpawnPoint Position
        GameObject targetGO = Instantiate(ghostPrefab, targetSpawnPoint.position, targetSpawnPoint.rotation);

        allSpawnedGhosts.Add(targetGO);

        // Wir benötigen die GhostHealth-Komponente, da diese den Kill registriert
        GhostHealth health = targetGO.GetComponent<GhostHealth>();

        if (health != null)
        {
            activeGhosts.Add(health);
            Debug.Log("Initiales Ziel für Loop 0 gespawnt.");
        }
        else
        {
            Debug.LogError("InitialTargetPrefab benötigt eine GhostHealth-Komponente!");
            Destroy(targetGO);
        }
    }
}

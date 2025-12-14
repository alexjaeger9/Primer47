using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefabs & Spawns")]
    public GameObject playerPrefab;
    public GameObject ghostPrefab;
    public Transform playerSpawnPoint;

    [Header("UI")]
    public UIManager uiManager;

    [Header("Game State")]
    public int currentLoopIndex;
    public List<RunData> allRuns = new List<RunData>();
    public List<GhostHealth> activeGhosts = new List<GhostHealth>();

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
        uiManager.SetLoopText(currentLoopIndex);
        SpawnGhostsFromRuns();
        playerRecorder.StartRecording();
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
        foreach (RunData run in allRuns)
        {
            GameObject ghostGO = Instantiate(ghostPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            GhostController controller = ghostGO.GetComponent<GhostController>();
            GhostHealth health = ghostGO.GetComponent<GhostHealth>();

            controller.Init(run);
            activeGhosts.Add(health);
        }
    }

    private void ClearGhosts()
    {
        foreach (GhostHealth gh in activeGhosts)
        {
            if (gh != null) Destroy(gh.gameObject);
        }
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
        uiManager.ShowGameOver();
    }
}

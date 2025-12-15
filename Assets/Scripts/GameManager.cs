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
    //public UIManager uiManager;
    [Header("Game State")]
    public int currentLoopIndex;
    public List<RunData> allRuns = new List<RunData>();
    public List<GhostHealth> activeGhosts = new List<GhostHealth>();
    private List<GameObject> allSpawnedGhosts = new List<GameObject>();
    private PlayerRecorder playerRecorder;
    private PlayerHealth playerHealth;
    private float timeBetweenLoops = 1.0f;

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
        if (currentLoopIndex == 0) SpawnInitialTarget();
        else SpawnGhostsFromRuns();
        if (playerRecorder != null) playerRecorder.StartRecording();
    }

    private void EndLoop()
    {
        RunData data = playerRecorder.StopRecording();
        if (data != null) allRuns.Add(data);
        currentLoopIndex++;
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
        if (activeGhosts.Contains(ghost)) activeGhosts.Remove(ghost);
        if (activeGhosts.Count == 0) StartCoroutine(WaitBetweenLoops());
    }

    private IEnumerator WaitBetweenLoops()
    {
        yield return new WaitForSeconds(0.1f);
        EndLoop();
        yield return new WaitForSeconds(timeBetweenLoops);
        ClearGhosts();
        StartLoop();
    }

    private void HandlePlayerDeath()
    {
        GameOver();
    }

    private void GameOver()
    {
        // TODO: GameOver-UI, Restart-Option
        //uiManager.ShowGameOver();
        ClearGhosts();
    }

    private void SpawnInitialTarget()
    {
        GameObject ghostZero = Instantiate(ghostPrefab, targetSpawnPoint.position, targetSpawnPoint.rotation);
        allSpawnedGhosts.Add(ghostZero);
        GhostHealth health = ghostZero.GetComponent<GhostHealth>();
        if (health != null)
        {
            activeGhosts.Add(health);
        }
    }
}

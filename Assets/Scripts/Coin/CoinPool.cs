using UnityEngine;
using System.Collections.Generic;

public class CoinPool : MonoBehaviour
{
    public static CoinPool Instance;

    [Header("Setup")]
    public GameObject coinPrefab;
    public int coinsPerLoop = 3;
    
    [Header("Positions")]
    public List<Transform> spawnPoints;

    private List<GameObject> coinPool = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < coinsPerLoop; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            coin.transform.parent = transform;
            coinPool.Add(coin);
        }
    }

    public void SpawnCoins()
    {
        DeactivateAll();

        if (spawnPoints.Count < coinsPerLoop) return;
        List<Transform> availableSpots = new List<Transform>(spawnPoints);

        foreach (GameObject coin in coinPool)
        {
            // Zufälligen Ort wählen
            int randomIndex = Random.Range(0, availableSpots.Count);
            Transform spot = availableSpots[randomIndex];

            // Coin platzieren
            coin.transform.position = spot.position;
            coin.transform.rotation = Quaternion.identity;
            coin.SetActive(true);

            // Benutzten Ort entfernen
            availableSpots.RemoveAt(randomIndex);
        }
    }

    public void DeactivateAll()
    {
        foreach (GameObject coin in coinPool)
        {
            coin.SetActive(false);
        }
    }
}
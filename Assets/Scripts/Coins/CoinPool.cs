using UnityEngine;
using System.Collections.Generic;

public class CoinPool : MonoBehaviour
{
    public static CoinPool Instance;

    [Header("Setup")]
    public GameObject coinPrefab; // Dein Coin Prefab
    public int coinsPerLoop = 3;  // Wir wollen immer nur 3 Coins gleichzeitig
    
    [Header("Positions")]
    public List<Transform> spawnPoints; // Hier ziehst du deine leeren GameObjects rein

    private List<GameObject> coinPool = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    // Erstellt die Coins EINMAL beim Spielstart (Pooling)
    void InitializePool()
    {
        for (int i = 0; i < coinsPerLoop; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false); // Erstmal unsichtbar
            coin.transform.parent = transform; // Der Ordnung halber im Hierarchy-Baum unterordnen
            coinPool.Add(coin);
        }
    }

    public void SpawnCoins()
    {
        // Erstmal alle deaktivieren (falls noch welche vom letzten Loop da sind)
        DeactivateAll();

        if (spawnPoints.Count < coinsPerLoop) return;

        // Liste kopieren, damit wir Positionen rausstreichen können (damit keine 2 Coins am selben Ort landen)
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
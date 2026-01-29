using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Settings")]
    public float points = 50f; // Wie viele Punkte gibt ein Coin?

    [Header("Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffectPrefab; // Das Partikel-Prefab

    [Header("Animation")]
    public float rotateSpeed = 100f; // Wie schnell soll er sich drehen?

    private void OnTriggerEnter(Collider other)
    {
        // Prüfen ob es der Spieler ist (Tag muss "Player" sein!)
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // 1. Punkte an den GameManager senden
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoinPoints(points);
        }

        // 2. Sound abspielen (Erstellt temporäres Audio-Objekt an der Position)
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 3. Partikel spawnen (Unabhängig vom Coin)
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        // 4. POOLING: Nicht zerstören, nur deaktivieren!
        gameObject.SetActive(false);
    }
    void Update()
    {
        // Dreht das Objekt dauerhaft um die Y-Achse (hochkant)
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
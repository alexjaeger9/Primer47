using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Settings")]
    private float points = 5f; // Score für Coin

    [Header("Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffectPrefab; // Partikel-Prefab

    [Header("Animation")]
    public float rotateSpeed = 100f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Collect();
    }

    void Collect()
    {
        // Punkte an den GameManager senden
        if (GameManager.Instance != null) GameManager.Instance.AddCoinPoints(points);

        // Sound abspielen (Erstellt temporäres Audio-Objekt an der Position)
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // Partikel spawnen (Unabhängig vom Coin)
        if (pickupEffectPrefab != null) Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        HUDAnimationController hudAnim = FindFirstObjectByType<HUDAnimationController>();
        hudAnim.ShakeAndFlash("Coins");

        // POOLING: Nicht zerstören, nur deaktivieren!
        gameObject.SetActive(false);
    }
    void Update()
    {
        // Dreht das Objekt dauerhaft um die Y-Achse (hochkant)
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
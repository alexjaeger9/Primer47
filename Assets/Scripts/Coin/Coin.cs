using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Settings")]
    private float points = 5f; // Score

    [Header("Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffectPrefab;

    [Header("Animation")]
    public float rotateSpeed = 100f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Collect();
    }

    void Collect()
    {
        // Punkte an den GameManager senden
        GameManager.Instance.AddCoinPoints(points);

        // Sound abspielen
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // Partikel spawnen
        Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        // deaktivieren
        gameObject.SetActive(false);
    }
    void Update()
    {
        // Animiert Coin um die eigene Achse
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Settings")]
    private float points = 5f;

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
        GameManager.Instance.AddCoinPoints(points);
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
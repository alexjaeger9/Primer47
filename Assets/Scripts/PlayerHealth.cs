using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    // Event, das der GameManager abonniert, um das Spiel zu beenden
    public event Action OnPlayerDeath;

    // Optional: Lebenspunkte hinzufügen, wenn Sie mehr als 1 Treffer zulassen
    // [SerializeField] private int maxHealth = 1; 
    // private int currentHealth;

    private void Awake()
    {
        // currentHealth = maxHealth;
    }

    // Diese Methode wird vom GhostShooter aufgerufen
    public void TakeDamage()
    {
        // currentHealth--;

        Debug.Log("Spieler wurde getroffen!");

        // Wenn Sie One-Shot-Kill verwenden:
        Die();

        /*
        // Oder mit Lebenspunkten:
        if (currentHealth <= 0)
        {
            Die();
        }
        */
    }

    private void Die()
    {
        // 1. Visuelle/Sound-Effekte

        // 2. Bewegung deaktivieren (damit der Recorder keine weiteren Frames aufzeichnet)
        //PlayerController controller = GetComponent<PlayerController>();
        //if (controller != null) controller.enabled = false;

        Debug.Log("GAME OVER - YOU GOT KILLED");

        // 3. Informiere den GameManager
        OnPlayerDeath?.Invoke();

        // Optional: Zerstöre das Spieler-Objekt nach einer kurzen Verzögerung
        // Destroy(gameObject, 0.5f);
    }
}
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public event Action OnPlayerDeath;

    public void TakeDamage()
    {
        Die();
    }

    private void Die()
    {
        Debug.Log("GAME OVER - YOU GOT KILLED");
        // 1. Visuelle/Sound-Effekte

        // 2. Bewegung deaktivieren (damit der Recorder keine weiteren Frames aufzeichnet)

        //PlayerController controller = GetComponent<PlayerController>();
        //if (controller != null) controller.enabled = false;

        // 3. Informiere den GameManager
        OnPlayerDeath?.Invoke();

        // Optional: Zerstöre das Spieler-Objekt nach einer kurzen Verzögerung
        // Destroy(gameObject, 0.5f);
    }
}
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
        // Visuelle/Sound-Effekte

        // Informiere den GameManager
        OnPlayerDeath?.Invoke();
    }
}
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public event Action OnPlayerDeath;

    public bool isDead;

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        isDead = true;

        // TODO: Death-Animation, Effekte

        OnPlayerDeath?.Invoke();
    }
}

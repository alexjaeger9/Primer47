using System;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public event Action OnPlayerDeath;
    public GameObject playerNumbers;

    public void TakeDamage()
    {
        Die();
    }

    private void Die()
    {
        // an GameManager
        OnPlayerDeath?.Invoke();
    }

    public void UpdatePlayerNumbers(int loopCount)
    {
        TextMeshPro[] numbers = playerNumbers.GetComponentsInChildren<TextMeshPro>();
        foreach (TextMeshPro txt in numbers)
        {
            txt.text = loopCount.ToString();
        }
    }
}
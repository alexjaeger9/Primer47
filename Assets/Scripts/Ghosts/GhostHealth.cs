using UnityEngine;

public class GhostHealth : MonoBehaviour
{
    public bool isDead;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        isDead = true;

        // TODO: Death-Animation / Effekte

        gameManager.OnGhostKilled(this);
        Destroy(gameObject);
    }
}

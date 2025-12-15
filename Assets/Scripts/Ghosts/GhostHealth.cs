using UnityEngine;

public class GhostHealth : MonoBehaviour
{
    private Renderer enemyRenderer;

    void Awake()
    {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
        }
    }

    public void TakeHit()
    {
        if (enemyRenderer != null && enemyRenderer.material.color != Color.gray)
        {
            enemyRenderer.material.color = Color.gray;
            if (TryGetComponent<Collider>(out var col))
            {
                col.enabled = false;
            }
            if (TryGetComponent<GhostController>(out var controller)) controller.enabled = false;
            GameManager.Instance.OnGhostKilled(this);
        }
    }
}

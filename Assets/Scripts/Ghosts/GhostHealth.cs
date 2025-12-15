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
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
            GhostController controller = GetComponent<GhostController>();
            if (controller != null) controller.enabled = false;
            GameManager.Instance.OnGhostKilled(this);
        }
    }
}

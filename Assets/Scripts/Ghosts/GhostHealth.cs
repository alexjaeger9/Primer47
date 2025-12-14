using UnityEngine;

public class GhostHealth : MonoBehaviour
{
    // Wir speichern den initialen Renderer, um das Material zu ändern
    private Renderer enemyRenderer;

    void Awake()
    {
        // Holt den Renderer auf diesem GameObject (sollte die Kapsel sein)
        enemyRenderer = GetComponent<Renderer>();

        // Setzt die initiale Farbe auf Rot
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
        }
    }

    // Diese Methode wird vom PlayerShooter aufgerufen, wenn der Gegner getroffen wird
    public void TakeHit()
    {
        // Hier könnten Sie Logik für Lebenspunkte, Soundeffekte etc. hinzufügen.
        // Fürs Erste: Ändert die Farbe auf Grau und verhindert weitere Treffer (optional)

        if (enemyRenderer != null && enemyRenderer.material.color != Color.gray)
        {
            Debug.Log(gameObject.name + " wurde getroffen!");

            // Ändert die Farbe auf Grau
            enemyRenderer.material.color = Color.gray;

            // Optional: Deaktiviert den Collider, damit er nicht mehrmals getroffen werden kann
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            GameManager.Instance.OnGhostKilled(this);

            // Optional: Zerstört das Objekt nach einer kurzen Verzögerung
            // Destroy(gameObject, 3f);
        }
    }
}

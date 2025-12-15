using UnityEngine;
using System.Collections;

public class GhostShooter : MonoBehaviour
{
    public Transform muzzleTransform; // Nur noch für visuelle Hierarchie/Platzierung
    public Transform pitchTarget;     // Wird nicht mehr für die Richtung verwendet, nur für visuelle Pitch-Darstellung
    public LayerMask hitMask;
    public float maxRange = 100f;
    public GameObject tracePrefab;
    public float traceDuration = 0.1f;

    // **KORREKTUR:** Erhält die gespeicherten Schussdaten vom Controller
    public void ShootFromReplay(Vector3 savedMuzzlePosition, Vector3 savedDirection)
    {
        // NUTZT DIE GESPEICHERTEN, KONSISTENTEN WERTE
        Vector3 rayStart = savedMuzzlePosition;
        Vector3 rayDirection = savedDirection;

        // Der Muzzle-Offset-Fix wird nun nicht mehr benötigt, da die Position exakt ist.
        // Falls Sie ihn doch beibehalten wollen, wäre er hier:
        // rayStart += rayDirection * 0.05f; 

        Vector3 hitTarget;
        RaycastHit hit;

        // Wir nutzen die ursprüngliche Reichweite als maximale Range für diesen Raycast
        float currentMaxRange = rayDirection.magnitude > 0 ? maxRange : 0;

        // HINWEIS: Wir müssen hier die Reichweite des Originalschusses verwenden,
        // falls das Original ins Leere geschossen hat (maxRange wurde im Player-Shot begrenzt, 
        // wenn er das Ziel traf oder wenn maxRange erreicht wurde).
        // Um das zu vereinfachen, verwenden wir hier maxRange = 100f, 
        // und speichern stattdessen besser die Max-Range des Schusses im Frame.

        if (Physics.Raycast(rayStart, rayDirection, out hit, maxRange, hitMask))
        {
            hitTarget = hit.point;

            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();

            // DEBUG LOGS
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
                Debug.Log($"[GHOST HIT] Object: {hit.collider.gameObject.name} | Start: {rayStart} | End: {hitTarget} (TREFFER)");
            }
            else
            {
                Debug.Log($"[GHOST HIT] Object: {hit.collider.gameObject.name} | Start: {rayStart} | End: {hitTarget} (UMGEBUNG)");
            }
        }
        else
        {
            hitTarget = rayStart + rayDirection * maxRange;

            // DEBUG LOGS
            Debug.Log($"[GHOST MISSED] Target: Nichts | Start: {rayStart} | End: {hitTarget} (MAX RANGE)");
        }

        if (tracePrefab != null)
        {
            GameObject newTrace = Instantiate(tracePrefab, rayStart, Quaternion.identity);
            newTrace.transform.SetParent(null);

            LineRenderer newLR = newTrace.GetComponent<LineRenderer>();

            if (newLR != null)
            {
                newLR.enabled = true;
                newLR.SetPosition(0, rayStart);
                newLR.SetPosition(1, hitTarget);

                Debug.Log($"[GHOST TRACE POS] Start: {rayStart} | End: {hitTarget} | Range: {Vector3.Distance(rayStart, hitTarget)}");

                StartCoroutine(FadeOutTrace(newTrace));
            }
        }
    }

    private IEnumerator FadeOutTrace(GameObject traceObject)
    {
        yield return new WaitForSeconds(traceDuration);
        if (traceObject != null)
        {
            Destroy(traceObject);
        }
    }
}
using UnityEngine;
using System.Collections; // Hinzugefügt für Coroutine

public class GhostShooter : MonoBehaviour
{
    public Transform muzzleTransform;
    public Transform pitchTarget;      // Der Punkt, der Pitch rotiert (Child des Ghosts)
    public LayerMask hitMask;
    public float maxRange = 100f;
    public GameObject tracePrefab;     // NEU: Trace Prefab für Visualisierung
    public float traceDuration = 0.1f; // NEU: Dauer des Traces

    public void ShootFromReplay()
    {
        // 1. Richtungsvektor berechnen: Die nach vorne gerichtete Richtung des Pitch-Targets
        Vector3 rayDirection = pitchTarget.forward;
        Vector3 rayStart = muzzleTransform.position;
        Vector3 hitTarget;
        RaycastHit hit;

        // 2. Raycast
        if (Physics.Raycast(rayStart, rayDirection, out hit, maxRange, hitMask))
        {
            hitTarget = hit.point;

            // Prüfen, ob der Spieler getroffen wurde (er muss die PlayerHealth-Komponente haben)
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Schaden beim Spieler verursachen
                playerHealth.TakeDamage();
                Debug.Log("Ghost hat Spieler getroffen!");
            }
        }
        else
        {
            // Ray trifft nichts -> Endpunkt ist am Max-Range
            hitTarget = rayStart + rayDirection * maxRange;
        }

        // 3. Trace-Visualisierung (Fehlerstelle behoben)
        if (tracePrefab != null)
        {
            Debug.Log("GhostShooter: Tracer instanziiert!");

            // Instanziiere das Trace-Prefab am Muzzle
            GameObject newTrace = Instantiate(tracePrefab, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);

            newTrace.transform.SetParent(null); // Optional: Damit sich der Trace nicht mit dem Ghost mitbewegt

            LineRenderer newLR = newTrace.GetComponent<LineRenderer>();

            if (newLR != null)
            {
                /*
                // Vektor relativ zum Muzzle berechnen
                Vector3 localHitTarget = muzzleTransform.InverseTransformPoint(hitTarget);
                newLR.enabled = true;
                newLR.SetPosition(0, Vector3.zero);
                newLR.SetPosition(1, localHitTarget);

                // Startet die Coroutine, die zuvor fehlte
                StartCoroutine(FadeOutTrace(newTrace));
                */
                newLR.enabled = true;

                // Setze Startpunkt auf die Weltposition des Muzzles
                newLR.SetPosition(0, rayStart); // rayStart ist bereits muzzleTransform.position

                // Setze Endpunkt auf die Weltposition des Treffers (oder Max-Range)
                newLR.SetPosition(1, hitTarget);

                // Startet die Coroutine
                StartCoroutine(FadeOutTrace(newTrace));
            }
        }
    }

    // NEU: Die fehlende Coroutine aus PlayerShooter übernommen
    private IEnumerator FadeOutTrace(GameObject traceObject)
    {
        yield return new WaitForSeconds(traceDuration);
        if (traceObject != null)
        {
            Destroy(traceObject);
        }
    }
}
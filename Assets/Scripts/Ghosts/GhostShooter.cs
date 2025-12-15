using UnityEngine;
using System.Collections;

public class GhostShooter : MonoBehaviour
{
    // --- ERFORDERLICHE REPLAY-KOMPONENTEN ---
    public Transform muzzleTransform; // Startpunkt des Schusses
    public LayerMask hitMask;        // Welche Objekte getroffen werden können
    public float maxRange = 100f;    // Maximale Reichweite für Raycasting

    // --- TRACER-KOMPONENTEN ---
    public GameObject tracePrefab;   // TrailTracer Prefab mit TracerMovement.cs
    public float traceDuration = 0.1f; // Wird zur Steuerung der Tracer-Dauer verwendet

    // --- ANIMATIONS-KOMPONENTE ---
    // Dies ist das Transform, das die vertikale Neigung (Pitch) der Waffe steuert 
    // und vom GhostController angesprochen wird.
    public Transform pitchTarget;

    // Methode: Führt den Schuss basierend auf aufgezeichneten Daten aus
    // Dies ist die einzige Schusslogik, die der Ghost benötigt.
    public void ShootFromReplay(Vector3 savedMuzzlePosition, Vector3 savedDirection)
    {
        Vector3 rayStart = savedMuzzlePosition;
        Vector3 rayDirection = savedDirection;

        RaycastHit hit;
        Vector3 finalHitTarget;

        // Wir nutzen die ursprüngliche Reichweite als maximale Range für diesen Raycast
        // Die hitMask wird verwendet, um alle potenziellen Ziele (Player, Ghosts, World) zu treffen
        if (Physics.Raycast(rayStart, rayDirection, out hit, maxRange, hitMask))
        {
            finalHitTarget = hit.point;

            // --- ZIELANALYSE und SCHADENSLOGIK ---

            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            // Wir holen die GhostHealth-Komponente, um Ghosts zu identifizieren
            GhostHealth ghostHealth = hit.collider.GetComponent<GhostHealth>();

            if (playerHealth != null)
            {
                // 1. TREFFER: Der Player (zwischengespawnt oder nicht) wurde getroffen
                playerHealth.TakeDamage();
                Debug.Log($"[GHOST REPLAY HIT] Object: {hit.collider.gameObject.name} | Start: {rayStart} | End: {finalHitTarget} (PLAYER TREFFER)");
            }
            else if (ghostHealth != null)
            {
                // 2. TREFFER: Ein Ghost wurde getroffen. 
                // Regel: Ghosts dürfen keine anderen Ghosts töten. Kein Schaden wird angewendet.
                Debug.Log($"[GHOST REPLAY HIT] Object: {hit.collider.gameObject.name} | Ghost Ignoriert. Schaden wird nicht angewendet.");
            }
            else
            {
                // 3. TREFFER: Umgebung oder ein anderes unschädliches Objekt
                Debug.Log($"[GHOST REPLAY HIT] Object: {hit.collider.gameObject.name} | Start: {rayStart} | End: {finalHitTarget} (UMGEBUNG)");
            }
        }
        else
        {
            // KEIN TREFFER: Raycast erreicht maximale Reichweite
            finalHitTarget = rayStart + rayDirection * maxRange;
            Debug.Log($"[GHOST REPLAY MISSED] Target: Nichts | Start: {rayStart} | End: {finalHitTarget} (MAX RANGE)");
        }

        // Die visuelle Spur wird immer bis zum Treffpunkt/Max Range gespawnt
        SpawnTrace(rayStart, finalHitTarget);
    }

    // NEUE HELPER-METHODE ZUR TRACER-GENERIERUNG (nutzt Trail Renderer)
    private void SpawnTrace(Vector3 rayStart, Vector3 hitTarget)
    {
        if (tracePrefab != null)
        {
            GameObject newTrace = Instantiate(tracePrefab);
            newTrace.transform.SetParent(null);

            TracerMovement movement = newTrace.GetComponent<TracerMovement>();

            if (movement != null)
            {
                movement.Initialize(rayStart, hitTarget);
                // Übergibt die Dauer an das TracerMovement-Skript
                movement.destroyDelay = traceDuration;

                Debug.Log($"[GHOST TRACE - TRAIL] Start: {rayStart} | End: {hitTarget} | Range: {Vector3.Distance(rayStart, hitTarget):F6}");
            }
            else
            {
                Debug.LogError("Tracer Prefab is missing the TracerMovement component!");
            }
        }
    }
}
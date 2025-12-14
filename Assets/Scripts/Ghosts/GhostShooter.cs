using UnityEngine;

public class GhostShooter : MonoBehaviour
{
    public Transform muzzleTransform; // Der Schusspunkt
    public Transform pitchTarget;      // NEU: Das Child-Objekt, das nur für den Pitch rotiert wird
    public LayerMask hitMask;
    public float maxRange = 100f; // NEU: Range vom PlayerShooter übernehmen
    public GameObject tracePrefab;

    public void ShootFromReplay()
    {
        // Der Ghost ist bereits auf der richtigen Position (Position) 
        // und horizontal rotiert (transform.rotation).

        // NEU: Richtungsvektor berechnen
        Vector3 rayDirection = pitchTarget.forward;

        // Raycast
        RaycastHit hit;

        if (Physics.Raycast(muzzleTransform.position, rayDirection, out hit, maxRange, hitMask))
        {
            // 1. Trefferpunkt und Trefferobjekt identifizieren
            GhostHealth enemyHealth = hit.collider.GetComponent<GhostHealth>();

            // 2. Wichtig: Der Ghost darf nur den *Player* treffen, nicht andere Ghosts oder sich selbst.
            // Ihre `hitMask` sollte dies bereits filtern, aber hier zur Sicherheit:
            if (enemyHealth == null)
            {
                // Wenn wir den Player treffen wollen, braucht der Player eine PlayerHealth-Komponente.
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // TODO: Spieler-Treffer-Logik (z.B. playerHealth.TakeDamage())
                    Debug.Log("Ghost hat Spieler getroffen!");
                }
            }

            if (tracePrefab != null)
            {
                GameObject newTrace = Instantiate(tracePrefab, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);

                newTrace.transform.SetParent(null);

                LineRenderer newLR = newTrace.GetComponent<LineRenderer>();

                if (newLR != null)
                {
                    Vector3 localHitTarget = muzzleTransform.InverseTransformPoint(hitTarget);
                    newLR.enabled = true;
                    newLR.SetPosition(0, Vector3.zero);
                    newLR.SetPosition(1, localHitTarget);

                    StartCoroutine(FadeOutTrace(newTrace));
                }
            }
        }
    }
}
using UnityEngine;
using System.Collections;

public class GhostShooter : MonoBehaviour
{
    public Transform muzzleTransform;
    public LayerMask hitMask;
    public float maxRange = 100f;
    public GameObject tracePrefab;
    public float traceDuration = 0.1f;
    public Transform pitchTarget;

    public void ShootFromReplay(Vector3 savedMuzzlePosition, Vector3 savedDirection)
    {
        Vector3 rayStart = savedMuzzlePosition;
        Vector3 rayDirection = savedDirection;
        RaycastHit hit;
        Vector3 finalHitTarget;

        if (Physics.Raycast(rayStart, rayDirection, out hit, maxRange, hitMask))
        {
            finalHitTarget = hit.point;
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            GhostHealth ghostHealth = hit.collider.GetComponent<GhostHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
        }
        else
        {
            finalHitTarget = rayStart + rayDirection * maxRange;
        }
        SpawnTrace(rayStart, finalHitTarget);
    }

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
                movement.destroyDelay = traceDuration;
            }
        }
    }
}
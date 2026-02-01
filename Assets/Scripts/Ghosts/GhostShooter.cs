using UnityEngine;
using System.Collections;

public class GhostShooter : MonoBehaviour
{
    //public Transform muzzleTransform;
    public LayerMask hitMask;
    public float maxRange = 100f;
    public float traceDuration = 0.1f;
    //public Transform pitchTarget;

    [Header("Hand Pose")]
    public Transform[] handBones;
    private Quaternion[] savedRotations;

    private void Start()
    {
        // Save Bones
        if (handBones != null)
        {
            savedRotations = new Quaternion[handBones.Length];
            for (int i = 0; i < handBones.Length; i++)
            {
                if (handBones[i] != null)
                    savedRotations[i] = handBones[i].localRotation;
            }
        }
    }

    private void LateUpdate()
    {
        // Force Bone Locations
        if (savedRotations != null)
        {
            for (int i = 0; i < handBones.Length; i++)
            {
                if (handBones[i] != null)
                    handBones[i].localRotation = savedRotations[i];
            }
        }
    }

    public void ShootFromReplay(Vector3 savedMuzzlePosition, Vector3 savedDirection, float savedDistance)
    {
        Vector3 rayStart = savedMuzzlePosition;
        Vector3 rayDirection = savedDirection;
        float rayDistance = savedDistance;
        Vector3 finalHitTarget;

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, rayDistance, hitMask))
        {
            finalHitTarget = hit.point;
            if (hit.collider.TryGetComponent<PlayerHealth>(out var playerHealth)) playerHealth.TakeDamage();
            else if (hit.collider.TryGetComponent<ExplosiveBarrel>(out var barrel)) barrel.TakeHit();
        }
        else
        {
            finalHitTarget = rayStart + rayDirection * rayDistance;
        }
        SpawnTrace(rayStart, finalHitTarget);
    }

    private void SpawnTrace(Vector3 rayStart, Vector3 hitTarget)
    {
        //holt Tracer aus Pool
        GameObject newTrace = BulletPool.Instance.GetBullet();
        if (newTrace.TryGetComponent<TracerMovement>(out var movement))
        {
            movement.Initialize(rayStart, hitTarget); //Bullet aktivieren
            movement.destroyDelay = traceDuration;
        }
    }
}
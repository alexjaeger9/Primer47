using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Transform muzzleTransform;
    public Transform gunTransform;
    public GameObject tracePrefab;
    public float fireRate = 5f;
    public float maxRange = 100f;
    public LayerMask hitMask;
    public float trailDuration = 0.1f;
    [HideInInspector] public bool firedThisTick;
    [HideInInspector] public Vector3 recordedMuzzlePosition;
    [HideInInspector] public Vector3 recordedFireDirection;
    private float lastShotTime;
    private bool isAiming;
    private readonly Vector3 LOCAL_GUN_OFFSET = new Vector3(0.6f, 0f, 0.3f);
    private readonly Vector3 LOCAL_MUZZLE_OFFSET_FROM_GUN = new Vector3(0f, 0f, 1f);

    private void Update()
    {
        HandleAim(Input.GetMouseButton(1));
        HandleShooting(Input.GetMouseButton(0));
    }

    private void HandleAim(bool aimPressed)
    {
        isAiming = aimPressed;
    }

    private void HandleShooting(bool firePressed)
    {
        if (firePressed && Time.time >= lastShotTime + 1f / fireRate)
        {
            lastShotTime = Time.time;
            Shoot();
        }
    }

    private void Shoot()
    {
        firedThisTick = true;
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray cameraRay = mainCamera.ScreenPointToRay(screenCenter);
        Vector3 idealHitTarget;
        RaycastHit cameraHit;
        if (Physics.Raycast(cameraRay, out cameraHit, maxRange, hitMask))
        {
            idealHitTarget = cameraHit.point;
        }
        else
        {
            idealHitTarget = cameraRay.origin + cameraRay.direction * maxRange;
        }
        Vector3 playerWorldPos = transform.position;
        Vector3 gunWorldPos = playerWorldPos + transform.rotation * LOCAL_GUN_OFFSET;
        Vector3 scaledMuzzleOffset = LOCAL_MUZZLE_OFFSET_FROM_GUN;

        scaledMuzzleOffset.x *= gunTransform.localScale.x;
        scaledMuzzleOffset.y *= gunTransform.localScale.y;
        scaledMuzzleOffset.z *= gunTransform.localScale.z;

        Vector3 expectedMuzzleOffsetVector = gunTransform.rotation * scaledMuzzleOffset;
        Vector3 expectedMuzzleWorldPos = gunWorldPos + expectedMuzzleOffsetVector;
        Vector3 actualMuzzlePosUnity = muzzleTransform.position;

        float positionDifference = Vector3.Distance(expectedMuzzleWorldPos, actualMuzzlePosUnity);

        Vector3 rayStart = actualMuzzlePosUnity;
        Vector3 rayDirection = (idealHitTarget - rayStart).normalized;

        float currentRange = Vector3.Distance(rayStart, idealHitTarget);

        recordedMuzzlePosition = rayStart;
        recordedFireDirection = rayDirection;

        Vector3 finalHitTarget;
        RaycastHit hit;
        bool isHit = false;

        if (Physics.Raycast(rayStart, rayDirection, out hit, currentRange, hitMask))
        {
            finalHitTarget = hit.point;
            isHit = true;
            GhostHealth enemyHealth = hit.collider.GetComponent<GhostHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeHit();
            }
        }
        else
        {
            finalHitTarget = idealHitTarget;
            if (cameraHit.collider != null)
            {
                GhostHealth enemyHealth = cameraHit.collider.GetComponent<GhostHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeHit();
                }
            }
        }
        if (tracePrefab != null)
        {
            GameObject newTrace = Instantiate(tracePrefab);
            newTrace.transform.SetParent(null);
            TracerMovement movement = newTrace.GetComponent<TracerMovement>();
            if (movement != null)
            {
                movement.Initialize(rayStart, finalHitTarget);
                movement.destroyDelay = trailDuration;
            }
        }
    }

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }
}
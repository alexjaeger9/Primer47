using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Animator playerAnimator;
    public Transform muzzleTransform;
    public Transform gunTransform;

    public GameObject tracePrefab;
    public float fireRate = 5f;
    public float maxRange = 100f;
    public LayerMask hitMask;
    public float trailDuration = 0.1f;
    private float lastShotTime;
    [HideInInspector] public bool firedThisTick;
    [HideInInspector] public Vector3 recordedMuzzlePosition;
    [HideInInspector] public Vector3 recordedFireDirection;
    [HideInInspector] public bool isAiming; // später für Kamera

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
        Vector3 screenCenter = new(Screen.width / 2, Screen.height / 2, 0);
        Ray cameraRay = mainCamera.ScreenPointToRay(screenCenter);
        Vector3 idealHitTarget;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRange, hitMask))
        {
            idealHitTarget = cameraHit.point;
        }
        else
        {
            idealHitTarget = cameraRay.origin + cameraRay.direction * maxRange;
        }
        Vector3 actualMuzzlePosUnity = muzzleTransform.position;
        Vector3 rayStart = actualMuzzlePosUnity;
        Vector3 rayDirection = (idealHitTarget - rayStart).normalized;

        float currentRange = Vector3.Distance(rayStart, idealHitTarget);

        recordedMuzzlePosition = rayStart;
        recordedFireDirection = rayDirection;

        Vector3 finalHitTarget;

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, currentRange, hitMask))
        {
            finalHitTarget = hit.point;
            if (hit.collider.TryGetComponent<GhostHealth>(out var enemyHealth))
            {
                enemyHealth.TakeHit();
            }
        }
        else
        {
            finalHitTarget = idealHitTarget;
            if (cameraHit.collider != null && cameraHit.collider.TryGetComponent<GhostHealth>(out var enemyHealth))
            {
                enemyHealth.TakeHit();
            }
        }
        if (tracePrefab != null)
        {
            GameObject newTrace = Instantiate(tracePrefab);
            newTrace.transform.SetParent(null);
            if (newTrace.TryGetComponent<TracerMovement>(out var movement))
            {
                movement.Initialize(rayStart, finalHitTarget);
                movement.destroyDelay = trailDuration;
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // Vektorberechnung
        Vector3 weaponAimDirection = mainCamera.transform.forward;
        Vector3 currentAimPosition = mainCamera.transform.position + weaponAimDirection * maxRange;

        if (playerAnimator == null) return;

        float ikWeight = 1.0f;

        // Set Hand IK
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, currentAimPosition);

        // Rotation
        Quaternion targetRotation = Quaternion.LookRotation(weaponAimDirection, mainCamera.transform.up);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, targetRotation);

        // Blickrichtung
        playerAnimator.SetLookAtWeight(ikWeight, 0.8f, 1.0f, 1.0f);
        playerAnimator.SetLookAtPosition(currentAimPosition);
    }

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }
}
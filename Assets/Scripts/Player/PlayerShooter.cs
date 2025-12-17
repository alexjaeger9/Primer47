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

    [HideInInspector] public Vector3 currentAimPosition; // Das globale Ziel
    [SerializeField] private Vector3 rightHandGripOffset = new Vector3(0.5f, 1.3f, 0.1f); // X, Y (Höhe), Z
    public ThirdPersonCamera cameraController;
    [HideInInspector] public Vector3 weaponAimDirection;
    [HideInInspector] public Vector3 handTargetPosition; // NEU: Die Zielposition der Hand (nah am Körper)

    private const float HandDistanceOffset = 10f; // Passt die Reichweite des Arms an


    private void LateUpdate()
    {
        // Die LateUpdate-Methode ist ideal, da die Kamera-Bewegung bereits abgeschlossen ist.
        UpdateAimTarget();
        CalculateWeaponAimDirection();
    }

    private void CalculateWeaponAimDirection()
    {
        // A. Zielpunkt im Raum finden (idealHitTarget)
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

        // B. Ursprung für die ZIELRICHTUNG ist die MÜNDUNG (muzzleTransform)
        Vector3 rayStartForDirection = muzzleTransform.position;

        // C. Die Richtung von der Mündung zum Ziel berechnen (weaponAimDirection)
        weaponAimDirection = (idealHitTarget - rayStartForDirection).normalized;

        // =====================================================================
        // D. IK-POSITION NEU BERECHNEN (OHNE rightHandIKTarget Transform)
        // =====================================================================

        // 1. Lokale Basisposition des Griffs relativ zum Spieler
        // Wir transformieren den Offset (z.B. (0.5, 1.3, 0.1)) in die Weltkoordinaten.
        Vector3 gripBasePosition = transform.position + transform.rotation * rightHandGripOffset;

        // 2. Normalisierung: Die IK-Position wird von der Basis aus entlang der Zielrichtung verschoben.
        handTargetPosition = gripBasePosition + (weaponAimDirection * HandDistanceOffset);
        // **WICHTIG:** Dieser Vektor MUSS zum IK-Zielpunkt werden (IKPosition), 
        // um den Arm an diese normalisierte Position zu zwingen.
    }

    private void UpdateAimTarget()
    {
        // 1. Raycast von der Kameramitte, um das ZIEL zu finden (currentAimPosition)
        Vector3 screenCenter = new(Screen.width / 2, Screen.height / 2, 0);
        Ray cameraRay = mainCamera.ScreenPointToRay(screenCenter);

        // Die maximale Reichweite des Ziels
        if (Physics.Raycast(cameraRay, out RaycastHit hit, maxRange, hitMask))
        {
            currentAimPosition = hit.point;
        }
        else
        {
            currentAimPosition = cameraRay.origin + cameraRay.direction * maxRange;
        }

        // ACHTUNG: Wir führen HIER KEINE Rotation des rightHandIKTarget mehr durch!
        // rightHandIKTarget dient nur noch als Positionshalter (Offset) für die Hand.
    }


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
            if (cameraHit.collider != null)
            {
                if (cameraHit.collider.TryGetComponent<GhostHealth>(out var enemyHealth))
                {
                    enemyHealth.TakeHit();
                }
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

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }
}
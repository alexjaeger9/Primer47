using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Transform muzzleTransform; // Für den Schussstartpunkt
    public Transform gunTransform;    // Für die Waffe selbst

    // Nur noch Variablen für den Schuss-Effekt/Timing
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

    // --- VEREINFACHTE IK/AIMING VARIABLEN ---
    // Definiert den lokalen Griffpunkt relativ zum Spieler-Pivot.
    [SerializeField] private Vector3 rightHandGripOffset = new Vector3(0.5f, 1.3f, 0.1f);

    // Die weite Distanz, die den Arm zwingt, sich auszustrecken.
    private const float HandDistanceOffset = 10f;

    [HideInInspector] public Vector3 weaponAimDirection;
    [HideInInspector] public Vector3 handTargetPosition;
    [HideInInspector] public Vector3 currentAimPosition; // Das globale Ziel für LookAt/Shoot-Raycast
    // ------------------------------------------

    public ThirdPersonCamera cameraController;

    private void LateUpdate()
    {
        CalculateAimingVectors();
    }

    private void CalculateAimingVectors()
    {
        // 1. ZIELRICHTUNG (weaponAimDirection)
        // Die Zielrichtung der Waffe ist exakt die Vorwärtsrichtung der Kamera.
        weaponAimDirection = mainCamera.transform.forward;


        // 2. IK-POSITION (handTargetPosition)

        // A. Finde die Basisposition des Griffs im Weltraum (transform.position + Lokaler Offset)
        Vector3 gripBasePosition = transform.position + transform.rotation * rightHandGripOffset;

        // B. Berechne den IK-Zielpunkt: Vom Griff entlang der Zielrichtung um die definierte Distanz.
        // Dieser Punkt ist stabil und weit entfernt, um die Armrotation zu steuern.
        handTargetPosition = gripBasePosition + (weaponAimDirection * HandDistanceOffset);


        // 3. GLOBALES ZIEL (currentAimPosition)
        // Dies ist der Punkt in der Welt, auf den der Spieler schießt und hinsieht (für LookAt).
        // Wir setzen ihn einfach 100 Meter entfernt entlang der Kamera-Richtung.
        currentAimPosition = mainCamera.transform.position + weaponAimDirection * maxRange;
    }

    // ... Update, HandleAim, HandleShooting, ResetTickFlags ...

    // Die Shoot() Methode benötigt weiterhin Raycasting, um den Treffer zu registrieren, 
    // verwendet aber die hier berechneten Richtungen und currentAimPosition.
    // DIESE LOGIK BLEIBT SO WIE SIE IST, da Sie einen Treffer registrieren wollen.
    // Die Änderung ist, dass die Richtung aus CalculateAimingVectors kommt.

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

    // HIER NUR DER ANFANG DER SHOOT-METHODE, UM ZU ZEIGEN, DASS NUR EINE RAYCASTING-METHODE VERWENDET WIRD.
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
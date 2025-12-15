using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Transform muzzleTransform;
    public Transform gunTransform;

    // NEU: traceDuration wird für TracerMovement.destroyDelay NICHT direkt benötigt
    // WICHTIG: tracePrefab muss jetzt das TrailTracer-Prefab sein
    public GameObject tracePrefab;

    public float fireRate = 5f;
    public float maxRange = 100f;
    public LayerMask hitMask;
    public float traceDuration = 0.1f; // HIER BEIBEHALTEN, WENN AUCH FÜR ANDERE EFFEKTE GENUTZT

    [HideInInspector] public bool firedThisTick;
    [HideInInspector] public Vector3 recordedMuzzlePosition;
    [HideInInspector] public Vector3 recordedFireDirection;

    private float lastShotTime;
    private bool isAiming;

    // Konstante lokale Offsets
    private readonly Vector3 LOCAL_GUN_OFFSET = new Vector3(0.6f, 0f, 0.3f);
    private readonly Vector3 LOCAL_MUZZLE_OFFSET_FROM_GUN = new Vector3(0f, 0f, 1f);


    private void Update()
    {
        if (mainCamera == null || muzzleTransform == null || gunTransform == null)
        {
            Debug.LogError("PlayerShooter: Missing required Transform assignments (mainCamera, muzzleTransform, or gunTransform).");
            return;
        }

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

        Debug.Log($"[SHOOTER] Schuss abgefeuert in Frame: {Time.frameCount} | Time: {Time.time:F3}. firedThisTick=TRUE.");

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray cameraRay = mainCamera.ScreenPointToRay(screenCenter);

        // --- PHASE 1: ZIELPUNKT (idealHitTarget) ÜBER DIE KAMERA FINDEN ---
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

        // ----------------------------------------------------------------------
        // DEBUG / MANUELLE BERECHNUNG (BEIBEHALTEN)
        // ----------------------------------------------------------------------
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

        Debug.Log("---------------------------------------------------");
        Debug.Log($"[DEBUG POS] Player World Pos: {playerWorldPos}");
        Debug.Log($"[DEBUG POS] Gun Rotation (Pitch X): {gunTransform.rotation.eulerAngles.x:F2}°");
        Debug.Log($"[DEBUG POS] Gun Local Scale: {gunTransform.localScale}");
        Debug.Log($"[DEBUG POS] Expected Muzzle World Pos (Manual): {expectedMuzzleWorldPos}");
        Debug.Log($"[DEBUG POS] Actual Muzzle World Pos (Unity): {actualMuzzlePosUnity}");

        if (positionDifference > 0.005f)
        {
            Debug.LogError($"[FEHLER!] ERNSTHAFTE ABWEICHUNG DES MUZZLE-PUNKTES! Differenz: {positionDifference:F4} m.");
        }
        Debug.Log("---------------------------------------------------");


        // ----------------------------------------------------------------------
        // PHASE 2: RAYCAST VOM MUZZLE ZUM ZIELPUNKT (actualHit)
        // ----------------------------------------------------------------------

        Vector3 rayStart = actualMuzzlePosUnity;
        Vector3 rayDirection = (idealHitTarget - rayStart).normalized;
        float currentRange = Vector3.Distance(rayStart, idealHitTarget);

        // SPEICHERUNG für den Recorder
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

        // ----------------------------------------------------------------------
        // TRACER (Trail Renderer) - NEUE LOGIK
        // ----------------------------------------------------------------------
        if (tracePrefab != null)
        {
            GameObject newTrace = Instantiate(tracePrefab);
            newTrace.transform.SetParent(null);

            TracerMovement movement = newTrace.GetComponent<TracerMovement>();

            if (movement != null)
            {
                // Initialisiere das TracerMovement-Skript
                movement.Initialize(rayStart, finalHitTarget);

                // Setze die Zerstörungsverzögerung basierend auf der Shooter-Einstellung (optional)
                movement.destroyDelay = traceDuration;

                Debug.Log($"[PLAYER TRACE - TRAIL] Start: {rayStart} | End: {finalHitTarget} | Range: {Vector3.Distance(rayStart, finalHitTarget):F6}");
            }
            else
            {
                Debug.LogError("Tracer Prefab is missing the TracerMovement component!");
            }
        }

        // --- LOGGING ---
        if (isHit)
        {
            Debug.Log($"[PLAYER HIT] Target: {hit.collider.gameObject.name} | Start: {rayStart} | End: {finalHitTarget}");
        }
        else if (currentRange < maxRange)
        {
            Debug.Log($"[PLAYER HIT] Target: Zielpunkt erreicht | Start: {rayStart} | End: {finalHitTarget}");
        }
        else
        {
            Debug.Log($"[PLAYER MISSED/CLEARED] Target: Nichts | Start: {rayStart} | End: {finalHitTarget}");
        }
    }

    // Coroutine für LineRenderer wurde entfernt, da TracerMovement das übernimmt.
    // private IEnumerator FadeOutTrace(GameObject traceObject) { ... } 

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }
}
using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Transform muzzleTransform;

    //  NEU: Referenz auf das Gun-GameObject (Muss im Inspector zugewiesen werden)
    public Transform gunTransform;

    public GameObject tracePrefab;
    public float fireRate = 5f;
    public float maxRange = 100f;
    public LayerMask hitMask;
    public float traceDuration = 0.1f;

    [HideInInspector] public bool firedThisTick;
    [HideInInspector] public Vector3 recordedMuzzlePosition;
    [HideInInspector] public Vector3 recordedFireDirection;

    private float lastShotTime;
    private bool isAiming;

    // Konstante lokale Offsets (Dies sind die korrekten Editor-Werte gemäß Ihrer Angabe)
    // Wenn die lokale Position der GUN relativ zum Player (0.6, 0, 0.3) ist
    private readonly Vector3 LOCAL_GUN_OFFSET = new Vector3(0.6f, 0f, 0.3f);
    // Wenn die lokale Position der MUZZLE relativ zur GUN (0, 0, 1) ist
    private readonly Vector3 LOCAL_MUZZLE_OFFSET_FROM_GUN = new Vector3(0f, 0f, 1f);


    private void Update()
    {
        // Wir fügen den Gun Transform Check hinzu, da er für die Berechnung benötigt wird.
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
        //  DEBUG / MANUELLE BERECHNUNG DER WELTPOSITION (KORRIGIERT FÜR SKALIERUNG)
        // ----------------------------------------------------------------------

        Vector3 playerWorldPos = transform.position;

        // 1. Gun World Pos
        Vector3 gunWorldPos = playerWorldPos + transform.rotation * LOCAL_GUN_OFFSET;

        // 2. Muzzle Offset Vektor, SKALIERT und ROTIERT
        // Da die Skalierung 0.2, 0.2, 0.2 ist, müssen wir den lokalen Muzzle-Offset entsprechend skalieren.

        Vector3 scaledMuzzleOffset = LOCAL_MUZZLE_OFFSET_FROM_GUN;

        // Anwendung der lokalen Skalierung der Gun auf den lokalen Muzzle-Offset.
        // Da LOCAL_MUZZLE_OFFSET_FROM_GUN (0, 0, 1) ist, beeinflusst nur die Z-Skalierung den Offset.
        scaledMuzzleOffset.x *= gunTransform.localScale.x;
        scaledMuzzleOffset.y *= gunTransform.localScale.y;
        scaledMuzzleOffset.z *= gunTransform.localScale.z;

        // Wende die Gun-Rotation (Pitch) auf den SKALIERTEN Offset an
        Vector3 expectedMuzzleOffsetVector = gunTransform.rotation * scaledMuzzleOffset;

        // Finale erwartete Weltposition
        Vector3 expectedMuzzleWorldPos = gunWorldPos + expectedMuzzleOffsetVector;

        // Die tatsächliche Position, die Unity durch die Hierarchie liefert
        Vector3 actualMuzzlePosUnity = muzzleTransform.position;

        // --- LOGGING ---
        float positionDifference = Vector3.Distance(expectedMuzzleWorldPos, actualMuzzlePosUnity);

        Debug.Log("---------------------------------------------------");
        Debug.Log($"[DEBUG POS] Player World Pos: {playerWorldPos}");
        Debug.Log($"[DEBUG POS] Gun Rotation (Pitch X): {gunTransform.rotation.eulerAngles.x:F2}°");
        Debug.Log($"[DEBUG POS] Gun Local Scale: {gunTransform.localScale}");
        Debug.Log($"[DEBUG POS] Expected Muzzle World Pos (Manual): {expectedMuzzleWorldPos}");
        Debug.Log($"[DEBUG POS] Actual Muzzle World Pos (Unity): {actualMuzzlePosUnity}");

        if (positionDifference > 0.005f) // Toleranz von 5mm
        {
            Debug.LogError($"[FEHLER!] ERNSTHAFTE ABWEICHUNG DES MUZZLE-PUNKTES! Differenz: {positionDifference:F4} m.");
            Debug.LogError("Die Skalierung wurde in der Berechnung berücksichtigt. Falls die Abweichung bleibt: 1) Prüfen Sie die konstanten Offsets, 2) Prüfen Sie auf Skripte, die die Muzzle-Position in LateUpdate manipulieren.");
        }
        Debug.Log("---------------------------------------------------");


        // ----------------------------------------------------------------------
        // PHASE 2: RAYCAST VOM MUZZLE ZUM ZIELPUNKT (actualHit)
        // ----------------------------------------------------------------------

        // Wir verwenden die tatsächliche Weltposition, die Unity für das Muzzle-Transform berechnet.
        Vector3 rayStart = actualMuzzlePosUnity;

        Vector3 rayDirection = (idealHitTarget - rayStart).normalized;
        float currentRange = Vector3.Distance(rayStart, idealHitTarget);

        // **KORREKTUR/SPEICHERUNG:** Bereitstellen der exakten Schussdaten für den Recorder
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
        // DEBUG LOGS
        // ----------------------------------------------------------------------
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

        // ----------------------------------------------------------------------
        // TRACER (Weltkoordinaten)
        // ----------------------------------------------------------------------
        if (tracePrefab != null)
        {
            GameObject newTrace = Instantiate(tracePrefab, rayStart, Quaternion.identity);
            newTrace.transform.SetParent(null);
            LineRenderer newLR = newTrace.GetComponent<LineRenderer>();

            if (newLR != null)
            {
                newLR.enabled = true;
                newLR.SetPosition(0, rayStart);
                newLR.SetPosition(1, finalHitTarget);
                Debug.Log($"[TRACE OBJECT POS] Start Pos: {rayStart} | Tracer World Pos: {newTrace.transform.position}");

                Debug.Log($"[PLAYER TRACE POS] Start: {rayStart} | End: {finalHitTarget} | Range: {Vector3.Distance(rayStart, finalHitTarget):F6}");

                StartCoroutine(FadeOutTrace(newTrace));
            }
        }
    }

    private IEnumerator FadeOutTrace(GameObject traceObject)
    {
        yield return new WaitForSeconds(traceDuration);
        if (traceObject != null)
        {
            Destroy(traceObject);
        }
    }

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }
}
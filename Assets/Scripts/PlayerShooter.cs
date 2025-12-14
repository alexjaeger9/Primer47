using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Transform muzzleTransform;
    public GameObject tracePrefab;
    public float fireRate = 5f;
    public float maxRange = 100f;
    public LayerMask hitMask;
    public float traceDuration = 0.1f;
    [HideInInspector] public bool firedThisTick;
    private float lastShotTime;
    private bool isAiming;

    private void Update()
    {
        if (mainCamera == null || muzzleTransform == null) return;

        HandleAim(Input.GetMouseButton(1));
        HandleShooting(Input.GetMouseButton(0));
    }

    private void HandleAim(bool aimPressed)
    {
        isAiming = aimPressed;
    }

    private void HandleShooting(bool firePressed)
    {
        firedThisTick = false;

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
        Vector3 hitTarget;
        RaycastHit hit;

        if (Physics.Raycast(cameraRay, out hit, maxRange, hitMask))
        {
            hitTarget = hit.point;

            GhostHealth enemyHealth = hit.collider.GetComponent<GhostHealth>();

            // 2. Wenn die Komponente gefunden wurde (d.h. wir haben einen Gegner getroffen)
            if (enemyHealth != null)
            {
                // 3. Rufe die Methode auf, um den Treffer zu registrieren
                enemyHealth.TakeHit();
            }
        }
        else
        {
            hitTarget = cameraRay.origin + cameraRay.direction * maxRange;
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
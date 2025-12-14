using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    // Die Kamera-Referenz wird benötigt, da der Raycast von hier startet.
    public Camera mainCamera;

    // Die Position, von der die Kugel tatsächlich startet (Laufmündung).
    public Transform muzzleTransform;

    // Allgemeine Schuss-Parameter
    public float fireRate = 5f;
    public LayerMask hitMask;
    public float bulletSpeed = 100f; // Geschwindigkeit des simulierten Raycasts (kann hoch sein)
    public float maxRange = 100f;    // Maximale Reichweite

    [HideInInspector] public bool firedThisTick;

    private float lastShotTime;
    private bool isAiming;

    // --- Unity Life Cycle ---

    private void Update()
    {
        // Sicherstellen, dass die Kamera-Referenz gesetzt ist
        if (mainCamera == null) return;

        // Handhabung von Zielen (Rechte Maustaste)
        HandleAim(Input.GetMouseButton(1));

        // Handhabung des Schießens (Linke Maustaste)
        HandleShooting(Input.GetMouseButton(0));
    }

    // --- Logik-Funktionen ---

    private void HandleAim(bool aimPressed)
    {
        isAiming = aimPressed;
        // Optional: Hier könnten Sie das FOV der Kamera oder die Empfindlichkeit ändern
    }

    private void HandleShooting(bool firePressed)
    {
        firedThisTick = false;

        // Ist die Feuertaste gedrückt UND ist die Wartezeit abgelaufen?
        if (firePressed && Time.time >= lastShotTime + 1f / fireRate)
        {
            lastShotTime = Time.time;
            Shoot();
        }
    }

    private void Shoot()
    {
        firedThisTick = true;

        // 1. **ZIEL-RAYCAST (Line of Sight) von der Kamera**
        // Raycast startet in der Mitte des Bildschirms.
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray cameraRay = mainCamera.ScreenPointToRay(screenCenter);
        Vector3 targetPoint;

        RaycastHit hit;

        // Versuchen, ein Ziel zu treffen (wobei der Player ignoriert wird, falls in LayerMask enthalten)
        if (Physics.Raycast(cameraRay, out hit, maxRange, hitMask))
        {
            // Raycast hat etwas getroffen: Das ist unser präzises Ziel
            targetPoint = hit.point;

            // TODO: Optional: Hier könnten Sie Schaden zufügen (hit.collider.GetComponent<IDamageable>())
            Debug.Log("Getroffen: " + hit.collider.name + " an Punkt: " + targetPoint);
        }
        else
        {
            // Raycast hat nichts getroffen: Das Ziel ist weit entfernt (maxRange)
            targetPoint = cameraRay.origin + cameraRay.direction * maxRange;
            Debug.Log("Nichts getroffen, Zielpunkt auf maxRange gesetzt.");
        }


        // 2. **KUGEL-RICHTUNG BERECHNEN**
        // Die tatsächliche Richtung der Kugel geht von der Laufmündung zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzleTransform.position).normalized;


        // 3. **VISUELLE KUGEL / TRACE / PROJECTILE STARTEN**

        // Der Einfachheit halber simulieren wir hier einen weiteren Raycast ODER starten ein Projektil.
        // FÜR JETZT: Führen wir einen Raycast von der Mündung durch (Instant Hitscan):

        // Wir setzen einen neuen Raycast von der Mündung (muzzleTransform)
        if (Physics.Raycast(muzzleTransform.position, shootDirection, out hit, maxRange, hitMask))
        {
            // Die Kugel trifft das, was das Fadenkreuz anvisiert!
            Debug.DrawRay(muzzleTransform.position, shootDirection * maxRange, Color.red, 2f);

            // TODO: Optional: Visuellen Bullet-Trace (Trail) zeichnen
            // TODO: Optional: Einschlag-Effekt (Decal, Partikel) bei hit.point
        }
    }

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }
}
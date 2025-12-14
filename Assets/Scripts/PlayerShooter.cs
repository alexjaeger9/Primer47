using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Transform muzzleTransform;
    public float fireRate = 5f;
    public LayerMask hitMask;

    [HideInInspector] public bool firedThisTick;

    private float lastShotTime;
    private bool isAiming;

    private void Update()
    {
        HandleAim(Input.GetMouseButton(1));
        HandleShooting(Input.GetMouseButton(0));
    }

    private void HandleAim(bool aimPressed)
    {
        // TODO: Aim-Status setzen, ggf. FOV/Empfindlichkeit ändern
        isAiming = aimPressed;
    }

    private void HandleShooting(bool firePressed)
    {
        // TODO: Feuerrate prüfen, ggf. Shoot aufrufen
    }

    private void Shoot()
    {
        // TODO: Raycast / Projektil, Schaden auf IDamageable
        firedThisTick = true;
    }

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }
}

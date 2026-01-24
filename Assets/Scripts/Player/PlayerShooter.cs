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
    [SerializeField] private GameObject muzzleParticle;
    [SerializeField] private GameObject bulletHitParticle;
    private float lastShotTime;
    [HideInInspector] public bool firedThisTick;
    [HideInInspector] public Vector3 recordedMuzzlePosition;
    [HideInInspector] public Vector3 recordedFireDirection;
    [HideInInspector] public float recordedFireDistance;
    [HideInInspector] public bool isAiming; // sp�ter f�r Kamera

    [HideInInspector] private Vector3 handRotationOffset = new Vector3(0, 0, -90);

    [Header("Hand Pose")]
    public Transform[] handBones;
    private Quaternion[] savedRotations;

    private void Start()
    {
        // Speichere die Pose von allen zugewiesenen Knochen
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

    private void Update()
    {
        HandleAim(Input.GetMouseButton(1));
        HandleShooting(Input.GetMouseButton(0));
    }

    private void LateUpdate()
    {
        // Erzwinge die gespeicherte Pose �ber jede Animation dr�ber
        if (savedRotations != null)
        {
            for (int i = 0; i < handBones.Length; i++)
            {
                if (handBones[i] != null)
                    handBones[i].localRotation = savedRotations[i];
            }
        }
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
        SpawnMuzzleFlash();
        Vector3 screenCenter = new(Screen.width / 2, Screen.height / 2, 0);
        Ray cameraRay = mainCamera.ScreenPointToRay(screenCenter);
        Vector3 idealHitTarget;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRange, hitMask))
        {
            idealHitTarget = cameraHit.point;
            SpawnShockwave(cameraHit.point);
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
        recordedFireDistance = currentRange;

        Vector3 finalHitTarget;

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, currentRange, hitMask))
        {
            finalHitTarget = hit.point;
            SpawnShockwave(cameraHit.point);
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

        //Bullet aus Pool holen
        GameObject newTrace = BulletPool.Instance.GetBullet();
        if (newTrace.TryGetComponent<TracerMovement>(out var movement))
        {
            movement.destroyDelay = trailDuration;
            movement.Initialize(rayStart, finalHitTarget); //Initialize aktiviert das Bullet
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
        Quaternion correction = Quaternion.Euler(handRotationOffset);
        Quaternion finalHandRotation = targetRotation * correction;
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, finalHandRotation);

        // Blickrichtung
        playerAnimator.SetLookAtWeight(ikWeight, 0.8f, 1.0f, 1.0f);
        playerAnimator.SetLookAtPosition(currentAimPosition);
    }

    public void ResetTickFlags()
    {
        firedThisTick = false;
    }

    private void SpawnMuzzleFlash()
    {
            // Spawne am Muzzle (Gun-Ende)
            GameObject effect = Instantiate(
                muzzleParticle, 
                muzzleTransform.position, 
                muzzleTransform.rotation  // Zeigt nach vorne (Gun-Richtung)
            );
            Destroy(effect, 1f);
    }

    private void SpawnShockwave(Vector3 position)
    {
        // Billboard: Quaternion.identity (schaut automatisch zur Kamera)
        GameObject shockwave = Instantiate(
            bulletHitParticle, 
            position, 
            Quaternion.identity
        );

        if (mainCamera != null)
        {
            Vector3 directionToCamera = mainCamera.transform.position - position;
            shockwave.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
        Destroy(shockwave, 1f);
    }
}
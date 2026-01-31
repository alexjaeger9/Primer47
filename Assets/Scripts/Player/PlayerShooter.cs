using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    public Camera mainCamera;
    public Animator playerAnimator;
    public Transform muzzleTransform;
    public Transform gunTransform;

    [Header("Audio Settings")] 
    public AudioSource gunAudioSource; 
    public AudioClip gunshotClip;      
    [Range(0f, 0.5f)] public float pitchVariation = 0.1f; 

    [Header("VFX")]
    public GameObject tracePrefab;
    public GameObject impactPrefab;

    public float fireRate = 5f;
    public float maxRange = 100f;
    public LayerMask hitMask;
    public float trailDuration = 0.1f;
    private float lastShotTime;
    [HideInInspector] public bool firedThisTick;
    [HideInInspector] public Vector3 recordedMuzzlePosition;
    [HideInInspector] public Vector3 recordedFireDirection;
    [HideInInspector] public float recordedFireDistance;
    [HideInInspector] public bool isAiming; 

    [HideInInspector] private Vector3 handRotationOffset = new Vector3(0, 0, -90);

    [Header("Hand Pose")]
    public Transform[] handBones;
    private Quaternion[] savedRotations;

    public MuzzleFlash muzzleFlash;

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
        if (Time.timeScale == 0f) return;
        HandleAim(Input.GetMouseButton(1));
        HandleShooting(Input.GetMouseButton(0));
    }

    private void LateUpdate()
    {
        // Erzwinge die gespeicherte Pose über jede Animation drüber
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
        lastShotTime = Time.time;

        if (muzzleFlash != null) muzzleFlash.PlayFlash();

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        if (gunAudioSource != null && gunshotClip != null)
        {
            gunAudioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            gunAudioSource.PlayOneShot(gunshotClip);
        }
        
        Ray cameraRay = mainCamera.ScreenPointToRay(screenCenter);

        Vector3 rayStart = muzzleTransform.position;
        Vector3 targetWorldPoint;

        // Zielpunkt ermitteln
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRange, hitMask))
        {
            Vector3 dirToHitFromMuzzle = (cameraHit.point - rayStart).normalized;
            float dot = Vector3.Dot(mainCamera.transform.forward, dirToHitFromMuzzle);

            // Ist Punkt im Sichtfeld?
            if (dot > -0.2f) targetWorldPoint = cameraHit.point;
            else targetWorldPoint = rayStart + mainCamera.transform.forward * maxRange;
        }
        else targetWorldPoint = rayStart + mainCamera.transform.forward * maxRange;

        // Richtung und Distanz
        Vector3 fireDirection = (targetWorldPoint - rayStart).normalized;
        Vector3 finalHitTarget = targetWorldPoint;
        float currentRange = maxRange;

        // Echter Schuss von Muzzle
        if (Physics.Raycast(rayStart, fireDirection, out RaycastHit weaponHit, maxRange, hitMask))
        {
            finalHitTarget = weaponHit.point;
            currentRange = weaponHit.distance;
            
            if (impactPrefab != null)
            {
                Instantiate(impactPrefab, weaponHit.point, Quaternion.LookRotation(weaponHit.normal));
            }

            // Wir prüfen nach Komponenten nur wenn wir sie finden
            if (weaponHit.collider.TryGetComponent<GhostHealth>(out var enemyHealth)) 
            {
                enemyHealth.TakeHit();
            }
            else if (weaponHit.collider.TryGetComponent<ExplosiveBarrel>(out var barrel)) 
            {
                barrel.TakeHit();
            }
        }
        else currentRange = Vector3.Distance(rayStart, targetWorldPoint);

        // Aufnahme für das Ghost-System
        recordedMuzzlePosition = rayStart;
        recordedFireDirection = fireDirection;
        recordedFireDistance = currentRange;

        // Bullet Tracer
        GameObject newTrace = BulletPool.Instance.GetBullet();
        if (newTrace.TryGetComponent<TracerMovement>(out var movement))
        {
            movement.Initialize(rayStart, finalHitTarget); 
            movement.destroyDelay = trailDuration;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (playerAnimator == null) return;

        // Vektorberechnung
        Vector3 weaponAimDirection = mainCamera.transform.forward;
        Vector3 currentAimPosition = mainCamera.transform.position + weaponAimDirection * maxRange;

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
}
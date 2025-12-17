using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float mouseSensitivity = 200f;
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private float yaw;
    [HideInInspector] public bool jumpedThisTick;
    public Animator playerAnimator;

    private PlayerShooter playerShooter; // <== Neu!
    private Transform rightHandBone;

    void OnAnimatorIK(int layerIndex)
    {
        if (playerAnimator == null || playerShooter == null)
        {
            return;
        }

        float ikWeight = 1.0f;

        // --- Werte aus PlayerShooter abrufen ---
        // ikPosition ist jetzt der normalisierte Punkt (HandDistanceOffset entfernt vom Griff)
        Vector3 ikPosition = playerShooter.handTargetPosition;
        Vector3 aimDirection = playerShooter.weaponAimDirection;

        // =======================================================
        // 1. IK BERECHNUNG & ANWENDEN
        // =======================================================

        // Rotation: Hand dreht sich in die präzise Zielrichtung.
        Vector3 desiredForward = aimDirection;
        Vector3 desiredRightHandUp = playerShooter.mainCamera.transform.up;
        Quaternion targetRotation = Quaternion.LookRotation(desiredForward, desiredRightHandUp);


        // IK POSITION: Die IK Position ist der normalisierte Punkt (handTargetPosition).
        // Dies zwingt den ARM zur korrigierten Position im Nahbereich.
        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, ikPosition);
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);

        // IK ROTATION: Die Hand dreht sich, um die Waffe auszurichten.
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, targetRotation);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);


        // =======================================================
        // 2. WAFFE BINDEN (An die Hand gebunden)
        // =======================================================
        if (playerShooter.gunTransform != null)
        {
            // 1. Hole die aktuelle (durch IK verschobene) Handposition des Models.
            Transform rightHandBone = playerAnimator.GetBoneTransform(HumanBodyBones.RightHand);

            // 2. POSITION: Waffe folgt der Handposition.
            playerShooter.gunTransform.position = rightHandBone.position;

            // 3. ROTATION: Waffe folgt der berechneten IK-Rotation.
            playerShooter.gunTransform.rotation = targetRotation;
        }

        // =======================================================
        // KOPF
        // =======================================================
        playerAnimator.SetLookAtWeight(ikWeight, 0.3f, 0.5f, 0.5f);
        playerAnimator.SetLookAtPosition(playerShooter.currentAimPosition);
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;
        playerShooter = GetComponent<PlayerShooter>(); // <== Hinzufügen!
        if (playerAnimator != null)
        {
            rightHandBone = playerAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        }
    }

    void Update()
    {
        HandleRotation();
        HandleMovementInput();
        HandleGravityAndJump();
        HandleAnimation();
        jumpedThisTick = false;
        Vector3 finalMovement = (moveDirection * moveSpeed) + new Vector3(0, velocity.y, 0);
        controller.Move(finalMovement * Time.deltaTime);
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        yaw += mouseX * mouseSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new(horizontal, 0f, vertical);
        if (inputDir.sqrMagnitude < 0.001f)
        {
            moveDirection = Vector3.zero;
            return;
        }
        moveDirection = transform.rotation * inputDir.normalized;
    }

    void HandleAnimation()
    {
        if (playerAnimator == null) return;

        // 1. Die rohen Input-Werte holen (wie in HandleMovementInput)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 2. Bewegungsparameter im Animator setzen
        // Diese Werte steuern den 2D Blend Tree
        playerAnimator.SetFloat("MoveX", horizontal);
        playerAnimator.SetFloat("MoveY", vertical);

        //Debug.Log("MoveX: " + horizontal + " MoveY: "+vertical);



        /*
        // 3. Sprung/Fall-Animationen (Logik aus HandleGravityAndJump)
        bool grounded = controller.isGrounded;

        // Verwenden Sie die Sprung- und Fall-Logik aus dem vorherigen Schritt
        if (!grounded)
        {
            // Wenn man fällt oder hochsteigt
            playerAnimator.SetBool("IsFalling", true);
        }
        else
        {
            playerAnimator.SetBool("IsFalling", false);
              if (jumpedThisTick)
            {
            // Trigger für den Sprung
            playerAnimator.SetTrigger("JumpTrigger");
            }
            
        }*/

        // Beachten Sie, dass Sie hier die `jumpedThisTick` Flagge aus Ihrem Code nutzen!
    }

    void HandleGravityAndJump()
    {
        bool grounded = controller.isGrounded;
        if (grounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpForce;
                jumpedThisTick = true;
                playerAnimator.SetTrigger("JumpTrigger");
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }
}
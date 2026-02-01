using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Animator playerAnimator;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Footsteps")]
    public AudioClip[] footstepClips;

    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] public float mouseSensitivity = 200f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private float yaw;
    private float currentSpeed;
    [HideInInspector] public bool jumpedThisTick;
    [HideInInspector] public bool landedThisTick;
    [HideInInspector] public bool slidingThisTick;

    private Vector3 boostVelocity;
    private float boostVelocityDecay = 2f;

    private bool isSprintingLocked = false;

    private bool landed = false;
    private int currentStepIndex = 0;

    [Header("ADS Settings")]
    [Range(0.1f, 1f)] public float aimSensitivityMultiplier = 0.5f;
    private bool isAiming = false;

    [Header("Slide Settings")]
    [SerializeField] private float slideBoostForce = 8f; 
    [SerializeField] private float slideCooldown = 0.5f; 
    private float lastSlideTime = 0f;

    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;
        currentSpeed = runSpeed;
    }

    void Update()
    {
        HandleRotation();
        HandleMovementInput();
        HandleGravityAndJump();
        HandleAnimation();

        // Boost Velocity abbauen
        if (boostVelocity.magnitude > 0.1f)
        {
            boostVelocity = Vector3.Lerp(boostVelocity, Vector3.zero, boostVelocityDecay * Time.deltaTime);
        }
        else
        {
            // Auf exakt 0 setzen wenn sehr klein
            boostVelocity = Vector3.zero;
        }

        Vector3 finalMovement = (moveDirection * currentSpeed) + boostVelocity + new Vector3(0, velocity.y, 0);
        controller.Move(finalMovement * Time.deltaTime);
    }

    public void SetAiming(bool state)
    {
        isAiming = state;
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float sensitivityMultiplier = isAiming ? aimSensitivityMultiplier : 1.0f;
        
        yaw += mouseX * mouseSensitivity * sensitivityMultiplier * Time.deltaTime;
        
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (controller.isGrounded)
        {
            bool isSprinting = false;
            if (vertical >= 0f) isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? sprintSpeed : runSpeed;
            isSprintingLocked = isSprinting;
        }
        else
        {
            if (!Input.GetKey(KeyCode.LeftShift)) isSprintingLocked = false;
            currentSpeed = isSprintingLocked ? sprintSpeed : runSpeed;
        }

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical);
        if (inputDir.sqrMagnitude < 0.001f)
        {
            moveDirection = Vector3.zero;
            return;
        }
        moveDirection = transform.rotation * inputDir.normalized;
    }

    void HandleGravityAndJump()
    {
        bool grounded = controller.isGrounded;
        bool nearGround = false;
        
        if (!grounded && velocity.y < 0) nearGround = Physics.Raycast(transform.position, Vector3.down, 1.25f);

        if (grounded || nearGround)
        {
            if (playerAnimator.GetBool("isFalling") && !landed)
            {
                playerAnimator.SetBool("isFalling", false);
                playerAnimator.SetTrigger("Land");
                landedThisTick = true;
                landed = true;
                PlaySound(landClip);
                //Debug.Log("Land");
            } 
            else if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("JumpTag") && !landed) playerAnimator.SetBool("isFalling", true);

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpForce;
                playerAnimator.SetTrigger("Jump");
                playerAnimator.SetBool("isFalling", false);
                jumpedThisTick = true;
                PlaySound(jumpClip);
                landed = false;
            }
            
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < -3f)
            {
                playerAnimator.SetBool("isFalling", true);
                landed = false;
            }
        }
    }

    public void OnFootstep()
    {
        if (!controller.isGrounded) return;

        // Wenn man zu langsam ist (Antippen), kein Sound
        if (controller.velocity.sqrMagnitude < 0.5f) return;

        if (footstepClips.Length > 0)
        {

            currentStepIndex = currentStepIndex % footstepClips.Length;
            AudioClip clipToPlay = footstepClips[currentStepIndex];

            if (audioSource != null && clipToPlay != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(clipToPlay, 0.25f); 
            }

            currentStepIndex++;
        }
    }
    void HandleAnimation()
    {
        if (playerAnimator == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool isSprinting = (currentSpeed > runSpeed) && (h != 0 || v != 0);
        float multiplier = isSprinting ? 2f : 1f;
        float targetX = h * multiplier;
        float targetY = v * multiplier;

         if (Input.GetKeyDown(KeyCode.LeftControl) && (h != 0 || v != 0) && controller.isGrounded)
        {
            // cooldown check
            if (Time.time >= lastSlideTime + slideCooldown)
            {
                playerAnimator.SetTrigger("Slide");
                playerAnimator.SetBool("isSliding", true);
                
                // Slide Boost anwenden
                if (moveDirection.magnitude > 0.1f)
                {
                    ApplySlideBoost();
                    lastSlideTime = Time.time;
                }
            }
        }

        //if (Input.GetKeyUp(KeyCode.LeftControl) || (h == 0 && v == 0)) playerAnimator.SetBool("isSliding", false);

        slidingThisTick = playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("SlidingTag");

        playerAnimator.SetFloat("MoveX", targetX, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat("MoveY", targetY, 0.1f, Time.deltaTime);
        playerAnimator.SetBool("isGrounded", controller.isGrounded);
    }

    private void ApplySlideBoost()
    {
        // Boost in aktuelle Bewegungsrichtung (horizontal only, kein Y)
        Vector3 boostDirection = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
        boostVelocity = boostDirection * slideBoostForce;
    }

    public void ApplyJumpPadBoost(Vector3 boostVelocity)
    {
        velocity.y = boostVelocity.y;
        this.boostVelocity = new Vector3(boostVelocity.x, 0f, boostVelocity.z);
        playerAnimator.SetBool("isFalling", false);
        playerAnimator.SetTrigger("Jump");
        jumpedThisTick = true;
        PlaySound(jumpClip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clip);
        }
    }
}
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

        Vector3 finalMovement = (moveDirection * currentSpeed) + boostVelocity + new Vector3(0, velocity.y, 0);
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
            else if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("JumpTag")) playerAnimator.SetBool("isFalling", true);

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
            // Wir ändern NICHT mehr die globale Lautstärke (audioSource.volume),
            // damit das Springen danach nicht leiser wird.

            currentStepIndex = currentStepIndex % footstepClips.Length;
            AudioClip clipToPlay = footstepClips[currentStepIndex];

            if (audioSource != null && clipToPlay != null)
            {
                // Pitch Variation (wie vorher)
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                
                // HIER ist dein Regler: Die '0.4f' bedeutet 40% Lautstärke.
                // Ändere die 0.4f zu 0.2f (leiser) oder 0.8f (lauter), wie du willst.
                audioSource.PlayOneShot(clipToPlay, 1.0f); 
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

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            playerAnimator.SetTrigger("Slide");
            playerAnimator.SetBool("isSliding", true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl) || (h == 0 && v == 0)) playerAnimator.SetBool("isSliding", false);

        slidingThisTick = playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("SlidingTag");

        playerAnimator.SetFloat("MoveX", targetX, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat("MoveY", targetY, 0.1f, Time.deltaTime);
        playerAnimator.SetBool("isGrounded", controller.isGrounded);
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
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

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical); // Syntax Korrektur: Vector3 explizit
        if (inputDir.sqrMagnitude < 0.001f)
        {
            moveDirection = Vector3.zero;
            return;
        }
        moveDirection = transform.rotation * inputDir.normalized;
    }

    // HIER WAR DER FEHLER: Ich habe die Klammern aufgeräumt
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
        if (controller.isGrounded && footstepClips.Length > 0)
        {
            audioSource.volume = Random.Range(0.8f, 1.0f); 
            AudioClip clipToPlay = footstepClips[Random.Range(0, footstepClips.Length)];
            PlaySound(clipToPlay);
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

        if (isSprinting && Input.GetKeyDown(KeyCode.LeftControl))
        {
            playerAnimator.SetTrigger("Slide");
            playerAnimator.SetBool("isSliding", true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            playerAnimator.SetBool("isSliding", false);
        }

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
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }
}
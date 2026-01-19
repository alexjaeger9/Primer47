using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Animator playerAnimator;

    [Header("Audio Settings")] // NEU
    public AudioSource audioSource; // Audio Source
    public AudioClip jumpClip;      // Sprung-Sound
    public AudioClip landClip;      // Lande-Sound

    [Header("Footsteps")] //
    public AudioClip[] footstepClips; // Array für mehrere Sounds (Abwechslung)
    public float walkStepInterval = 0.5f; // Zeit zwischen Schritten beim normalen Laufen
    public float sprintStepInterval = 0.3f; // Zeit zwischen Schritten beim Sprinten
    private float footstepTimer; // Zählt die Zeit runter

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

    private Vector3 boostVelocity;
    private float boostVelocityDecay = 2f; //wie schnell der Jumppad Boost abnimmt


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
        HandleFootsteps();
        HandleAnimation();
        //jumpedThisTick = false;

        //Boost Velocity über Zeit abbauen
        if (boostVelocity.magnitude > 0.1f)
        {
            boostVelocity = Vector3.Lerp(boostVelocity, Vector3.zero, boostVelocityDecay * Time.deltaTime);
        }

        //normale Bewegung + externe Kräfte + Gravity
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

        bool isSprinting = false;
        if (vertical >= 0f) isSprinting = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isSprinting ? sprintSpeed : runSpeed;

        Vector3 inputDir = new(horizontal, 0f, vertical);
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
            if (playerAnimator.GetBool("isFalling"))
            {
                playerAnimator.SetBool("isFalling", false);
                playerAnimator.SetTrigger("Land");
                landedThisTick = true;
                //Debug.Log("Landung JETZT: " + velocity.y);
                PlaySound(landClip);
            }

            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpForce;
                playerAnimator.SetTrigger("Jump");
                playerAnimator.SetBool("isFalling", false);
                jumpedThisTick = true;
                PlaySound(jumpClip);
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            // erst Fallen bei gewisser Geschwindigkeit
            if (velocity.y < -3f)
            {
                playerAnimator.SetBool("isFalling", true);
            }
        }
        //Debug.Log(playerAnimator.GetBool("isFalling"));
    }

    // --- NEU: Schritt-Logik ---
    void HandleFootsteps()
    {
        // Wir spielen nur Sounds, wenn wir am Boden sind UND uns bewegen
        if (controller.isGrounded && moveDirection.magnitude > 0.1f)
        {
            // Timer runterzählen
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                // Zufälligen Sound aus dem Array wählen
                if (footstepClips.Length > 0)
                {
                    AudioClip clipToPlay = footstepClips[Random.Range(0, footstepClips.Length)];
                    PlaySound(clipToPlay);
                }

                // Timer zurücksetzen: Je nach Speed (Sprint oder Normal)
                bool isSprinting = (currentSpeed == sprintSpeed);
                footstepTimer = isSprinting ? sprintStepInterval : walkStepInterval;
            }
        }
        else
        {
            // Wenn wir stehen bleiben, Timer fast auf 0 setzen, 
            // damit wir beim Loslaufen sofort einen Schritt hören
            footstepTimer = 0.05f; 
        }
    }
    // -------------------------
    
    void HandleAnimation()
    {
        if (playerAnimator == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool isSprinting = false;
        if (v >= 0f) isSprinting = Input.GetKey(KeyCode.LeftShift) && (h != 0 || v != 0);
        float multiplier = isSprinting ? 2f : 1f;
        float targetX = h * multiplier;
        float targetY = v * multiplier;

        playerAnimator.SetFloat("MoveX", targetX, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat("MoveY", targetY, 0.1f, Time.deltaTime);

        playerAnimator.SetBool("isGrounded", controller.isGrounded);
    }

    public void ApplyJumpPadBoost(Vector3 boostVelocity)
    {
        //vertikale Komponente in velocity.y
        velocity.y = boostVelocity.y;

        //horizontale Komponente in boostVelocity
        this.boostVelocity = new Vector3(boostVelocity.x, 0f, boostVelocity.z);

        //Animation
        playerAnimator.SetBool("isFalling", false);
        playerAnimator.SetTrigger("Jump");
        PlaySound(jumpClip);
    }
    // Hilfsfunktion damit wir den Code nicht doppelt schreiben
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Leichte Variation der Tonhöhe für Natürlichkeit
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }
}
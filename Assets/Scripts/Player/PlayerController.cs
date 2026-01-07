using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Animator playerAnimator;
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
        if(vertical >= 0f) isSprinting = Input.GetKey(KeyCode.LeftShift);
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
                //Debug.Log("Landung JETZT: " + velocity.y);
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
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;

            // Erst bei einer gewissen Fallgeschwindigkeit das Falling aktivieren
            // -3f bis -4f ist gut, um nicht bei Treppenstufen zu "fallen"
            if (velocity.y < -3f)
            {
                playerAnimator.SetBool("isFalling", true);
            }
        }
        //Debug.Log(playerAnimator.GetBool("isFalling"));
    }

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
    }
}
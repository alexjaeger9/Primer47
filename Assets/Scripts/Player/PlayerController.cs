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

    void OnAnimatorIK(int layerIndex)
    {
        if (playerAnimator == null || playerShooter == null)
        {
            return;
        }

        float ikWeight = 1.0f;

        // --- Werte aus PlayerShooter abrufen ---
        Vector3 ikPosition = playerShooter.handTargetPosition;
        Vector3 aimDirection = playerShooter.weaponAimDirection;


        // =======================================================
        // NEUER, KORREKTER ROTATIONSANSATZ
        // =======================================================

        // 1. Definiere die Vorwärts-Richtung (Z-Achse der Hand)
        // Sie soll in die Zielrichtung zeigen (aimDirection).
        Vector3 forward = aimDirection;

        // 2. Definiere die Up-Richtung (Y-Achse der Hand)
        // Für die rechte Hand sollte die 'Up'-Achse senkrecht zur Schussrichtung
        // und typischerweise in Richtung des Handgelenks/Armes zeigen.

        // Wir verwenden die lokale Rechts-Achse des Spielers als Orientierung
        // und drehen sie, um die Handfläche zur Waffe zu halten.
        // Die Up-Achse des Griffs muss quer zur Wussrichtung stehen.
        Vector3 up = transform.up; // Startwert: Welt-Up

        // Da die Z-Achse (forward) gesetzt ist, muss die Y-Achse (up) so gedreht werden, 
        // dass sie senkrecht zu 'forward' steht, aber die natürliche Haltung der Hand beibehält.

        // Bessere Variante: Nimm die Up-Achse der Kamera/des Spielers, 
        // und korrigiere sie, damit sie senkrecht zur Schussrichtung steht.

        // **Wir müssen die Y- und Z-Achsen der Handknochen vertauschen, die LookRotation standardmäßig annimmt.**

        // Quaternion.LookRotation erwartet: Z-Achse = forward, Y-Achse = up.
        // Für die Waffe muss die Hand-Z-Achse die Waffe halten.

        // Variante A (Die professionellste): Wir definieren die Achsen explizit.
        Vector3 desiredForward = aimDirection;
        Vector3 desiredUp = transform.up; // Der Handrücken soll nach oben zeigen (relativ zur Welt)

        // ABER: Wenn die Hand verdreht ist, liegt es daran, dass die Up-Achse nicht stimmt.
        // Die 'Up'-Achse des Handknochens muss beim Griff typischerweise in Richtung der Finger zeigen.
        // Korrektur: Die 'Up'-Achse der Hand muss die Waffe halten (meist die 'Links'-Achse des Arms).

        // Nutzen wir die LookRotation-Überladung mit der Up-Richtung:
        // Der Vektor, der die Handfläche definiert. Für die rechte Hand sollte der Daumen nach oben zeigen.
        // Wir nehmen die Up-Richtung der Kamera, die in die Blickrichtung rotiert ist.
        Vector3 desiredRightHandUp = playerShooter.mainCamera.transform.up;

        Quaternion targetRotation = Quaternion.LookRotation(desiredForward, desiredRightHandUp);

        // Jetzt der WICHTIGSTE Fix: Die Rotation der Hand anpassen, damit die Hand die Waffe hält.
        // Der Offset ist nicht statisch, sondern basiert auf den Achsen der Hand im Vergleich zur Waffe.
        // Für die Hand-IK ist die X-Achse meist die Daumenachse, Z die Schussrichtung, Y die Krümmung.

        // Daumen muss nach oben zeigen: Wir rotieren um die Z-Achse, damit die Handfläche nach hinten zeigt
        // und rotieren um die X-Achse, um die Waffe auszurichten.

        // Der notwendige Korrektur-Offset, der das 'Grip'-Problem behebt, ist NICHT mehr statisch zur Welt, 
        // sondern statisch zur LookRotation.

        // **VERSUCHEN SIE DIESEN SPEZIFISCHEN OFFSET (Der häufigste Fix für Hands-IK):**
        // X = 0 (Kein Pitch), Y = -90 (Dreht die Waffe in die Handfläche), Z = 90 (Legt die Hand flach auf die Waffe)
        //Quaternion gripOffset = Quaternion.Euler(0, 90f, 0f);

        // Wenn -90/90 nicht klappt, versuchen Sie 0/90 oder 0/-90.
        // Quaternion gripOffset = Quaternion.Euler(0, 0, 90f); 

        //targetRotation *= gripOffset;


        // =======================================================
        // RECHTE HAND ANWENDEN
        // =======================================================

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, ikPosition);
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);

        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, targetRotation);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);


        // =======================================================
        // Waffe an die Hand binden (Muss an die Hand gebunden werden)
        // Die Waffe folgt dem IK-Target, das jetzt an der IK-Position ist.
        // Wir setzen die Waffe direkt auf die berechnete IK-Position und -Rotation.
        if (playerShooter.gunTransform != null)
        {
            playerShooter.gunTransform.position = ikPosition; // Die Waffe geht zur Handposition
            playerShooter.gunTransform.rotation = targetRotation; // Die Waffe zeigt in die Zielrichtung
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
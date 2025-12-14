using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
        ApplyGravity();
        RotateToCameraForward();
    }

    private void HandleMovement()
    {
        // TODO: Input einlesen und bewegen
    }

    private void HandleJump()
    {
        // TODO: Springen
    }

    private void ApplyGravity()
    {
        // TODO: Gravity anwenden
    }

    private void RotateToCameraForward()
    {
        // TODO: Spieler zur Kamerarichtung ausrichten
    }
}

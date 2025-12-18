using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    public float recordTickRate = 1000f;
    [HideInInspector] public RunData currentRunData;
    private PlayerController playerController;
    private PlayerShooter playerShooter;
    private ThirdPersonCamera thirdPersonCamera;
    private float currentTime;
    private float timeSinceLastTick;
    private bool isRecording;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerShooter = GetComponent<PlayerShooter>();
        thirdPersonCamera = FindAnyObjectByType<ThirdPersonCamera>();
    }

    private void Update()
    {
        if (!isRecording) return;
        currentTime += Time.deltaTime;
        timeSinceLastTick += Time.deltaTime;
        if (timeSinceLastTick >= 1f / recordTickRate)
        {
            CaptureFrame();
            timeSinceLastTick = 0f;
            playerShooter.ResetTickFlags();
        }
    }

    public void StartRecording()
    {
        currentRunData = new RunData();
        currentTime = 0f;
        timeSinceLastTick = 0f;
        isRecording = true;
    }

    public RunData StopRecording()
    {
        currentTime += Time.deltaTime;
        CaptureFrame();
        isRecording = false;
        if (currentRunData != null) currentRunData.duration = currentTime;
        return currentRunData;
    }

    private void CaptureFrame()
    {
        Vector3 weaponAimDirection = thirdPersonCamera.transform.forward;
        Vector3 currentAimTarget = thirdPersonCamera.transform.position + weaponAimDirection * 100f; // 100f = maxRange

        RecordedFrame frame = new RecordedFrame
        {
            time = currentTime,
            position = transform.position,
            rotation = transform.rotation,
            pitch = thirdPersonCamera.pitch,
            fired = playerShooter.firedThisTick,
            jumped = playerController.jumpedThisTick,

            moveX = playerController.playerAnimator.GetFloat("MoveX"),
            moveY = playerController.playerAnimator.GetFloat("MoveY"),
            isFalling = playerController.playerAnimator.GetBool("isFalling"),
            isGrounded = playerController.playerAnimator.GetBool("isGrounded"),
            aimTargetPosition = currentAimTarget
        };
        if (frame.fired)
        {
            frame.fireMuzzlePosition = playerShooter.recordedMuzzlePosition;
            frame.fireDirection = playerShooter.recordedFireDirection;
        }
        else
        {
            frame.fireMuzzlePosition = Vector3.zero;
            frame.fireDirection = Vector3.zero;
        }
        if (currentRunData.frames.Count == 0 || currentRunData.frames[currentRunData.frames.Count - 1].time < frame.time - 0.001f)
        {
            currentRunData.frames.Add(frame);
        }
    }
}
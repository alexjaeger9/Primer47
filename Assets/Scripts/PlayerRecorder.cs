using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerShooter))]
public class PlayerRecorder : MonoBehaviour
{
    public float recordTickRate = 20f;

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
        thirdPersonCamera = GetComponent<ThirdPersonCamera>();
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
        isRecording = false;
        if (currentRunData != null)
        {
            currentRunData.duration = currentTime;
        }
        return currentRunData;
    }

    private void CaptureFrame()
    {
        RecordedFrame frame = new RecordedFrame();
        frame.time = currentTime;
        frame.position = transform.position;
        frame.rotation = transform.rotation;
        frame.pitch = thirdPersonCamera.pitch;
        frame.fired = playerShooter.firedThisTick;
        frame.jumped = playerController.jumpedThisTick;

        currentRunData.frames.Add(frame);
    }
}

using UnityEngine;

// Annahme: RecordedFrame und RunData sind global verfügbar (wie in Ihren separaten Dateien)
// [RequireComponent(typeof(PlayerController))] // Angenommen, diese ist noch nötig
[RequireComponent(typeof(PlayerShooter))]
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
        // Sicherstellen, dass die Komponenten korrekt geholt werden
        playerController = GetComponent<PlayerController>();
        playerShooter = GetComponent<PlayerShooter>();
        // FindAnyObjectByType ist korrekt, wenn die Kamera nicht am Player hängt
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

    // ... (StartRecording, StopRecording Logik bleiben gleich) ...
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

        
        // Wenn der Player im aktuellen Tick geschossen hat,
        // speichern wir die exakten Muzzle-Daten aus dem PlayerShooter.
        if (frame.fired)
        {
            frame.fireMuzzlePosition = playerShooter.recordedMuzzlePosition;
            frame.fireDirection = playerShooter.recordedFireDirection;
        }
        else
        {
            // Optional: Setze die Werte auf 0, um sicherzustellen, dass keine alten Werte übernommen werden.
            // (Obwohl dies bei structs nicht nötig ist, macht es die Absicht klarer)
            frame.fireMuzzlePosition = Vector3.zero;
            frame.fireDirection = Vector3.zero;
        }

        currentRunData.frames.Add(frame);
    }
}
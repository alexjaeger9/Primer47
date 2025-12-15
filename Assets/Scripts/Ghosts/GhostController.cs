using UnityEngine;

public class GhostController : MonoBehaviour
{
    // HINWEIS: RunData und RecordedFrame müssen hier oder in einem externen File definiert sein
    public RunData runData;

    private int currentFrameIndex;
    private float currentTime;
    private GhostShooter ghostShooter;

    private void Awake()
    {
        ghostShooter = GetComponentInChildren<GhostShooter>();

        if (ghostShooter == null)
        {
            Debug.LogError("GhostController: GhostShooter konnte in den Kindern nicht gefunden werden!", this);
        }
    }

    public void Init(RunData data)
    {
        runData = data;
        currentFrameIndex = 0;
        currentTime = 0f;
    }

    private void Update()
    {
        if (runData == null || runData.frames.Count == 0) return;

        currentTime += Time.deltaTime;
        UpdatePlayback();
    }

    private void UpdatePlayback()
    {
        // A) Frame-Index vorrücken und Events verarbeiten
        while (currentFrameIndex < runData.frames.Count - 1 && currentTime >= runData.frames[currentFrameIndex + 1].time)
        {
            RecordedFrame frame = runData.frames[currentFrameIndex];

            if (frame.fired)
            {
                // **KORREKTUR:** Übergabe der gespeicherten, konsistenten Schussdaten
                ghostShooter.ShootFromReplay(frame.fireMuzzlePosition, frame.fireDirection);
            }

            currentFrameIndex++;
        }

        if (currentFrameIndex >= runData.frames.Count - 1)
        {
            return;
        }

        // B) Interpolation
        RecordedFrame a = runData.frames[currentFrameIndex];
        RecordedFrame b = runData.frames[currentFrameIndex + 1];

        float t = Mathf.InverseLerp(a.time, b.time, currentTime);

        Vector3 pos = Vector3.Lerp(a.position, b.position, t);
        transform.position = pos;

        Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);
        transform.rotation = rot;

        float pitch = Mathf.Lerp(a.pitch, b.pitch, t);

        if (ghostShooter.pitchTarget != null)
        {
            // Die Pitch-Rotation wird weiterhin hier für die visuelle Darstellung gesetzt
            ghostShooter.pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
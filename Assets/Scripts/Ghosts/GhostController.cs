using UnityEngine;

public class GhostController : MonoBehaviour
{
    public RunData runData;

    private int currentFrameIndex;
    private float currentTime;
    private GhostShooter ghostShooter;
    private bool hasShotThisTick = false;

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
        //hasShotThisTick = false;
        UpdatePlayback();
    }

    private void UpdatePlayback()
    {
        // A) Frame-Index vorrücken, falls die Zeit des nächsten Frames erreicht wurde
        while (currentFrameIndex < runData.frames.Count - 1 && currentTime >= runData.frames[currentFrameIndex + 1].time)
        {
            if (runData.frames[currentFrameIndex].fired)
            {
                // Führen Sie den Schuss aus
                ghostShooter.ShootFromReplay();
                // Wir müssen hier keine hasShotThisTick setzen, da wir den aktuellen 
                // Frame *sofort* verarbeiten, wenn seine Zeit erreicht ist.
            }

            currentFrameIndex++;
        }

        if (currentFrameIndex >= runData.frames.Count - 1)
        {
            // Replay beendet (oder letzter Frame erreicht)
            return;
        }

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
            // Wir setzen die LOKALE Rotation, da der Root-Ghost bereits horizontal rotiert ist
            ghostShooter.pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
        /*
        if (a.fired && !hasShotThisTick)
        {
            // Führen Sie den Schuss aus
            ghostShooter.ShootFromReplay();
            hasShotThisTick = true;
        }
        */

    }
}

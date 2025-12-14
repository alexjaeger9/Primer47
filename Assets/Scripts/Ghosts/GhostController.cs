using UnityEngine;

public class GhostController : MonoBehaviour
{
    public RunData runData;

    private int currentFrameIndex;
    private float currentTime;
    private GhostShooter ghostShooter;

    private void Awake()
    {
        ghostShooter = GetComponent<GhostShooter>();
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
        if (currentFrameIndex >= runData.frames.Count - 1)
        {
            // Optional: Ghost bleibt stehen oder despawnt
            return;
        }

        RecordedFrame a = runData.frames[currentFrameIndex];
        RecordedFrame b = runData.frames[currentFrameIndex + 1];

        if (currentTime > b.time)
        {
            currentFrameIndex++;
            return;
        }

        float t = Mathf.InverseLerp(a.time, b.time, currentTime);
        Vector3 pos = Vector3.Lerp(a.position, b.position, t);
        Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);

        transform.position = pos;
        transform.rotation = rot;

        if (a.fired || b.fired)
        {
            // TODO: genauer Zeitpunkt prüfen, hier vereinfacht
            ghostShooter.ShootFromReplay();
        }
    }
}

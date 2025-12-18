using UnityEngine;

public class GhostController : MonoBehaviour
{
    public RunData runData;
    private int currentFrameIndex;
    private float currentTime;
    private GhostShooter ghostShooter;

    public Animator ghostAnimator;
    public Transform pitchTarget;
    private Vector3 currentIKTarget;

    private void Awake()
    {
        ghostShooter = GetComponentInChildren<GhostShooter>();
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
        // --- TEIL A: Einmalige Ereignisse (Trigger) ---
        // Alles was in der while-Schleife passiert, wird exakt so oft ausgeführt, wie es aufgenommen wurde
        while (currentFrameIndex < runData.frames.Count - 1 && currentTime >= runData.frames[currentFrameIndex + 1].time)
        {
            RecordedFrame frame = runData.frames[currentFrameIndex];

            // Schießen
            if (frame.fired)
            {
                ghostShooter.ShootFromReplay(frame.fireMuzzlePosition, frame.fireDirection);
            }

            // JUMP hierhin verschieben! 
            // So wird der Trigger exakt EINMAL pro aufgenommenem Sprung gefeuert.
            if (frame.jumped && ghostAnimator != null)
            {
                ghostAnimator.SetBool("isFalling", false);
                ghostAnimator.SetTrigger("Jump");
                Debug.Log("JumpTRIGGER");
            }

            // LANDUNG Logik innerhalb der Ticks prüfen
            RecordedFrame nextFrame = runData.frames[currentFrameIndex + 1];
            if (frame.isFalling && !nextFrame.isFalling)
            {
                ghostAnimator.SetTrigger("Land");
            }

            currentFrameIndex++;
        }

        if (currentFrameIndex >= runData.frames.Count - 1) return;

        // --- TEIL B: Kontinuierliche Werte (Lerp & Bools) ---
        RecordedFrame a = runData.frames[currentFrameIndex];
        RecordedFrame b = runData.frames[currentFrameIndex + 1];
        float t = Mathf.InverseLerp(a.time, b.time, currentTime);

        transform.position = Vector3.Lerp(a.position, b.position, t);
        transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);

        if (ghostAnimator != null)
        {
            ghostAnimator.SetFloat("MoveX", Mathf.Lerp(a.moveX, b.moveX, t));
            ghostAnimator.SetFloat("MoveY", Mathf.Lerp(a.moveY, b.moveY, t));

            // Bools setzen wir hier permanent (Zustände)
            ghostAnimator.SetBool("isFalling", a.isFalling);
        }

        currentIKTarget = Vector3.Lerp(a.aimTargetPosition, b.aimTargetPosition, t);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (ghostAnimator == null) return;
        if (runData.duration == 0) return;

        float ikWeight = 1.0f; // Oder nimm einen Wert aus dem Frame, falls du Aiming an/aus willst

        // Hand zum Ziel führen
        ghostAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
        ghostAnimator.SetIKPosition(AvatarIKGoal.RightHand, currentIKTarget);

        // Hand rotieren (sie soll in Richtung des Ziels schauen)
        Vector3 direction = (currentIKTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            ghostAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            ghostAnimator.SetIKRotation(AvatarIKGoal.RightHand, lookRot);
        }

        // Kopf zum Ziel drehen
        ghostAnimator.SetLookAtWeight(ikWeight, 0.8f, 1.0f, 1.0f);
        ghostAnimator.SetLookAtPosition(currentIKTarget);
    }
}
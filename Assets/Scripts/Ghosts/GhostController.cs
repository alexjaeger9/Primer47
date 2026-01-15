using UnityEngine;

public class GhostController : MonoBehaviour
{
    public RunData runData;
    private int currentFrameIndex;
    private float currentTime;
    private GhostShooter ghostShooter;
    private GhostHealth ghostHealth;

    public Animator ghostAnimator;
    public Transform pitchTarget;
    private Vector3 currentIKTarget;

    [HideInInspector] private Vector3 handRotationOffset = new Vector3(0, 0, -90);


    private void Awake()
    {
        ghostShooter = GetComponentInChildren<GhostShooter>();
        ghostHealth = GetComponentInChildren<GhostHealth>();
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
        // Alles was in der while-Schleife passiert, wird exakt so oft ausgeführt, wie es aufgenommen wurde
        while (currentFrameIndex < runData.frames.Count - 1 && currentTime >= runData.frames[currentFrameIndex + 1].time)
        {
            // Recording Ende
            if (currentFrameIndex >= runData.frames.Count - 1)
            {
                ghostHealth.StopMovement();
                return;
            }
            
            //current Frame
            RecordedFrame frame = runData.frames[currentFrameIndex];

            // Schiessen
            if (frame.fired)
            {
                ghostShooter.ShootFromReplay(frame.fireMuzzlePosition, frame.fireDirection);
            }

            // Jump trigger
            if (frame.jumped && ghostAnimator != null)
            {
                ghostAnimator.SetBool("isFalling", false);
                ghostAnimator.SetTrigger("Jump");
                Debug.Log("JumpTRIGGER");
            }

            // Landung Logik
            RecordedFrame nextFrame = runData.frames[currentFrameIndex + 1];
            if (frame.isFalling && !nextFrame.isFalling)
            {
                ghostAnimator.SetTrigger("Land");
            }

            currentFrameIndex++;
        }

        // Kontinuierliche Werte (Lerp & Bools) 
        RecordedFrame a = runData.frames[currentFrameIndex];
        RecordedFrame b = runData.frames[currentFrameIndex + 1];
        float t = Mathf.InverseLerp(a.time, b.time, currentTime);

        transform.position = Vector3.Lerp(a.position, b.position, t);
        transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);

        if (ghostAnimator != null)
        {
            ghostAnimator.SetFloat("MoveX", Mathf.Lerp(a.moveX, b.moveX, t));
            ghostAnimator.SetFloat("MoveY", Mathf.Lerp(a.moveY, b.moveY, t));
            ghostAnimator.SetBool("isFalling", a.isFalling);
        }

        currentIKTarget = Vector3.Lerp(a.aimTargetPosition, b.aimTargetPosition, t);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (ghostAnimator == null) return;
        if (runData.duration == 0) return;

        float ikWeight = 1.0f;

        // Hand/Arm auf Ziel zeigen
        ghostAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
        ghostAnimator.SetIKPosition(AvatarIKGoal.RightHand, currentIKTarget);

        // Hand Rotation
        Vector3 direction = (currentIKTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion correction = Quaternion.Euler(handRotationOffset);
            Quaternion finalHandRotation = targetRotation * correction;
            ghostAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            ghostAnimator.SetIKRotation(AvatarIKGoal.RightHand, finalHandRotation);
        }

        // Kopf zum Ziel drehen
        ghostAnimator.SetLookAtWeight(ikWeight, 0.8f, 1.0f, 1.0f);
        ghostAnimator.SetLookAtPosition(currentIKTarget);
    }
}
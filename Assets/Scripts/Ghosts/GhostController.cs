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
        while (currentFrameIndex < runData.frames.Count - 1 && currentTime >= runData.frames[currentFrameIndex + 1].time)
        {
            RecordedFrame frame = runData.frames[currentFrameIndex];
            if (frame.fired)
            {
                ghostShooter.ShootFromReplay(frame.fireMuzzlePosition, frame.fireDirection);
            }
            currentFrameIndex++;
        }

        if (currentFrameIndex >= runData.frames.Count - 1)
        {
            return;
        }
        RecordedFrame a = runData.frames[currentFrameIndex];
        RecordedFrame b = runData.frames[currentFrameIndex + 1];
        float t = Mathf.InverseLerp(a.time, b.time, currentTime);
        transform.position = Vector3.Lerp(a.position, b.position, t);
        transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);

        // 2. Animationen synchronisieren
        if (ghostAnimator != null)
        {
            // Floats interpolieren wir für flüssige Übergänge
            ghostAnimator.SetFloat("MoveX", Mathf.Lerp(a.moveX, b.moveX, t));
            ghostAnimator.SetFloat("MoveY", Mathf.Lerp(a.moveY, b.moveY, t));

            // Bools übernehmen wir vom aktuellen Frame
            ghostAnimator.SetBool("isFalling", a.isFalling);
            ghostAnimator.SetBool("isGrounded", a.isGrounded);

            // Trigger (Jump/Land)
            if (a.jumped) ghostAnimator.SetTrigger("Jump");

            // Logik für Landung: Wenn im letzten Frame falling war und jetzt nicht mehr
            if (a.isFalling && !b.isFalling) ghostAnimator.SetTrigger("Land");
        }

        /*float pitch = Mathf.Lerp(a.pitch, b.pitch, t);
        if (pitchTarget != null)
        {
            pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }*/

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
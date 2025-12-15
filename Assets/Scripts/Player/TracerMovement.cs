using UnityEngine;

public class TracerMovement : MonoBehaviour
{
    private Vector3 targetPosition;
    public float speed = 300f;
    public float destroyDelay = 0.1f;

    public void Initialize(Vector3 start, Vector3 end)
    {
        targetPosition = end;
        transform.position = start;
        transform.LookAt(targetPosition);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
        if (transform.position == targetPosition)
        {
            Destroy(gameObject, destroyDelay);
            enabled = false;
        }
    }
}
using UnityEngine;

public class TracerMovement : MonoBehaviour
{
    private Vector3 targetPosition;
    public float speed = 300f; // Hohe Geschwindigkeit für sofortigen Schuss-Effekt
    public float destroyDelay = 0.1f; // Verzögerung, damit der Trail ausklingt

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

        // Ziel erreicht
        if (transform.position == targetPosition)
        {
            Destroy(gameObject, destroyDelay);
            enabled = false;
        }
    }
}
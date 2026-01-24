using UnityEngine;
using System.Collections;

public class TracerMovement : MonoBehaviour
{
    private Vector3 targetPosition;
    public float speed = 300f;
    public float destroyDelay = 0.1f;

    public void Initialize(Vector3 start, Vector3 end)
    {
        targetPosition = end; //wo tracer hinsoll
        transform.position = start; //startpos
        transform.LookAt(targetPosition); //in richtung des Ziels drehen
        gameObject.SetActive(true); //Bullet aktivieren
        enabled = true;
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
            enabled = false; //nach Delay in Pool zurück
            StartCoroutine(ReturnToPoolAfterDelay());
        }
    }
    
    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        BulletPool.Instance.ReturnBullet(gameObject);
    }
}
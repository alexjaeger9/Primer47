using UnityEngine;

public class TracerMovement : MonoBehaviour
{
    private Vector3 targetPosition; //wohin soll Bullet fliegen
    public float speed = 300f; //wie schnell
    public float destroyDelay = 0.1f; //wie lange am Ziel, bevor zurück zum Pool
    
    private bool isInitialized = false; //ob Bullet schon initialisiert wurde

    public void Initialize(Vector3 start, Vector3 end)
    {
        targetPosition = end;
        transform.position = start;
        transform.LookAt(targetPosition);
        
        //Bullet aktivieren
        gameObject.SetActive(true);
        enabled = true;
        isInitialized = true;
    }

    void Update()
    {        
        //Bullet Bewegung
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
        
        if (transform.position == targetPosition)
        {
            Invoke(nameof(ReturnToPool), destroyDelay);
            enabled = false;
        }
    }

    private void ReturnToPool()
    {
        isInitialized = false;
        BulletPool.Instance.ReturnBullet(gameObject);
    }
    
    //wenn Bullet deaktiviert, alle Invokes abbrechen
    private void OnDisable()
    {
        CancelInvoke();
    }
}
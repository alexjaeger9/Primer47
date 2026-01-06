using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; //Spieler, dem die Cam folgt
    
    //Kamera Position
    [SerializeField] private float distance = 5f; //wie weit hinten die Cam ist
    [SerializeField] private float height = 1.5f;
    
    //Sensitivität
    [SerializeField] public float verticalMouseSensitivity = 200f;
    [SerializeField] public float horizontalMouseSensitivity = 200f;
    
    //Vertikale Rotation Limitter (kein 360 vertikal möglicj)
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;
    
    [HideInInspector] public float pitch; //vertikal
    public float yaw; //horizontal

    //für unabhängiges gucken, ohne Spieler (nur Kamara)
    private bool useOwnYaw = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //Kameraposition aus dem Editor auslesen
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseY = Input.GetAxis("Mouse Y");
        pitch -= mouseY * verticalMouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        float finalYaw;
        if (useOwnYaw)
        {
            //eigene Yaw Berechnung: Spieler steht still, Kamera dreht sich frei
            float mouseX = Input.GetAxis("Mouse X");
            yaw += mouseX * horizontalMouseSensitivity * Time.deltaTime;
            finalYaw = yaw;
        }
        else
        {
            //normal
            finalYaw = target.eulerAngles.y;
            yaw = finalYaw;
        }
        
        Quaternion rotation = Quaternion.Euler(pitch, finalYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);
        Vector3 desiredPos = target.position + offset;
        transform.position = desiredPos;
        transform.rotation = rotation;
    }
    
    public void EnableFreeCamera()
    {
        useOwnYaw = true;
    }
    
    public void DisableFreeCamera()
    {
        useOwnYaw = false;
    }
}
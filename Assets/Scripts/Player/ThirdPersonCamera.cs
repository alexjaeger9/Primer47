using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Position")]
    [SerializeField] private float distance = 3f;
    [SerializeField] private float height = 0.6f;
    [SerializeField] private float shoulderOffset = 1.0f;

    [Header("Kamera Kollision")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float minDistance = 0.5f;

    // Wie schnell die Kamera auf Hindernisse reagiert
    [SerializeField] private float smoothInSpeed = 25f;
    [SerializeField] private float smoothOutSpeed = 5f;

    private float currentDistance;

    [Header("Sensitivität")]
    public float verticalMouseSensitivity = 200f;
    public float horizontalMouseSensitivity = 200f;

    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [HideInInspector] public float pitch;
    public float yaw;

    private bool useOwnYaw = false;


    [Header("ADS Sensitivity")]
    [Range(0.1f, 1f)] public float aimSensitivityMultiplier = 0.5f;

    [Header("Zoom Settings")]
    public float defaultFOV = 60f; 
    public float zoomFOV = 40f;    
    public float zoomSpeed = 10f;  
    private Camera cam;            
    private bool isZooming = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
        currentDistance = distance;

        cam = GetComponent<Camera>();
        defaultFOV = cam.fieldOfView;
    }

    public void SetZoom(bool state)
    {
        isZooming = state;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetFOV = isZooming ? zoomFOV : defaultFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

        float mouseY = Input.GetAxis("Mouse Y");

        float sensitivityMultiplier = isZooming ? aimSensitivityMultiplier : 1.0f;

        pitch -= mouseY * verticalMouseSensitivity * sensitivityMultiplier * Time.deltaTime;        
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        float finalYaw;
        if (useOwnYaw)
        {
            float mouseX = Input.GetAxis("Mouse X");
            yaw += mouseX * horizontalMouseSensitivity * Time.deltaTime;
            finalYaw = yaw;
        }
        else
        {
            finalYaw = target.eulerAngles.y;
            yaw = finalYaw;
        }

        Quaternion rotation = Quaternion.Euler(pitch, finalYaw, 0f);

        // Kollision
        Vector3 rayStartPos = target.position + (rotation * new Vector3(shoulderOffset, height, 0f));
        Vector3 rayDirection = rotation * Vector3.back;

        RaycastHit hit;
        float targetDist = distance;

        // small offset
        Vector3 castStart = rayStartPos + (rotation * Vector3.forward * 0.2f);

        if (Physics.SphereCast(castStart, collisionRadius, rayDirection, out hit, distance + 0.2f, collisionLayers))
        {
            targetDist = Mathf.Clamp(hit.distance - collisionRadius, minDistance, distance);
        }

        // Smoothing
        float currentSmoothing = (targetDist < currentDistance) ? smoothInSpeed : smoothOutSpeed;
        currentDistance = Mathf.Lerp(currentDistance, targetDist, Time.deltaTime * currentSmoothing);

        // final Position
        transform.position = rayStartPos + (rayDirection * currentDistance);
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
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float verticalMouseSensitivity = 200f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    [HideInInspector] public float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pitch = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseY = Input.GetAxis("Mouse Y");
        pitch -= mouseY * verticalMouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        float playerYaw = target.eulerAngles.y;

        Quaternion rotation = Quaternion.Euler(pitch, playerYaw, 0f);

        Vector3 offset = rotation * new Vector3(0f, height, -distance);
        Vector3 desiredPos = target.position + offset;

        transform.position = desiredPos;
        transform.rotation = rotation;
    }
}
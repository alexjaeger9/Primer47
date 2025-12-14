using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public Transform playerTransform;
    public float height = 20f;
    public bool rotateWithPlayer = false;

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 pos = playerTransform.position;
        pos.y += height;
        transform.position = pos;

        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}

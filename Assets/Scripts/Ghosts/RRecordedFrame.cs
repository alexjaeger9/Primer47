using UnityEngine;

[System.Serializable]
public class RecordedFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public bool fired;
    public bool jumped;
}

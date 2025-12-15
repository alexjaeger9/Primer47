using UnityEngine;
using System;

[Serializable]
public struct RecordedFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public float pitch;
    public bool fired;
    public bool jumped;
    public Vector3 fireMuzzlePosition;
    public Vector3 fireDirection;
}
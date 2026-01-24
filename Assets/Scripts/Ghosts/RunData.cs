using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class RunData
{
    public List<RecordedFrame> frames = new();
    public float duration;
    public Vector3 startPosition;
}
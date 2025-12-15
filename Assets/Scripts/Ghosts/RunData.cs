using System.Collections.Generic;
using System;

[Serializable]
public class RunData
{
    public List<RecordedFrame> frames = new List<RecordedFrame>();
    public float duration;
}
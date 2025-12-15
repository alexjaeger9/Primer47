using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RunData
{
    // WICHTIG: Stellt sicher, dass RecordedFrame hier bekannt ist (mittels using oder Namespace)
    public List<RecordedFrame> frames = new List<RecordedFrame>();
    public float duration;
}
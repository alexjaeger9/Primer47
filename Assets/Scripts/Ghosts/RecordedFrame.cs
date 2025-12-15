using UnityEngine;
using System;

[Serializable]
public struct RecordedFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public float pitch; // Kamera-Neigung (X-Achse)
    public bool fired;
    public bool jumped;

    // NEU: Konsistente Schussdaten (Müssen bei 'fired = true' befüllt werden)
    public Vector3 fireMuzzlePosition; // Exakte Position der Mündung beim Feuern
    public Vector3 fireDirection;      // Exakte Blickrichtung beim Feuern (normalized)
}
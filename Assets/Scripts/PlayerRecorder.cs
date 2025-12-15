using UnityEngine;
// Fügen Sie hinzu, falls es sich um eine eigenständige Datei handelt
// using System.Collections.Generic;

// Annahme: RecordedFrame und RunData sind global verfügbar (wie in Ihren separaten Dateien)
// [RequireComponent(typeof(PlayerController))] 
[RequireComponent(typeof(PlayerShooter))]
public class PlayerRecorder : MonoBehaviour
{
    public float recordTickRate = 1000f;

    [HideInInspector] public RunData currentRunData;

    private PlayerController playerController;
    private PlayerShooter playerShooter;
    private ThirdPersonCamera thirdPersonCamera;
    private float currentTime;
    private float timeSinceLastTick;
    private bool isRecording;

    private void Awake()
    {
        // Sicherstellen, dass die Komponenten korrekt geholt werden
        playerController = GetComponent<PlayerController>();
        playerShooter = GetComponent<PlayerShooter>();
        thirdPersonCamera = FindAnyObjectByType<ThirdPersonCamera>();
    }

    private void Update()
    {
        if (!isRecording) return;

        if (playerShooter.firedThisTick)
        {
            Debug.Log($"[RECORDER] Update - Schuss-Flag erkannt VOR Tick in Frame: {Time.frameCount}.");
        }

        currentTime += Time.deltaTime;
        timeSinceLastTick += Time.deltaTime;

        if (timeSinceLastTick >= 1f / recordTickRate)
        {
            if (playerShooter.firedThisTick)
            {
                Debug.Log($"[RECORDER] Frame erfasst DURCH REGULÄREN TICK in Frame: {Time.frameCount}.");
            }
            CaptureFrame();
            timeSinceLastTick = 0f;
            playerShooter.ResetTickFlags();
        }
    }

    public void StartRecording()
    {
        currentRunData = new RunData();
        currentTime = 0f;
        timeSinceLastTick = 0f;
        isRecording = true;
    }

    // KORREKTUR für Fehler 1: Die Methode ist bereits korrekt, aber 
    // der GameManager muss sicherstellen, dass er eine Variable vom Typ RunData verwendet.
    // Hier ist der Code im PlayerRecorder bereits korrekt, da er RunData zurückgibt.
    // Innerhalb der Klasse PlayerRecorder
    public RunData StopRecording()
    {
        Debug.Log($"[RECORDER] STOP RECORDING aufgerufen in Frame: {Time.frameCount} | Time: {Time.time:F3}.");
        // NEUE LOGIK: Aktualisiere die Zeit, bevor wir den Frame erfassen
        // Wir müssen die Zeit hier explizit hinzufügen, da StopRecording VOR dem nächsten Update/Tick kommt
        currentTime += Time.deltaTime;

        // 1. Erfasse den letzten Frame (mit der Sicherheitsprüfung in CaptureFrame)
        CaptureFrame();

        // 2. Setze die Aufnahme SOFORT auf FALSE, um den Update-Prozess zu stoppen
        isRecording = false;

        // 3. Dauer setzen
        if (currentRunData != null)
        {
            currentRunData.duration = currentTime;
        }
        Debug.Log($"[RECORDER] STOP: Total Frames gespeichert: {currentRunData.frames.Count}.");
        // Die ResetTickFlags sind nicht mehr nötig, da der nächste Loop StartRecording aufruft.
        // Wir lassen die Flags für diesen Frame gesetzt, damit CaptureFrame sie sieht.
        return currentRunData;
    }


    private void CaptureFrame()
    {
        Debug.Log($"[RECORDER] CaptureFrame ausgeführt in Frame: {Time.frameCount} | firedThisTick={playerShooter.firedThisTick}.");
        RecordedFrame frame = new RecordedFrame();

        frame.time = currentTime;
        frame.position = transform.position;
        frame.rotation = transform.rotation;
        // Angenommen thirdPersonCamera.pitch und playerController.jumpedThisTick sind vorhanden
        frame.pitch = thirdPersonCamera.pitch;
        frame.fired = playerShooter.firedThisTick;
        frame.jumped = playerController.jumpedThisTick;


        // Wenn der Player im aktuellen Tick geschossen hat,
        // speichern wir die exakten Muzzle-Daten aus dem PlayerShooter.
        if (frame.fired)
        {
            frame.fireMuzzlePosition = playerShooter.recordedMuzzlePosition;
            frame.fireDirection = playerShooter.recordedFireDirection;
        }
        else
        {
            frame.fireMuzzlePosition = Vector3.zero;
            frame.fireDirection = Vector3.zero;
        }

        if (currentRunData.frames.Count > 0)
    {
        Debug.Log($"[RECORDER]   -> Letzte gespeicherte Zeit: {currentRunData.frames[currentRunData.frames.Count - 1].time:F6}");
    }

        // --- SICHERHEITSPRÜFUNG GEGEN DOPPELTE AUFZEICHNUNG ---
        // Wir fügen den Frame nur hinzu, wenn er entweder der erste Frame ist
        // ODER wenn die Zeit des aktuellen Frames merklich später ist als die Zeit des letzten Frames.
        // Die Toleranz von 0.001f verhindert Floating-Point-Fehler.
        if (currentRunData.frames.Count == 0 || currentRunData.frames[currentRunData.frames.Count - 1].time < frame.time - 0.001f)
        {
            currentRunData.frames.Add(frame);
        }
        // --------------------------------------------------------
    }
}
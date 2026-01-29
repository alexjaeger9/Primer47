using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioMixer mainMixer;

    [Header("Slow-Mo Pitch Settings")]
    public float minPitchAllowed = 0.95f;

    [Header("Slow-Mo Filter Settings")]
    public float normalCutoff = 22000f; // normal
    public float slowMoCutoff = 800f;    // Slow Mo

    [Header("Smoothness")]
    public float smoothSpeed = 1f; 

    private float currentPitch = 1f;
    private float currentCutoff = 22000f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float targetPitch = Mathf.Lerp(minPitchAllowed, 1f, Time.timeScale);
        float targetCutoff = (Time.timeScale < 1f) ? slowMoCutoff : normalCutoff;

        // Smoothing
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.unscaledDeltaTime * smoothSpeed);
        currentCutoff = Mathf.Lerp(currentCutoff, targetCutoff, Time.unscaledDeltaTime * smoothSpeed);

        // send to mixer
        mainMixer.SetFloat("MusicPitch", currentPitch);
        mainMixer.SetFloat("MusicLowPass", currentCutoff);
    }
}

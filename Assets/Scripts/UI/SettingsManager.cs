using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio")]
    public AudioMixer audioMixer;
    private float masterVolume = 0.75f;
    private float musicVolume = 0.75f;
    private float sfxVolume = 0.75f;
    
    private float mouseSensitivity = 100f;
    
    [Header("Graphics")]
    private bool isFullscreen = true;
    private int resolutionIndex = 0;
    private bool vSyncEnabled = true;
    private int fpsLimit = 60;

    private Resolution[] resolutions;

    private void Awake()
    {
        //damit Settings über Szenen bleiben -> Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //verfügbare Auflösungen
        resolutions = Screen.resolutions;
    }

    private void Start()
    {
        LoadSettings();
        ApplySettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0); //ture ? false
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("VSync", vSyncEnabled ? 1 : 0);
        PlayerPrefs.SetInt("FPSLimit", fpsLimit);
        
        PlayerPrefs.Save();
    }

    //vordefinierte Settings (default)
    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 100f);
        
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        vSyncEnabled = PlayerPrefs.GetInt("VSync", 1) == 1;
        fpsLimit = PlayerPrefs.GetInt("FPSLimit", 60);
    }

    public void ApplySettings()
    {
        //Audio
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        
        //Graphics
        SetResolution(resolutionIndex);
        SetFullscreen(isFullscreen);
        SetVSync(vSyncEnabled);
        SetFPSLimit(fpsLimit);
        
        //Sensitivity wird automatisch angewendet wenn Spiel lädt
        ApplySensitivityToGame();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
        ApplySensitivityToGame();
    }

    private void ApplySensitivityToGame()
    {
        //Kamera und PlayerController in der aktuellen Scene finden und Sensitivität setzen
        ThirdPersonCamera cam = FindAnyObjectByType<ThirdPersonCamera>();
        if (cam != null)
        {
            cam.horizontalMouseSensitivity = mouseSensitivity;
            cam.verticalMouseSensitivity = mouseSensitivity;
        }
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null)
        {
            controller.mouseSensitivity = mouseSensitivity;
        }
    }

    public void SetFullscreen(bool fullscreen)
    {
        isFullscreen = fullscreen;
        Screen.fullScreen = fullscreen;
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;
        resolutionIndex = index;
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
    }

    public void SetVSync(bool enabled)
    {
        vSyncEnabled = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    public void SetFPSLimit(int limit)
    {
        fpsLimit = limit;
        Application.targetFrameRate = limit;
    }

    //Getters
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetMouseSensitivity() => mouseSensitivity;
    public bool GetFullscreen() => isFullscreen;
    public int GetResolutionIndex() => resolutionIndex;
    public bool GetVSync() => vSyncEnabled;
    public int GetFPSLimit() => fpsLimit;
    public Resolution[] GetResolutions() => resolutions;
}
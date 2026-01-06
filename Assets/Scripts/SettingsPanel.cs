using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject settingsPanel;

    [Header("Audio Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;

    [Header("Sensitivity Slider")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityText;

    [Header("Graphics")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;
    public Toggle vsyncToggle;
    public TMP_Dropdown fpsLimitDropdown;

    [Header("Buttons")]
    public Button applyButton;
    public Button cancelButton;
    public Button resetButton;

    private Resolution[] resolutions;
    private bool eventsInitialized = false;

    //Defaults Konstanten
    private const float DEFAULT_MASTER_VOLUME = 0.75f;
    private const float DEFAULT_MUSIC_VOLUME = 0.75f;
    private const float DEFAULT_SFX_VOLUME = 0.75f;
    private const float DEFAULT_SENSITIVITY = 0.50f;
    private const bool DEFAULT_FULLSCREEN = true;
    private const bool DEFAULT_VSYNC = true;
    private const int DEFAULT_FPS_DROPDOWN_INDEX = 1;

    private void Update()
    {
        //ESC im Settings Panel = Cancel
        if(settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelSettings();
        }
    }

    private void Awake()
    {
        //füllt Resolutions Dropdown auf
        SetupResolutions();

        //füllt FPS Dropdown auf
        SetupFPSLimit();
    }

    private void InitializeEvents()
    {
        //Button Methoden
        applyButton.onClick.AddListener(ApplySettings);
        cancelButton.onClick.AddListener(CancelSettings);
        resetButton.onClick.AddListener(ResetSettings);

        //Slider/Toggle/Dropdown Methoden
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        fpsLimitDropdown.onValueChanged.AddListener(OnFPSLimitChanged);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        
        //beim ersten Mal initialisieren
        if (!eventsInitialized)
        {
            InitializeEvents();
            eventsInitialized = true;
        }
        
        LoadSavedSettings();
        
        //Cursor freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        //prüft Apply,Reset Button
        UpdateButtonStates();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        if (Time.timeScale > 0) //wenn Spiel läuft 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
    }

    public void ApplySettings()
    {
        //Settings anwenden
        SettingsManager.Instance.SetMasterVolume(masterVolumeSlider.value);
        SettingsManager.Instance.SetMusicVolume(musicVolumeSlider.value);
        SettingsManager.Instance.SetSFXVolume(sfxVolumeSlider.value);
        SettingsManager.Instance.SetMouseSensitivity(sensitivitySlider.value);
        SettingsManager.Instance.SetFullscreen(fullscreenToggle.isOn);
        SettingsManager.Instance.SetResolution(resolutionDropdown.value);
        SettingsManager.Instance.SetVSync(vsyncToggle.isOn);
        
        int[] fpsLimits = { 30, 60, 120, 144, 240, -1 };
        SettingsManager.Instance.SetFPSLimit(fpsLimits[fpsLimitDropdown.value]);
        
        //speichern
        SettingsManager.Instance.SaveSettings();
        
        //Apply und Reset Button werden geprüft
        UpdateButtonStates();
    }

    public void CancelSettings()
    {
        //lädt die alten Settings und verwirft die neuen Settings (kein Apply)
        LoadSavedSettings();
        CloseSettings();
    }

    public void ResetSettings()
    {
        //UI auf Defaults setzen (ohne anzuwenden! -> erst mit Apply)
        SetUIToDefaults();
        UpdateButtonStates();
    }

    private void LoadSavedSettings()
    {
        //gespeicherte Werte laden
        masterVolumeSlider.value = SettingsManager.Instance.GetMasterVolume();
        musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();
        sfxVolumeSlider.value = SettingsManager.Instance.GetSFXVolume();
        sensitivitySlider.value = SettingsManager.Instance.GetMouseSensitivity();
        fullscreenToggle.isOn = SettingsManager.Instance.GetFullscreen();
        resolutionDropdown.value = SettingsManager.Instance.GetResolutionIndex();
        vsyncToggle.isOn = SettingsManager.Instance.GetVSync();
        fpsLimitDropdown.value = GetFPSLimitDropdownIndex(SettingsManager.Instance.GetFPSLimit());

        //Texte updaten
        UpdateVolumeText(masterVolumeText, masterVolumeSlider.value);
        UpdateVolumeText(musicVolumeText, musicVolumeSlider.value);
        UpdateVolumeText(sfxVolumeText, sfxVolumeSlider.value);
        UpdateSensitivityText(sensitivityText, sensitivitySlider.value);
    }

    private void SetUIToDefaults()
    {
        masterVolumeSlider.value = DEFAULT_MASTER_VOLUME;
        musicVolumeSlider.value = DEFAULT_MUSIC_VOLUME;
        sfxVolumeSlider.value = DEFAULT_SFX_VOLUME;
        sensitivitySlider.value = DEFAULT_SENSITIVITY;
        fullscreenToggle.isOn = DEFAULT_FULLSCREEN;
        resolutionDropdown.value = resolutions.Length - 1; //höchste Auflösung
        vsyncToggle.isOn = DEFAULT_VSYNC;
        fpsLimitDropdown.value = DEFAULT_FPS_DROPDOWN_INDEX;

        //Texte updaten
        UpdateVolumeText(masterVolumeText, DEFAULT_MASTER_VOLUME);
        UpdateVolumeText(musicVolumeText, DEFAULT_MUSIC_VOLUME);
        UpdateVolumeText(sfxVolumeText, DEFAULT_SFX_VOLUME);
        UpdateSensitivityText(sensitivityText, DEFAULT_SENSITIVITY);
    }

    private void UpdateButtonStates()
    {
        //wenn UI modified -> true -> Apply button aktiv
        bool settingsChanged = IsUIModified();
        applyButton.interactable = settingsChanged;

        //wenn ein UI Element nicht default ist -> !false -> true -> Reset aktiv
        bool isNotDefault = !IsUIAtDefaults();
        resetButton.interactable = isNotDefault;
    }

    private bool IsUIModified()
    {
        //vergleicht UI mit gespeicherten Werten
        //wenn ein UI Element anders ist -> true (apply wird aktiv)
        if (!Mathf.Approximately(masterVolumeSlider.value, SettingsManager.Instance.GetMasterVolume())) return true;
        if (!Mathf.Approximately(musicVolumeSlider.value, SettingsManager.Instance.GetMusicVolume())) return true;
        if (!Mathf.Approximately(sfxVolumeSlider.value, SettingsManager.Instance.GetSFXVolume())) return true;
        if (!Mathf.Approximately(sensitivitySlider.value, SettingsManager.Instance.GetMouseSensitivity())) return true;
        if (fullscreenToggle.isOn != SettingsManager.Instance.GetFullscreen()) return true;
        if (resolutionDropdown.value != SettingsManager.Instance.GetResolutionIndex()) return true;
        if (vsyncToggle.isOn != SettingsManager.Instance.GetVSync()) return true;
        
        int[] fpsLimits = { 30, 60, 120, 144, 240, -1 };
        if (fpsLimits[fpsLimitDropdown.value] != SettingsManager.Instance.GetFPSLimit()) return true;

        return false;
    }

    private bool IsUIAtDefaults()
    {
        //vergleicht UI mit Default Werten
        //wenn ein UI Element anders ist als Default -> false
        bool masterCheck = Mathf.Approximately(masterVolumeSlider.value, DEFAULT_MASTER_VOLUME);
        bool musicCheck = Mathf.Approximately(musicVolumeSlider.value, DEFAULT_MUSIC_VOLUME);
        bool sfxCheck = Mathf.Approximately(sfxVolumeSlider.value, DEFAULT_SFX_VOLUME);
        bool sensCheck = Mathf.Approximately(sensitivitySlider.value, DEFAULT_SENSITIVITY);
        bool fullscreenCheck = fullscreenToggle.isOn == DEFAULT_FULLSCREEN;
        bool resCheck = resolutionDropdown.value == resolutions.Length - 1;
        bool vsyncCheck = vsyncToggle.isOn == DEFAULT_VSYNC;
        bool fpsCheck = fpsLimitDropdown.value == DEFAULT_FPS_DROPDOWN_INDEX;

        //alles muss true sein, damit reset nicht aktiv ist
        return masterCheck && musicCheck && sfxCheck && sensCheck && 
               fullscreenCheck && resCheck && vsyncCheck && fpsCheck;
    }

    private void OnMasterVolumeChanged(float value)
    {
        UpdateVolumeText(masterVolumeText, value);
        UpdateButtonStates();
    }

    private void OnMusicVolumeChanged(float value)
    {
        UpdateVolumeText(musicVolumeText, value);
        UpdateButtonStates();
    }

    private void OnSFXVolumeChanged(float value)
    {
        UpdateVolumeText(sfxVolumeText, value);
        UpdateButtonStates();
    }

    private void OnSensitivityChanged(float value)
    {
        UpdateSensitivityText(sensitivityText, value);
        UpdateButtonStates();
    }

    private void OnFullscreenChanged(bool value)
    {
        UpdateButtonStates();
    }

    private void OnResolutionChanged(int value)
    {
        UpdateButtonStates();
    }

    private void OnVSyncChanged(bool value)
    {
        UpdateButtonStates();
    }

    private void OnFPSLimitChanged(int value)
    {
        UpdateButtonStates();
    }

    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        text.text = Mathf.RoundToInt(value * 100).ToString();
    }

    private void UpdateSensitivityText(TextMeshProUGUI text, float value)
    {
        text.text = value.ToString("F2");
    }

    private void SetupResolutions()
    {
        resolutions = SettingsManager.Instance.GetResolutions();
        resolutionDropdown.ClearOptions();

        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }

        resolutionDropdown.AddOptions(options);
    }

    private void SetupFPSLimit()
    {
        fpsLimitDropdown.ClearOptions();
        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>
        {
            "30 FPS",
            "60 FPS",
            "120 FPS",
            "144 FPS",
            "240 FPS",
            "Unlimited"
        };
        fpsLimitDropdown.AddOptions(options);
        
        //füllt FPS DropDown Menü ohne Apply Button zu triggern
        fpsLimitDropdown.SetValueWithoutNotify(DEFAULT_FPS_DROPDOWN_INDEX);
    }

    private int GetFPSLimitDropdownIndex(int fpsLimit)
    {
        switch (fpsLimit)
        {
            case 30: return 0;
            case 60: return 1;
            case 120: return 2;
            case 144: return 3;
            case 240: return 4;
            default: return 5; //unlimited
        }
    }
}
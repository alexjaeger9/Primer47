using UnityEngine;
using UnityEngine.UI; // Wichtig für UI Zugriff

public class UIButtonSounds : MonoBehaviour
{
    public AudioSource uiAudioSource;
    public AudioClip clickSound;

    void Start()
    {
        // Der "true" Parameter sorgt dafür, dass er AUCH Buttons findet, 
        // die gerade unsichtbar sind (z.B. im Pause-Menü, das am Start aus ist).
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(PlaySound);
        }
    }

    void PlaySound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            // ignoreListenerPause = true sorgt dafür, dass der Sound auch spielt, 
            // wenn das Spiel komplett pausiert ist (falls du AudioListener pausierst)
            uiAudioSource.ignoreListenerPause = true; 
            uiAudioSource.pitch = 1f; // Immer normaler Pitch für UI
            uiAudioSource.PlayOneShot(clickSound);
        }
    }
}
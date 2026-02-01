using UnityEngine;
using UnityEngine.UI;

public class UIButtonSounds : MonoBehaviour
{
    public AudioSource uiAudioSource;
    public AudioClip clickSound;

    void Start()
    {
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
            uiAudioSource.ignoreListenerPause = true; 
            uiAudioSource.pitch = 1f;
            uiAudioSource.PlayOneShot(clickSound);
        }
    }
}
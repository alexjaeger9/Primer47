using UnityEngine;
using UnityEngine.UI;

public class VignetteController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image vignetteImage;

    [Header("Settings")]
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.7f;

    private bool isActive = false;

    void Start()
    {
        // Zu Beginn unsichtbar machen
        SetVignetteAlpha(0);

    }

    void Update()
    {
        if (!isActive || vignetteImage == null) return;

        float lerp = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, lerp);

        SetVignetteAlpha(currentAlpha);
    }

    public void SetActive(bool state)
    {
        isActive = state;
        
        // Wenn deaktiviert, sofort ausblenden
        if (!isActive)
        {
            SetVignetteAlpha(0);
        }
    }

    private void SetVignetteAlpha(float alpha)
    {

        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;

    }
}
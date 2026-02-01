using UnityEngine;
using UnityEngine.UI;

public class VignetteController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image vignetteImage;

    [Header("Settings")]
    [SerializeField] private float minPulseSpeed = 3f;
    [SerializeField] private float maxPulseSpeed = 12f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.7f;

    private bool isActive = false;
    private float currentPulseSpeed;
    private bool isDead = false;

    void Start()
    {
        SetVignetteAlpha(0);
        currentPulseSpeed = minPulseSpeed;
    }

    void Update()
    {
        if (isDead)
        {
            SetVignetteAlpha(maxAlpha);
            return;
        }

        if (!isActive || vignetteImage == null) return;

        float lerp = Mathf.PingPong(Time.time * currentPulseSpeed, 1f);
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, lerp);

        SetVignetteAlpha(currentAlpha);
    }

    public void SetActive(bool state)
    {
        isActive = state;
        isDead = false;
        
        if (!isActive)
        {
            SetVignetteAlpha(0);
        }
    }

    public void UpdatePulseSpeed(float progress)
    {
        currentPulseSpeed = Mathf.Lerp(minPulseSpeed, maxPulseSpeed, progress);
    }

    public void TriggerPermanentVignette()
    {
        isDead = true;
    }

    private void SetVignetteAlpha(float alpha)
    {
        if (vignetteImage == null) return;

        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class VignetteController : MonoBehaviour
{
    public Image vignetteImage;
    
    [Header("Settings")]
    public Color pressureColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Grau pulsierend
    public Color deathColor = new Color(1f, 0f, 0f, 1f);    // Rot fest
    public float pulseSpeed = 5f; 

    private bool isLowTime = false;
    private bool isDead = false;

    private void Start()
    {
        if (vignetteImage == null) vignetteImage = GetComponent<Image>();
        
        // Startet unsichtbar
        if (vignetteImage != null)
        {
            vignetteImage.color = new Color(0, 0, 0, 0);
        }
    }

    private void Update()
    {
        if (vignetteImage == null) return;

        // 1. Tod (Rot & Stark)
        if (isDead)
        {
            Color target = deathColor;
            target.a = 1.0f;
            vignetteImage.color = Color.Lerp(vignetteImage.color, target, Time.deltaTime * 5f);
        }
        else if (isLowTime)
        {
            float alpha = 0.85f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f; 
            
            Color target = pressureColor;
            target.a = alpha;
            
            // Lerp etwas schneller (15f), damit es "härter" wirkt
            vignetteImage.color = Color.Lerp(vignetteImage.color, target, Time.deltaTime * 15f);
        }
        // 3. Normal (Unsichtbar)
        else
        {
            Color target = vignetteImage.color;
            target.a = 0f;
            vignetteImage.color = Color.Lerp(vignetteImage.color, target, Time.deltaTime * 5f);
        }
    }

    public void SetLowTime(bool active)
    {
        if (isDead) return; // Wenn tot, ist Zeitdruck egal
        isLowTime = active;
    }

    public void TriggerDeath()
    {
        isLowTime = false;
        isDead = true;
    }
    
    public void ResetVignette()
    {
        isLowTime = false;
        isDead = false;
        // Sofort ausblenden beim Reset
        if(vignetteImage != null)
             vignetteImage.color = new Color(0,0,0,0);
    }
}
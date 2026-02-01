using UnityEngine;
using UnityEngine.UI;

public class VignetteController : MonoBehaviour
{
    public Image vignetteImage;
    
    [Header("Settings")]
    public Color pulseColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Grau pulsierend
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

        // Rot
        if (isDead)
        {
            Color target = deathColor;
            target.a = 1.0f;
            vignetteImage.color = Color.Lerp(vignetteImage.color, target, Time.deltaTime * 5f);
        }
        // grau pulsierend
        else if (isLowTime)
        {
            float alpha = 0.85f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f; 
            
            Color target = pulseColor;
            target.a = alpha;
            
            vignetteImage.color = Color.Lerp(vignetteImage.color, target, Time.deltaTime * 15f);
        }
        // Unsichtbar
        else
        {
            Color target = vignetteImage.color;
            target.a = 0f;
            vignetteImage.color = Color.Lerp(vignetteImage.color, target, Time.deltaTime * 5f);
        }
    }

    public void SetLowTime(bool active)
    {
        if (isDead) return;
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
        // ausblenden beim Reset
        if(vignetteImage != null)
             vignetteImage.color = new Color(0,0,0,0);
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUDAnimationController : MonoBehaviour
{
    [System.Serializable]
    public class HUDElement
    {
        public string name;
        public GameObject element;
        public Text textComponent;
        
        [Header("Slide Animation")]
        public bool enableSlideIn = true;
        public SlideDirection slideInDirection = SlideDirection.Down;
        public float slideInDuration = 0.3f;
        
        public bool enableSlideOut = false;
        public SlideDirection slideOutDirection = SlideDirection.Up;
        public float slideOutDuration = 0.5f;
        public float slideDistance = 100f;
        
        [Header("Shake Settings")]
        public bool enableShake = false;
        public float shakeDuration = 0.2f;
        public float shakeStrength = 10f;
        
        [Header("Color Flash Settings")]
        public bool enableColorFlash = false;
        public Color flashColor = Color.yellow;
        public float colorFlashDuration = 0.3f;
        
        [HideInInspector] public Vector2 originalPosition;
        [HideInInspector] public Color originalTextColor;
        [HideInInspector] public Color originalPanelColor;
        [HideInInspector] public bool isAnimating = false;
    }
    
    public enum SlideDirection { Up, Down }
    
    [Header("HUD Elements")]
    public List<HUDElement> hudElements = new List<HUDElement>();
    
    private void Start()
    {
        foreach (HUDElement elem in hudElements)
        {
            if (elem.element == null) continue;
            
            RectTransform rect = elem.element.GetComponent<RectTransform>();
            elem.originalPosition = rect.anchoredPosition;
            
            if (elem.textComponent != null)
            {
                elem.originalTextColor = elem.textComponent.color;
            }
            
            if (elem.element.GetComponent<CanvasGroup>() == null)
            {
                elem.element.AddComponent<CanvasGroup>();
            }
        }
    }
    
    public void SlideIn(string elementName)
    {
        HUDElement elem = FindElement(elementName);
        if (elem != null && elem.enableSlideIn)
        {
            StartCoroutine(SlideInElement(elem));
        }
    }
    
    public void SlideOut(string elementName)
    {
        HUDElement elem = FindElement(elementName);
        if (elem != null && elem.enableSlideOut)
        {
            StartCoroutine(SlideOutElement(elem));
        }
    }
    
    public void Shake(string elementName)
    {
        HUDElement elem = FindElement(elementName);
        if (elem != null && elem.enableShake)
        {
            StartCoroutine(ShakeElement(elem));
        }
    }
    
    public void ColorFlash(string elementName)
    {
        HUDElement elem = FindElement(elementName);
        if (elem != null && elem.enableColorFlash)
        {
            StartCoroutine(ColorFlashElement(elem));
        }
    }
    
    public void ShakeAndFlash(string elementName)
    {
        HUDElement elem = FindElement(elementName);
        if (elem != null)
        {
            if (elem.enableShake) StartCoroutine(ShakeElement(elem));
            if (elem.enableColorFlash) StartCoroutine(ColorFlashElement(elem));
        }
    }
    
    //Timer-spezifisch: Farbe ändern
    public void SetTimerColor(Color color)
    {
        HUDElement elem = FindElement("Timer");
        if (elem != null && elem.textComponent != null)
        {
            elem.textComponent.color = color;
            elem.originalTextColor = color; //update original für Shake
        }
    }
    
    //Extra starker Shake (für Timer bei 0)
    public void BigShake(string elementName)
    {
        HUDElement elem = FindElement(elementName);
        StartCoroutine(ShakeElement(elem, elem.shakeStrength * 5f, elem.shakeDuration * 1.5f));
    }
    
    private HUDElement FindElement(string name)
    {
        return hudElements.Find(e => e.name == name);
    }
    
    private IEnumerator SlideInElement(HUDElement elem)
    {
        if (elem.isAnimating || elem.element == null) yield break;
        elem.isAnimating = true;
        
        RectTransform rect = elem.element.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = elem.element.GetComponent<CanvasGroup>();
        
        Vector2 startPos = elem.originalPosition + GetSlideOffset(elem.slideInDirection, elem.slideDistance);
        rect.anchoredPosition = startPos;
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        
        while (elapsed < elem.slideInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / elem.slideInDuration);
            
            rect.anchoredPosition = Vector2.Lerp(startPos, elem.originalPosition, t);
            canvasGroup.alpha = t;
            
            yield return null;
        }
        
        rect.anchoredPosition = elem.originalPosition;
        canvasGroup.alpha = 1f;
        elem.isAnimating = false;
    }
    
    private IEnumerator SlideOutElement(HUDElement elem)
    {
        if (elem.isAnimating || elem.element == null) yield break;
        elem.isAnimating = true;
        
        RectTransform rect = elem.element.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = elem.element.GetComponent<CanvasGroup>();
        
        Vector2 endPos = elem.originalPosition + GetSlideOffset(elem.slideOutDirection, elem.slideDistance);
        
        float elapsed = 0f;
        
        while (elapsed < elem.slideOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / elem.slideOutDuration);
            
            rect.anchoredPosition = Vector2.Lerp(elem.originalPosition, endPos, t);
            canvasGroup.alpha = 1f - t;
            
            yield return null;
        }
        
        rect.anchoredPosition = endPos;
        canvasGroup.alpha = 0f;
        elem.isAnimating = false;
    }
    
    private IEnumerator ShakeElement(HUDElement elem, float? customStrength = null, float? customDuration = null)
    {
        if (elem.element == null) yield break;
        
        RectTransform rect = elem.element.GetComponent<RectTransform>();
        float strength = customStrength ?? elem.shakeStrength;
        float duration = customDuration ?? elem.shakeDuration;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float currentStrength = Mathf.Lerp(strength, 0f, elapsed / duration);
            
            float x = Random.Range(-1f, 1f) * currentStrength;
            float y = Random.Range(-1f, 1f) * currentStrength;
            
            rect.anchoredPosition = elem.originalPosition + new Vector2(x, y);
            
            yield return null;
        }
        
        rect.anchoredPosition = elem.originalPosition;
    }
    
    private IEnumerator ColorFlashElement(HUDElement elem)
    {
        float elapsed = 0f;
        
        while (elapsed < elem.colorFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / elem.colorFlashDuration;
            float pingPong = Mathf.PingPong(t * 2f, 1f);
            
            if (elem.textComponent != null)
            {
                elem.textComponent.color = Color.Lerp(elem.originalTextColor, elem.flashColor, pingPong);
            }

            
            yield return null;
        }
        
        if (elem.textComponent != null) elem.textComponent.color = elem.originalTextColor;
    }
    
    private Vector2 GetSlideOffset(SlideDirection direction, float distance)
    {
        switch (direction)
        {
            case SlideDirection.Up: return new Vector2(0, distance);
            case SlideDirection.Down: return new Vector2(0, -distance);
            default: return Vector2.zero;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDAnimationController : MonoBehaviour
{
    [Header("References")]
    public Text targetsText; // Zuweisen im Inspector
    public Text timerText;   // Zuweisen im Inspector

    [Header("Global Settings")]
    public float slideDuration = 0.5f;
    public float slideDistance = 100f; // Positive Y-Richtung (Up)
    public float shakeDuration = 0.2f;
    public float shakeStrength = 10f;

    [Header("Special Settings")]
    public Color targetFlashColor = Color.yellow;
    public float flashDuration = 0.3f;

    // Interne Speicher
    private Vector2 targetsOriginalPos;
    private Vector2 timerOriginalPos;
    private Color targetsOriginalColor;
    
    // Cache Components
    private RectTransform targetsRect;
    private RectTransform timerRect;
    private CanvasGroup targetsCanvas;
    private CanvasGroup timerCanvas;

    private void Start()
    {
        // Setup Targets Text
        SetupElement(targetsText, out targetsRect, out targetsCanvas, out targetsOriginalPos);
        if (targetsText != null) targetsOriginalColor = targetsText.color;
        
        // Setup Timer Text (Farbe speichern wir hier nicht speziell für Reset, da Timer Logik variiert)
        SetupElement(timerText, out timerRect, out timerCanvas, out timerOriginalPos);
    }

    private void SetupElement(Text textComp, out RectTransform rect, out CanvasGroup cg, out Vector2 origPos)
    {
        if (textComp == null) 
        {
            rect = null; cg = null; origPos = Vector2.zero;
            return;
        }

        rect = textComp.GetComponent<RectTransform>();
        cg = textComp.GetComponent<CanvasGroup>();
        
        if (cg == null) cg = textComp.gameObject.AddComponent<CanvasGroup>();

        origPos = rect.anchoredPosition;
    }

    // ----------------------------------------------------------------
    // PUBLIC Methods
    // ----------------------------------------------------------------

    public void SlideInAll()
    {
        if(targetsRect != null) StartCoroutine(SlideRoutine(targetsRect, targetsCanvas, targetsOriginalPos, true));
        if(timerRect != null) StartCoroutine(SlideRoutine(timerRect, timerCanvas, timerOriginalPos, true));
    }

    public void SlideOutAll()
    {
        if(targetsRect != null) StartCoroutine(SlideRoutine(targetsRect, targetsCanvas, targetsOriginalPos, false));
        if(timerRect != null) StartCoroutine(SlideRoutine(timerRect, timerCanvas, timerOriginalPos, false));
    }

    // --- TARGETS ---

    public void ShakeTargets()
    {
        if(targetsRect != null) StartCoroutine(ShakeRoutine(targetsRect, targetsOriginalPos, shakeStrength, shakeDuration));
    }

    public void ShakeAndFlashTargets()
    {
        if(targetsRect != null) 
        {
            StartCoroutine(ShakeRoutine(targetsRect, targetsOriginalPos, shakeStrength, shakeDuration));
            StartCoroutine(FlashRoutine(targetsText, targetsOriginalColor, targetFlashColor));
        }
    }

    // --- TIMER ---

    public void ShakeTimer()
    {
        if(timerRect != null) StartCoroutine(ShakeRoutine(timerRect, timerOriginalPos, shakeStrength, shakeDuration));
    }

    public void BigShakeTimer()
    {
        if(timerRect != null) StartCoroutine(ShakeRoutine(timerRect, timerOriginalPos, shakeStrength * 5f, shakeDuration * 1.5f));
    }

    public void SetTimerColor(Color color)
    {
        if(timerText != null) timerText.color = color;
    }

    // ----------------------------------------------------------------
    // Coroutines
    // ----------------------------------------------------------------

    private IEnumerator SlideRoutine(RectTransform rect, CanvasGroup cg, Vector2 targetPos, bool slideIn)
    {
        Vector2 offset = new Vector2(0, slideDistance); 
        Vector2 hiddenPos = targetPos + offset; 

        Vector2 start = slideIn ? hiddenPos : targetPos;
        Vector2 end = slideIn ? targetPos : hiddenPos;
        
        float startAlpha = slideIn ? 0f : 1f;
        float endAlpha = slideIn ? 1f : 0f;

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);

            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        rect.anchoredPosition = end;
        cg.alpha = endAlpha;
    }

    private IEnumerator ShakeRoutine(RectTransform rect, Vector2 basePos, float strength, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float currentStrength = Mathf.Lerp(strength, 0f, elapsed / duration);
            
            float x = Random.Range(-1f, 1f) * currentStrength;
            float y = Random.Range(-1f, 1f) * currentStrength;

            rect.anchoredPosition = basePos + new Vector2(x, y);

            yield return null;
        }
        rect.anchoredPosition = basePos;
    }

    private IEnumerator FlashRoutine(Text textComp, Color baseColor, Color flashCol)
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / flashDuration;
            float blend = Mathf.PingPong(t * 2f, 1f); 
            
            textComp.color = Color.Lerp(baseColor, flashCol, blend);
            yield return null;
        }
        textComp.color = baseColor;
    }
}
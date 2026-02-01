using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDAnimationController : MonoBehaviour
{
    [Header("References")]
    public Text targetsText;
    public Text timerText;  

    [Header("Settings")]
    public float slideDuration = 0.5f;
    public float slideDistance = 100f; //positive Y Richtung (Up)
    public float shakeDuration = 0.2f;
    public float shakeStrength = 10f;
    public Color targetFlashColor = Color.yellow;
    public float flashDuration = 0.3f;

    //interne Speicher
    private Vector2 targetsOriginalPos;
    private Vector2 timerOriginalPos;
    private Color targetsOriginalColor;
    
    //cache
    private RectTransform targetsRect;
    private RectTransform timerRect;
    private CanvasGroup targetsCanvas;
    private CanvasGroup timerCanvas;

    private void Start()
    {
        //setup Targets Text
        SetupElement(targetsText, out targetsRect, out targetsCanvas, out targetsOriginalPos);
        targetsOriginalColor = targetsText.color;
        
        // Setup Timer Text (Farbe speichern wir hier nicht speziell für Reset, da Timer Logik variiert)
        SetupElement(timerText, out timerRect, out timerCanvas, out timerOriginalPos);
    }

    private void SetupElement(Text textComp, out RectTransform rect, out CanvasGroup cg, out Vector2 origPos)
    {
        rect = textComp.GetComponent<RectTransform>();
        cg = textComp.GetComponent<CanvasGroup>();
        origPos = rect.anchoredPosition;
    }

    //Slide All Hilfs-Methoden
    public void SlideInAll()
    {
        StartCoroutine(SlideRoutine(targetsRect, targetsCanvas, targetsOriginalPos, true));
        StartCoroutine(SlideRoutine(timerRect, timerCanvas, timerOriginalPos, true));
    }

    public void SlideOutAll()
    {
        StartCoroutine(SlideRoutine(targetsRect, targetsCanvas, targetsOriginalPos, false));
        StartCoroutine(SlideRoutine(timerRect, timerCanvas, timerOriginalPos, false));
    }

    //Shake und Flash Hilfs-Methoden für Target
    public void ShakeTargets()
    {
        StartCoroutine(ShakeRoutine(targetsRect, targetsOriginalPos, shakeStrength, shakeDuration));
    }

    public void ShakeAndFlashTargets()
    {
        StartCoroutine(ShakeRoutine(targetsRect, targetsOriginalPos, shakeStrength, shakeDuration));
        StartCoroutine(FlashRoutine(targetsText, targetsOriginalColor, targetFlashColor));
    }

    //Shake und Color Hilfs-Methoden für Timer
    public void ShakeTimer()
    {
        StartCoroutine(ShakeRoutine(timerRect, timerOriginalPos, shakeStrength, shakeDuration));
    }

    public void BigShakeTimer()
    {
        StartCoroutine(ShakeRoutine(timerRect, timerOriginalPos, shakeStrength * 5f, shakeDuration * 1.5f));
    }

    public void SetTimerColor(Color color)
    {
        timerText.color = color;
    }

    //Coroutinen
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
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
    public float shakeDuration = 0.2f;
    public float shakeStrength = 10f;
    public Color targetFlashColor = Color.yellow;
    public float flashDuration = 0.3f;

    //Speicher für Original Werte
    private Vector2 originalTargetPos;
    private Vector2 originalTimerPos;
    private Color originalTargetsCol;
    
    //für performance einmal holen anstatt immer GetComponent<>
    private RectTransform targetsRect;
    private RectTransform timerRect;
    private CanvasGroup targetsCanvas;
    private CanvasGroup timerCanvas;

    private void Start()
    {
        //Target und timer Setup
        targetsRect = targetsText.GetComponent<RectTransform>();
        targetsCanvas = targetsText.GetComponent<CanvasGroup>();
        originalTargetPos = targetsRect.anchoredPosition;
        originalTargetsCol = targetsText.color;

        timerRect = timerText.GetComponent<RectTransform>();
        timerCanvas = timerText.GetComponent<CanvasGroup>();
        originalTimerPos = timerRect.anchoredPosition;
    }

    //Slide All Hilfs-Methoden
    public void SlideInAll()
    {
        StartCoroutine(SlideRoutine(targetsRect, targetsCanvas, originalTargetPos, true));
        StartCoroutine(SlideRoutine(timerRect, timerCanvas, originalTimerPos, true));
    }

    public void SlideOutAll()
    {
        StartCoroutine(SlideRoutine(targetsRect, targetsCanvas, originalTargetPos, false));
        StartCoroutine(SlideRoutine(timerRect, timerCanvas, originalTimerPos, false));
    }

    //Shake und Flash Hilfs-Methoden für Target
    public void ShakeTargets()
    {
        StartCoroutine(ShakeRoutine(targetsRect, originalTargetPos, shakeStrength, shakeDuration));
    }

    public void ShakeAndFlashTargets()
    {
        StartCoroutine(ShakeRoutine(targetsRect, originalTargetPos, shakeStrength, shakeDuration));
        StartCoroutine(FlashRoutine(targetsText, originalTargetsCol, targetFlashColor));
    }

    //Shake und Color Hilfs-Methoden für Timer
    public void ShakeTimer()
    {
        StartCoroutine(ShakeRoutine(timerRect, originalTimerPos, shakeStrength, shakeDuration));
    }

    public void BigShakeTimer()
    {
        StartCoroutine(ShakeRoutine(timerRect, originalTimerPos, shakeStrength * 5f, shakeDuration * 1.5f));
    }

    public void SetTimerColor(Color color)
    {
        timerText.color = color;
    }

    //Coroutinen
    private IEnumerator SlideRoutine(RectTransform rect, CanvasGroup cg, Vector2 targetPosition, bool slideIn)
    {
        Vector2 offset = new Vector2(0, 100f); //100f nach oben y richtung
        Vector2 hiddenPosition = targetPosition + offset; 

        //gucken ob nach oben oder unten
        Vector2 start = slideIn ? hiddenPosition : targetPosition;
        Vector2 end = slideIn ? targetPosition : hiddenPosition;
        
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
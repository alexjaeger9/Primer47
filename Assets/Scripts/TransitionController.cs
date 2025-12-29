using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Oben hinzufügen!

public class TransitionController : MonoBehaviour
{
    public Image transitionImage;
    public float fadeDuration = 1f;
    public bool startBlack = false;
    
    private void Start()
    {
        if (startBlack)
        {
            SetAlpha(1f); // Schwarz
            StartCoroutine(QuickFadeOut());
        }
        else
        {
            SetAlpha(0f); // Transparent
        }
    }
    
    private IEnumerator QuickFadeOut()
    {
        yield return new WaitForSeconds(0.1f); // Kurze Pause
        yield return FadeOut(1f); // Fade out
    }
    
    //zu schwarz
    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(0f, 1f, duration);
    }
    
    //zu transparent
    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(1f, 0f, duration);
    }
    
    //Haupt Fade Methode
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetAlpha(alpha);
            yield return null;
        }
        
        SetAlpha(endAlpha);
    }
    
    public void SetAlpha(float alpha)
    {
        Color color = transitionImage.color;
        color.a = alpha;
        transitionImage.color = color;
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Text buttonText;
    public Outline textOutline;
    
    [Header("Colors (anpassbar pro Button)")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.6f, 0.6f);
    public Color clickColor = new Color(0.8f, 0.2f, 0.2f);
    
    // Alle anderen Werte sind immer gleich → hardcoded
    private const float HOVER_SCALE = 1.15f;
    private const float SCALE_SPEED = 15f;
    private const float COLOR_SPEED = 10f;
    private const float HOVER_ROTATION = 3f;
    private const float SHAKE_DURATION = 0.15f;
    private const float SHAKE_STRENGTH = 8f;
    
    private static readonly Vector2 NORMAL_OUTLINE = new Vector2(1, -1);
    private static readonly Vector2 HOVER_OUTLINE = new Vector2(2, -2);
    
    private Vector3 targetScale;
    private Color targetColor;
    private Quaternion targetRotation;
    private Vector2 targetOutlineDistance;
    private Coroutine shakeCoroutine;
    private Vector3 originalPosition;

    private void Start()
    {
        targetScale = Vector3.one;
        targetColor = normalColor;
        targetRotation = Quaternion.identity;
        targetOutlineDistance = NORMAL_OUTLINE;
        originalPosition = buttonText.transform.localPosition;
        
        buttonText.color = normalColor;
        textOutline.effectDistance = NORMAL_OUTLINE;
    }

    private void Update()
    {
        // Elastic Scale
        float t = 1f - Mathf.Pow(0.001f, SCALE_SPEED * Time.unscaledDeltaTime);
        buttonText.transform.localScale = Vector3.Lerp(buttonText.transform.localScale, targetScale, t);
        
        // Smooth Color
        buttonText.color = Color.Lerp(buttonText.color, targetColor, COLOR_SPEED * Time.unscaledDeltaTime);
        
        // Smooth Rotation
        buttonText.transform.localRotation = Quaternion.Lerp(
            buttonText.transform.localRotation,
            targetRotation,
            SCALE_SPEED * Time.unscaledDeltaTime
        );
        
        // Outline Distance
        textOutline.effectDistance = Vector2.Lerp(
            textOutline.effectDistance,
            targetOutlineDistance,
            SCALE_SPEED * Time.unscaledDeltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * HOVER_SCALE;
        targetColor = hoverColor;
        targetRotation = Quaternion.Euler(0, 0, HOVER_ROTATION);
        targetOutlineDistance = HOVER_OUTLINE;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
        targetColor = normalColor;
        targetRotation = Quaternion.identity;
        targetOutlineDistance = NORMAL_OUTLINE;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        targetColor = clickColor;
        
        if (!gameObject.activeInHierarchy) return; // Fix für deaktivierten Button
        
        if (shakeCoroutine != null) 
            StopCoroutine(shakeCoroutine);
        
        shakeCoroutine = StartCoroutine(ShakeEffect());
    }

    private IEnumerator ShakeEffect()
    {
        float elapsed = 0f;
        
        while (elapsed < SHAKE_DURATION)
        {
            float strength = Mathf.Lerp(SHAKE_STRENGTH, 0f, elapsed / SHAKE_DURATION);
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;
            
            buttonText.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        buttonText.transform.localPosition = originalPosition;
    }
}
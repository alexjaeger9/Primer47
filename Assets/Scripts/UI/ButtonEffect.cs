using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Text buttonText;
    public Outline textOutline; 
    
    [Header("Scale Settings")]
    public float normalScale = 1f;
    public float hoverScale = 1.15f;
    public float scaleSpeed = 15f;
    
    [Header("Color")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.6f, 0.6f);
    public Color clickColor = new Color(0.8f, 0.2f, 0.2f);
    public float colorSpeed = 10f;
    
    [Header("Rotation")]
    public bool enableRotation = true;
    public float hoverRotation = 3f;
    
    [Header("Shake")]
    public bool enableShake = true;
    public float shakeDuration = 0.15f;
    public float shakeStrength = 8f;
    
    [Header("Outline")]
    public bool enableOutlineGrow = true;
    public Vector2 normalOutlineDistance = new Vector2(1, -1);
    public Vector2 hoverOutlineDistance = new Vector2(2, -2);
    
    private Vector3 targetScale;
    private Color targetColor;
    private Quaternion targetRotation;
    private Vector2 targetOutlineDistance;
    private Coroutine shakeCoroutine;
    private Vector3 originalPosition;
    
    private void Start()
    {
        targetScale = Vector3.one * normalScale;
        targetColor = normalColor;
        targetRotation = Quaternion.identity;
        targetOutlineDistance = normalOutlineDistance;
        originalPosition = buttonText.transform.localPosition;
        
        buttonText.color = normalColor;
        
        if (enableOutlineGrow)
        {
            textOutline.effectDistance = normalOutlineDistance;
        }
    }
    
    private void Update()
    {
        //Elastic Scale (bounce effect)
        float t = 1f - Mathf.Pow(0.001f, scaleSpeed * Time.unscaledDeltaTime);
        buttonText.transform.localScale = Vector3.Lerp(
            buttonText.transform.localScale, 
            targetScale, 
            t
        );
        
        //Smooth Color
        buttonText.color = Color.Lerp(
            buttonText.color, 
            targetColor, 
            colorSpeed * Time.unscaledDeltaTime
        );
        
        //Smooth Rotation
        if (enableRotation)
        {
            buttonText.transform.localRotation = Quaternion.Lerp(
                buttonText.transform.localRotation,
                targetRotation,
                scaleSpeed * Time.unscaledDeltaTime
            );
        }
        
        //Outline Distance
        if (enableOutlineGrow)
        {
            textOutline.effectDistance = Vector2.Lerp(
                textOutline.effectDistance,
                targetOutlineDistance,
                scaleSpeed * Time.unscaledDeltaTime
            );
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
        targetColor = hoverColor;
        
        if (enableRotation)
        {
            targetRotation = Quaternion.Euler(0, 0, hoverRotation);
        }
        
        if (enableOutlineGrow)
        {
            targetOutlineDistance = hoverOutlineDistance;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one * normalScale;
        targetColor = normalColor;
        
        if (enableRotation)
        {
            targetRotation = Quaternion.identity;
        }
        
        if (enableOutlineGrow)
        {
            targetOutlineDistance = normalOutlineDistance;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        targetColor = clickColor;
        if (!gameObject.activeInHierarchy) return; // Manh hier ist dein Fix, bitte checken
        if (enableShake)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeEffect());
        }
    }
    
    private IEnumerator ShakeEffect()
    {
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float strength = Mathf.Lerp(shakeStrength, 0f, elapsed / shakeDuration);
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;
            
            buttonText.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        buttonText.transform.localPosition = originalPosition;
    }
}
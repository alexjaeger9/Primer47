using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

//IpointerEnterHander Maus drüber
//IPointerExitHandler Maus weg
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Text buttonText;
    public Outline textOutline;
    
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.6f, 0.6f);
    public Color clickColor = new Color(0.8f, 0.2f, 0.2f);
    
    private readonly Vector2 outline = new Vector2(1, -1);
    private readonly Vector2 hover_outline = new Vector2(2, -2);
    
    private Vector3 targetScale;
    private Color targetColor;
    private Vector2 targetOutlineDistance;
    private Coroutine shakeCoroutine;
    private Vector3 originalPosition;

    private void Start()
    {
        //Targets auf Default Werte setzen
        targetScale = Vector3.one;
        targetColor = normalColor;
        targetOutlineDistance = outline;
        originalPosition = buttonText.transform.localPosition;
        
        buttonText.color = normalColor;
        textOutline.effectDistance = outline;
    }

    private void Update()
    {
        //Scale (kommt immer näher aber wird langsamer: smoother)
        buttonText.transform.localScale = Vector3.Lerp(
            buttonText.transform.localScale, //aktuelle Größe
            targetScale, //Zielgröße 
            15f * Time.unscaledDeltaTime //wv prozent in Richtung Ziel
        );
        
        //Smooth Color
        buttonText.color = Color.Lerp(
            buttonText.color, 
            targetColor, 
            10f * Time.unscaledDeltaTime
        );

        // Outline Distance
        textOutline.effectDistance = Vector2.Lerp(
            textOutline.effectDistance,
            targetOutlineDistance,
            15f * Time.unscaledDeltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * 1.15f; //1.15 hover scale
        targetColor = hoverColor;
        targetOutlineDistance = hover_outline;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
        targetColor = normalColor;
        targetOutlineDistance = outline;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        targetColor = clickColor;    
        
        //nur stoppen, wenn eine läuft, damit nicht zwei gleichzeitig laufen
        if (shakeCoroutine != null) 
            StopCoroutine(shakeCoroutine);
        
        shakeCoroutine = StartCoroutine(ShakeEffect());
    }

    private IEnumerator ShakeEffect()
    {
        float elapsed = 0f;
        
        while (elapsed < 0.15f) //shake duration
        {
            //8f shake strength, 0f end stürke, fortschritt/duration
            float strength = Mathf.Lerp(8f, 0f, elapsed / 0.15f);
            float x = Random.Range(-1f, 1f) * strength; //zufälliger Offset 
            float y = Random.Range(-1f, 1f) * strength;
            
            buttonText.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        buttonText.transform.localPosition = originalPosition;
    }
}
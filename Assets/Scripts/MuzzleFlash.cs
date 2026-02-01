using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public SpriteRenderer flashSprite;
    public Sprite[] flashVariants; // verschiedene Sprites
    public float flashDuration = 0.05f;
    public float minScale = 0.3f;
    public float maxScale = 0.5f;

    private float timer;
    private bool isFlashing;

    private void Start()
    {
        if (flashSprite != null)
            flashSprite.enabled = false;
    }

    private void Update()
    {
        if (isFlashing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                flashSprite.enabled = false;
                isFlashing = false;
            }
        }
    }

    public void PlayFlash()
    {
        //zufälliges Sprite wählen
        if (flashVariants.Length > 0)
        {
            flashSprite.sprite = flashVariants[Random.Range(0, flashVariants.Length)];
        }

        //zufällige Rotation für Variation
        transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        //zufällige Größe
        float scale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(scale, scale, scale);

        flashSprite.enabled = true;
        isFlashing = true;
        timer = flashDuration;
    }
}
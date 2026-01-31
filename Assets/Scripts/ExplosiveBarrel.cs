using UnityEngine;
using System.Collections;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public float upwardsModifier = 1.0f;

    [Header("References")]
    public GameObject solidModel;
    public GameObject shatteredModel;
    public GameObject explosionVisualSphere;
    public LayerMask damageMask;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip explosionClip;

    private Vector3[] fragmentPositions;
    private Quaternion[] fragmentRotations;
    private Rigidbody[] fragmentRbs;

    private bool hasExploded = false;

    void Awake()
    {
        // Save Debris Positions
        fragmentRbs = shatteredModel.GetComponentsInChildren<Rigidbody>(true);
        fragmentPositions = new Vector3[fragmentRbs.Length];
        fragmentRotations = new Quaternion[fragmentRbs.Length];

        for (int i = 0; i < fragmentRbs.Length; i++)
        {
            fragmentPositions[i] = fragmentRbs[i].transform.localPosition;
            fragmentRotations[i] = fragmentRbs[i].transform.localRotation;
        }
    }

    public void TakeHit()
    {
        if (!hasExploded) Explode();
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (audioSource != null && explosionClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(explosionClip);
        }

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        solidModel.SetActive(false);
        shatteredModel.SetActive(true);

        Vector3 forceOrigin = transform.position + (Vector3.down * 0.5f);

        Rigidbody[] rbs = shatteredModel.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.AddExplosionForce(explosionForce, forceOrigin, explosionRadius, upwardsModifier);
        }

        ApplyDamageInRadius();
        StartCoroutine(GrowExplosionSphere());
    }

    void ApplyDamageInRadius()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, explosionRadius, damageMask);
        foreach (Collider hit in targets)
        {
            if (hit.TryGetComponent<GhostHealth>(out var ghostHealth)) ghostHealth.TakeHit();
            if (hit.TryGetComponent<PlayerHealth>(out var playerHealth)) playerHealth.TakeDamage();
            if (hit.TryGetComponent<ExplosiveBarrel>(out var otherBarrel) && hit.gameObject != gameObject) otherBarrel.TakeHit();
        }
    }

    IEnumerator GrowExplosionSphere()
    {
        explosionVisualSphere.SetActive(true);
        Renderer rend = explosionVisualSphere.GetComponent<Renderer>();

        Material mat = rend.material;
        Color baseColor = mat.GetColor("_BaseColor");

        float timer = 0f;
        float duration = 0.3f;
        Vector3 targetScale = Vector3.one * (explosionRadius * 2f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            explosionVisualSphere.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

            baseColor.a = Mathf.Lerp(0.6f, 0f, progress);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_EmissionColor", baseColor * Mathf.Lerp(2f, 0f, progress));

            yield return null;
        }
        explosionVisualSphere.SetActive(false);
    }

    public void Respawn()
    {
        StopAllCoroutines();
        hasExploded = false;
        solidModel.SetActive(true);
        shatteredModel.SetActive(false);
        explosionVisualSphere.SetActive(false);
        GetComponent<Collider>().enabled = true;

        // reset debris to start
        for (int i = 0; i < fragmentRbs.Length; i++)
        {
            fragmentRbs[i].linearVelocity = Vector3.zero;
            fragmentRbs[i].angularVelocity = Vector3.zero;
            fragmentRbs[i].transform.localPosition = fragmentPositions[i];
            fragmentRbs[i].transform.localRotation = fragmentRotations[i];
        }
    }
}
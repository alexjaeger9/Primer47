using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public float upwardsModifier = 1.0f; // Für den "Popcorn-Effekt"

    [Header("References")]
    public GameObject solidModel;    // Hier das intakte Model reinziehen
    public GameObject shatteredModel; // Hier das Shatter-Model reinziehen

    private bool hasExploded = false;

    // Methode zum Testen (z.B. durch Mausklick)
    private void OnMouseDown()
    {
        if (!hasExploded) Explode();
    }

    public void Explode()
    {
        hasExploded = true;

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        // 1. Modelle tauschen
        solidModel.SetActive(false);
        shatteredModel.SetActive(true);

        // 2. Den Ursprung der Kraft leicht nach unten setzen
        Vector3 forceOrigin = transform.position + (Vector3.down * 0.5f);

        // 3. Alle Rigidbodys der Trümmer finden und wegpfeffern
        // WICHTIG: Deine Maya-Teile brauchen in Unity alle die Komponente "Rigidbody"
        Rigidbody[] rbs = shatteredModel.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rbs)
        {
            // Die Kraft wird auf jedes Trümmerteil angewendet
            rb.AddExplosionForce(explosionForce, forceOrigin, explosionRadius, upwardsModifier);
        }

        // 4. Schaden an Umgebung (optionaler Schritt)
        ApplyPhysicsToSurroundings(forceOrigin);

        // 5. Cleanup: Das ganze Fass nach 10 Sekunden löschen
        Destroy(gameObject, 10f);
    }

    void ApplyPhysicsToSurroundings(Vector3 origin)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            // Wir wenden die Kraft nur auf andere Objekte an, nicht auf die Trümmer (die haben wir schon)
            if (rb != null && !rb.transform.IsChildOf(shatteredModel.transform))
            {
                rb.AddExplosionForce(explosionForce, origin, explosionRadius, upwardsModifier);
            }
        }
    }
}

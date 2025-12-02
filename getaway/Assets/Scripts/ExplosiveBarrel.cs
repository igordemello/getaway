using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    public float explosionRadius = 1000f;
    public float explosionForce = 1000;
    public float damage = 75f;

    public GameObject explosionEffect;

    private bool exploded = false;

    private void OnDrawGizmosSelected() //para ver o range da explosão
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        // FX
        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        //objetos que estão no range
        Vector3 halfExtents = Vector3.one * explosionRadius;
        Quaternion rot = Quaternion.identity;

        Collider[] objects = Physics.OverlapBox(transform.position, halfExtents, rot);

        foreach (Collider col in objects)
        {
            //empurra
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            // dano relativo à distância
            float dist = Vector3.Distance(transform.position, col.transform.position);
            float pct = Mathf.Clamp01(dist / explosionRadius);
            float finalDamage = Mathf.RoundToInt(damage * (1f - pct));
            finalDamage = Mathf.Max(0, finalDamage);

            Target t = col.GetComponent<Target>() ??
                       col.GetComponentInParent<Target>() ??
                       col.GetComponentInChildren<Target>();

            if (t != null)
                t.TakeDamage(finalDamage);
        }

        Destroy(gameObject);
    }
}

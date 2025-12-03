using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 8f;
    public float explosionForce = 1000f;
    public float damage = 75f;

    [Header("FX")]
    public GameObject explosionEffect;

    private bool exploded = false;

    private bool hasBeenThrown = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void ThrownedBarrel()
    {
        hasBeenThrown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) 
            return; //caso nao tenha sido arrmessado nao vai explodir

        if (collision.collider.CompareTag("Player")) //nao explode no player 
            return;

        Explode();
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Collider[] objects = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in objects)
        {
            // aplica empurrão se tiver rigidbody
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            // calcula dano pela distância
            float dist = Vector3.Distance(transform.position, col.transform.position);
            float pct = Mathf.Clamp01(dist / explosionRadius);
            float finalDamage = Mathf.RoundToInt(damage * (1f - pct));

            // procura componente Target no objeto
            Target t =col.GetComponent<Target>() ?? col.GetComponentInParent<Target>() ?? col.GetComponentInChildren<Target>();

            ExplosiveBarrel otherBarrel =
                col.GetComponent<ExplosiveBarrel>() ??
                col.GetComponentInParent<ExplosiveBarrel>() ??
                col.GetComponentInChildren<ExplosiveBarrel>();

            if (otherBarrel != null && otherBarrel != this)
                otherBarrel.Explode();  // expplosão em cadeia

            if (t != null && finalDamage > 0)
                t.TakeDamage(finalDamage);
        }

        Destroy(gameObject);
    }
}

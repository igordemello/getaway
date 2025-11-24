using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ExplosiveArrow : MonoBehaviour
{
    [Header("Configurações de Explosão")]
    public float damage = 100f;
    public float lifeTime = 10.0f;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public GameObject explosionEffect; 

    private Rigidbody rb;
    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!hasHit && rb.linearVelocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;
        if (hasHit) return;
        hasHit = true;

        Explode(collision.contacts[0].point, collision.contacts[0].normal);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        rb.isKinematic = true;
        Destroy(gameObject, 2f);
    }

    void Explode(Vector3 position, Vector3 normal)
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, position, Quaternion.LookRotation(normal));
        }

        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce, position, explosionRadius);
            }

            Target target = hit.GetComponentInParent<Target>();
            if (target == null) target = hit.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            ShatterableGlass glass = hit.GetComponent<ShatterableGlass>();
            if (glass != null)
            {
                Vector3 dir = (hit.transform.position - position).normalized;
                ShatterableGlassInfo info = new ShatterableGlassInfo(hit.ClosestPoint(position), dir * explosionForce);
                glass.Shatter3D(info);
            }
        }
    }
}
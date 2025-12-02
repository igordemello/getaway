using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Arrow : MonoBehaviour
{
    [Header("Configurações da Flecha")]
    public float damage = 40f;

    [Tooltip("Tempo em segundos para a flecha ser destruída se não acertar nada.")]
    public float lifeTime = 15.0f;

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

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Target target = collision.gameObject.GetComponentInParent<Target>();
        if (target == null) target = collision.gameObject.GetComponent<Target>();

        if (target != null)
        {
            target.TakeDamage(damage);
            transform.SetParent(collision.transform);
        }
        else if (collision.rigidbody != null && !collision.rigidbody.isKinematic)
        {
            transform.SetParent(collision.transform);
        }
        else
        {
            transform.SetParent(null);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            var agent = collision.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (agent != null)
            {
                Vector3 pushDir = collision.contacts[0].normal * -1f;
                float force = 1.0f;

                agent.velocity = pushDir * force * 5f;
            }
        }


        ShatterableGlass glass = collision.gameObject.GetComponent<ShatterableGlass>();
        if (glass != null)
        {
            ShatterableGlassInfo info = new ShatterableGlassInfo(collision.contacts[0].point, transform.forward * 10f);
            glass.Shatter3D(info);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        this.enabled = false;
    }
}
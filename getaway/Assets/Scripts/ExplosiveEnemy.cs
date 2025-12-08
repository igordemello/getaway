using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ExplosiveEnemy : MonoBehaviour
{
    public Transform player;
    public GameObject explosionEffect;
    public float range = 6f;
    public float explosionRadius = 4f;
    public float damage = 40f;

    private NavMeshAgent agent;
    private bool isCharging = false;
    private bool exploded = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    void Update()
    {
        if (exploded || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= range && !isCharging)
        {
            isCharging = true;
            agent.isStopped = false;
        }

        if (isCharging)
        {
            agent.SetDestination(player.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (exploded)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }

    public void Explode()
    {
        exploded = true;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (explosionEffect)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var t = hit.GetComponent<Target>() ?? hit.GetComponentInParent<Target>() ?? hit.GetComponentInChildren<Target>();
                if (t != null) t.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}

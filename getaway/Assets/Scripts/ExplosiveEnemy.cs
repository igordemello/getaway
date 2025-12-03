using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public Transform player;
    public GameObject explosionEffect;

    public float range = 6f;

    public float speed = 14f;

    public float explosionRadius = 4f;
    public float damage = 40f;

    private bool isCharging = false;
    private bool exploded = false;

    void Update()
    {
        if (exploded)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= range && !isCharging)
        {
            isCharging = true;
        }

        if (isCharging)
        {
            Vector3 direction = (player.position - transform.position).normalized; 
            transform.position += direction * speed * Time.deltaTime; //vai perseguir o player
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (exploded)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }

    void Explode()
    {
        exploded = true;

        if (explosionEffect)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);

        }

        Collider[] objects = Physics.OverlapSphere(transform.position, explosionRadius);
        print(objects);

        foreach (Collider hit in objects)
        {
            if (hit.CompareTag("Player"))
            {
                print("é o player");
                Target t = hit.GetComponent<Target>() ?? hit.GetComponentInParent<Target>() ?? hit.GetComponentInChildren<Target>(); //n sei se player é filho ou pai
                if (t != null)
                {
                    t.TakeDamage(damage);
                    print("tomou dano");
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

using UnityEngine;

public class MeleePlayer : MonoBehaviour
{
    [Header("References")]
    public GameObject hitEffect;
    public Camera cam;

    [Header("Melee Attack")]
    public float distance;
    public float delay;
    public float speed;
    public float damage;

    bool attacking = false;
    bool ready = true;
    int count;

    private PlayerControls controls;
    private bool fireInput;

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Fire.performed += ctx => fireInput = true;
        controls.Player.Fire.canceled += ctx => fireInput = false;
    }

    public void Attack()
    {
        if (!ready || attacking) return;

        ready = false;
        attacking = true;

        Invoke(nameof(ResetAttack), speed);
        Invoke(nameof(AttackRaycast), speed);
    }

    private void ResetAttack()
    {
        attacking = false;
        ready = true;
    }

    void AttackRaycast()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distance))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            ExplosiveBarrel barrel = hit.collider.GetComponent<ExplosiveBarrel>();
            if (barrel != null)
            {
                barrel.Explode();
                return;
            }

            ShatterableGlass glass = hit.transform.GetComponent<ShatterableGlass>();

            if (glass != null)
            {
                ShatterableGlassInfo info = new ShatterableGlassInfo(hit.point, cam.transform.forward * 10f);

                glass.Shatter3D(info);

                return;
            }

            GameObject GO = Instantiate(hitEffect, hit.point, Quaternion.identity);
            Destroy(GO, 10);

        }
    }

    void Update()
    {
        if (fireInput)
        {
            Attack();
        }
    }
}

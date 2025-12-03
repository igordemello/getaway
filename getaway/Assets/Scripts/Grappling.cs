using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public LineRenderer lr;
    public LayerMask whatIsGrappleable;
    public Rigidbody rb;

    [Header("Just Cause Grapple")]
    public float maxDistance = 80f;
    public float baseForce = 30f;
    public float maxForce = 180f;
    public float forceGrowthRate = 6f;
    // comentario
    [Tooltip("Ponto final será hit.point + este valor para garantir que o player passe por cima de bordas")]
    public float verticalOffset = 2f;

    private Vector3 grappleTarget;
    private bool grappling;
    private float currentForce;


    private Vector3 grappleStartPosition;
    private Vector3 grappleDirectionFromStart;

    [Header("Cooldown")]
    public float cooldown = 0.3f;
    private float cooldownTimer;

    private PlayerControls controls;
    private bool grapplePressed;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Grappling.performed += ctx => grapplePressed = true;
        controls.Player.Grappling.canceled += ctx => grapplePressed = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
        lr.enabled = false;
    }

    private void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        if (grapplePressed && cooldownTimer <= 0)
            TryStartGrapple();

        if (grappling)
            UpdateLine();
    }

    private void FixedUpdate()
    {
        if (grappling)
            PullPlayer();
    }

    private void TryStartGrapple()
    {
        RaycastHit hit;

        if (!Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, whatIsGrappleable))
            return;
        grappleTarget = hit.point + Vector3.up * verticalOffset;

        grappling = true;
        pm.activeGrapple = true;

        currentForce = baseForce;

        grappleStartPosition = transform.position;
        grappleDirectionFromStart = (grappleTarget - grappleStartPosition);

        lr.enabled = true;
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, hit.point);
    }

    private void PullPlayer()
    {
        Vector3 directionToTarget = (grappleTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, grappleTarget);

        currentForce = Mathf.Lerp(currentForce, maxForce, Time.fixedDeltaTime * forceGrowthRate);

        rb.AddForce(directionToTarget * currentForce, ForceMode.Acceleration);

        Vector3 playerToTarget = transform.position - grappleTarget;
 
        float dot = Vector3.Dot(playerToTarget, grappleDirectionFromStart);

        if (dot > 0f || distance < 3f)
        {
            StopGrapple();
            return;
        }
    }

    private void UpdateLine()
    {
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grappleTarget-verticalOffset*Vector3.up);
    }

    public void StopGrapple()
    {
        if (!grappling) return;

        grappling = false;
        pm.activeGrapple = false;
        lr.enabled = false;
        cooldownTimer = cooldown;

        pm.preserveMomentumTimerActive = true;
        pm.momentumTimer = 0.3f;
    }
}

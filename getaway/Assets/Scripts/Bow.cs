using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; 

public class Bow : MonoBehaviour
{
    [Header("Configurações do Arco")]
    [Tooltip("O prefab da flecha NORMAL.")]
    public GameObject normalArrowPrefab;
    [Tooltip("O prefab da flecha EXPLOSIVA.")]
    public GameObject explosiveArrowPrefab;

    public Transform arrowSpawnPoint;

    [Header("Força e Dano")]
    public float minLaunchForce = 10f;
    public float maxLaunchForce = 30f;

    public float minDamage = 15f;
    public float maxDamage = 50f;

    public float maxDrawTime = 1.0f;
    public float reloadTime = 0.5f;

    [Header("Munição")]
    public int maxAmmo = 20;
    public int maxExplosiveAmmo = 3;
    private int currentAmmo;
    private int currentExplosiveAmmo;
    private bool useExplosive = false;

    [Header("Referências")]
    public Animator animator;
    public TextMeshProUGUI debugAmmo;
    public Collider playerCollider;
    public CamRecoil camRecoil;
    public GunRecoil gunRecoil;
    public Camera playerCamera;

    private PlayerControls controls;
    private bool isDrawing = false;
    private bool isReloading = false;
    private float drawStartTime = 0f;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Fire.performed += ctx => StartDrawing();
        controls.Player.Fire.canceled += ctx => Fire();

        if (animator != null)
            animator.keepAnimatorStateOnDisable = true;
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnEnable()
    {
        controls.Enable();
        isReloading = false;
        isDrawing = false;

        if (animator != null)
        {
            animator.keepAnimatorStateOnDisable = true;
            animator.Play("Idle", 0, 0f);
            animator.Update(0);
        }
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        currentExplosiveAmmo = maxExplosiveAmmo;
    }

    void Update()
    {
        // mouse direito para trocar de flecha
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            useExplosive = !useExplosive;
        }

        if (isDrawing && animator != null)
        {
            float drawDuration = Time.time - drawStartTime;
            float drawPercent = Mathf.Clamp01(drawDuration / maxDrawTime);
            animator.SetFloat("DrawAmount", drawPercent);
        }

        if (debugAmmo != null)
        {
            string type = useExplosive ? "EXPLOSIVA" : "NORMAL";
            int ammoVal = useExplosive ? currentExplosiveAmmo : currentAmmo;
            debugAmmo.text = $"Modo: {type}\nFlechas: {ammoVal}";

            // Muda a cor do texto para indicar perigo uaaau
            debugAmmo.color = useExplosive ? Color.red : Color.white;
        }
    }

    private void StartDrawing()
    {
        if (!IsInIdleState())
            return;

        if (isReloading) return;

        if (useExplosive && currentExplosiveAmmo <= 0) return;
        if (!useExplosive && currentAmmo <= 0) return;

        isDrawing = true;
        drawStartTime = Time.time;

        if (animator != null)
            animator.SetBool("IsDrawing", true);
    }

    private void Fire()
    {
        if (!isDrawing) return;

        isDrawing = false;

        if (animator != null)
        {
            animator.SetBool("IsDrawing", false);
            animator.SetTrigger("Fire");
        }

        camRecoil.Fire();
        gunRecoil.Fire();


        float drawDuration = Time.time - drawStartTime;
        float drawPercent = Mathf.Clamp01(drawDuration / maxDrawTime);

        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, drawPercent);
        float finalDamage = Mathf.Lerp(minDamage, maxDamage, drawPercent);

        GameObject prefabToUse = useExplosive ? explosiveArrowPrefab : normalArrowPrefab;

        GameObject arrowObj = Instantiate(prefabToUse, arrowSpawnPoint.position, arrowSpawnPoint.rotation);

        Arrow arrowScript = arrowObj.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.damage = finalDamage;
        }

        Collider arrowCollider = arrowObj.GetComponent<Collider>();
        if (playerCollider != null && arrowCollider != null)
        {
            Physics.IgnoreCollision(arrowCollider, playerCollider);
        }

        Rigidbody rb = arrowObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 shootDirection = playerCamera.transform.forward;

            arrowObj.transform.rotation = Quaternion.LookRotation(shootDirection);

            rb.linearVelocity = shootDirection * launchForce;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        if (useExplosive) currentExplosiveAmmo--;
        else currentAmmo--;

        StartCoroutine(Reload());
    }

    private bool IsInIdleState()
    {
        if (animator == null) return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        return state.IsName("arco_idle");
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;

        if (animator != null)
            animator.SetTrigger("NockArrow");
    }
}
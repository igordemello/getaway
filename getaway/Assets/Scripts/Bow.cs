using System.Collections;
using TMPro;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("Configurações do Arco")]
    [Tooltip("O prefab da flecha que será disparado.")]
    public GameObject arrowPrefab;

    [Tooltip("O ponto de onde a flecha sai.")]
    public Transform arrowSpawnPoint;

    [Header("Força e Dano")]
    [Tooltip("Força mínima do disparo.")]
    public float minLaunchForce = 10f;
    [Tooltip("Força máxima do disparo.")]
    public float maxLaunchForce = 30f;

    [Tooltip("Dano se apenas clicar (tiro fraco).")]
    public float minDamage = 15f;
    [Tooltip("Dano se segurar até o final (tiro forte).")]
    public float maxDamage = 50f;

    [Tooltip("Tempo (em segundos) para carregar o tiro máximo.")]
    public float maxDrawTime = 1.0f;

    [Tooltip("Tempo para recarregar.")]
    public float reloadTime = 0.5f;

    [Header("Munição")]
    public int maxAmmo = 20;
    private int currentAmmo;

    [Header("Referências")]
    public Animator animator;
    public TextMeshProUGUI debugAmmo;
    public Collider playerCollider;

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
    }

    void Update()
    {
        if (debugAmmo != null)
            debugAmmo.text = $"Flechas:\n{currentAmmo}/{maxAmmo}";
    }

    private void StartDrawing()
    {
        if (isReloading || currentAmmo <= 0) return;

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

        float drawDuration = Time.time - drawStartTime;
        float drawPercent = Mathf.Clamp01(drawDuration / maxDrawTime);

        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, drawPercent);
        float finalDamage = Mathf.Lerp(minDamage, maxDamage, drawPercent);

        GameObject arrowObj = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);

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
            rb.AddForce(arrowSpawnPoint.forward * launchForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("O Prefab da Flecha não tem Rigidbody!");
        }

        currentAmmo--;
        StartCoroutine(Reload());
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
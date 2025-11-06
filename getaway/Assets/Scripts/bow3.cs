using System.Collections;
using TMPro;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("Configurações do Arco")]
    [Tooltip("O prefab da flecha que será disparado.")]
    public GameObject arrowPrefab;

    [Tooltip("O ponto de onde a flecha será disparada.")]
    public Transform arrowSpawnPoint;

    [Tooltip("Força mínima do disparo (se apenas clicar).")]
    public float minLaunchForce = 10f;

    [Tooltip("Força máxima do disparo (após o tempo máximo de puxada).")]
    public float maxLaunchForce = 30f;

    [Tooltip("Tempo (em segundos) para atingir a força máxima.")]
    public float maxDrawTime = 1.0f;

    [Tooltip("Tempo (em segundos) para poder atirar novamente.")]
    public float reloadTime = 0.5f;

    [Header("Munição")]
    public int maxAmmo = 20;
    private int currentAmmo;

    [Header("Referências")]
    public Animator animator;
    public TextMeshProUGUI debugAmmo;
    public Collider playerCollider; // <-- ADICIONE ESTA LINHA

    private PlayerControls controls;
    private bool isDrawing = false;
    private bool isReloading = false;
    private float drawStartTime = 0f;

    private void Awake()
    {
        controls = new PlayerControls();

        // Disparado quando o botão de atirar é PRESSIONADO
        controls.Player.Fire.performed += ctx => StartDrawing();
        // Disparado quando o botão de atirar é SOLTO
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
            animator.Play("Idle", 0, 0f); // Garante que comece no Idle
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

        if (isDrawing)
        {
            // Aqui você pode atualizar um slider de UI ou um som de tensão
        }
    }

    private void StartDrawing()
    {
        if (isReloading || currentAmmo <= 0) return;

        isDrawing = true;
        drawStartTime = Time.time;

        if (animator != null)
            animator.SetBool("IsDrawing", true);

        //Debug.Log("Começou a puxar...");
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

        // Calcula o tempo que o botão foi segurado
        float drawDuration = Time.time - drawStartTime;
        // Limita a força baseada no tempo máximo de puxada
        float drawPercent = Mathf.Clamp01(drawDuration / maxDrawTime);
        // Interpola a força entre o mínimo e o máximo
        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, drawPercent);

        //Debug.Log($"Atirou com {launchForce} de força.");

        // Instancia a flecha
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        
        Collider arrowCollider = arrow.GetComponent<Collider>();
        if (playerCollider != null && arrowCollider != null)
        {
            Physics.IgnoreCollision(arrowCollider, playerCollider);
        }

        // Aplica a força na flecha
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(arrowSpawnPoint.forward * launchForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("O Prefab da Flecha (Arrow) não tem um Rigidbody!");
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
            animator.SetTrigger("NockArrow"); // Trigger para animar colocando uma nova flecha
    }
}
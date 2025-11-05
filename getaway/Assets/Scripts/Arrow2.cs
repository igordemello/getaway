using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Arrow : MonoBehaviour
{
    [Header("Configura��es da Flecha")]
    public float damage = 40f;
    [Tooltip("Tempo em segundos para a flecha ser destru�da se n�o acertar nada.")]
    public float lifeTime = 15.0f;

    private Rigidbody rb;
    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Destr�i a flecha depois de um tempo, caso ela n�o acerte nada
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Se a flecha est� voando (n�o bateu) e tem velocidade,
        // faz ela "olhar" para a dire��o do movimento.
        if (!hasHit && rb.linearVelocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return; // Evita múltiplas colisões
        hasHit = true;

        // --- CORREÇÃO AQUI ---
        // 1. Pare todo o movimento de física PRIMEIRO
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2. AGORA torne-o cinemático para que a física pare de tentar movê-lo
        rb.isKinematic = true;
        // ---------------------

        // "Gruda" a flecha no objeto que acertou
        transform.SetParent(collision.transform);

        // Tenta aplicar dano (usando a mesma classe Target do seu Gun.cs)
        Target target = collision.gameObject.GetComponent<Target>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        // Desativa o collider para não bloquear o jogador
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Desativa este script para parar o Update()
        this.enabled = false;

        // Opcional: Destrói a flecha depois de um tempo
        // Destroy(gameObject, 10f); 
    }
}
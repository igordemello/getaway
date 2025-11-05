using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent Agent;
    public Transform enemy; // pivot/transform do inimigo (pode ser o próprio transform)

    [Header("Layers")]
    public LayerMask whatIsPlayer;
    public LayerMask visionBlockMask; // camadas que bloqueiam visão (ex: Default, Environment)

    [Header("Vision")]
    public float recognitionRange = 15f;
    [Range(10, 180)] public float visionAngle = 90f;
    public float eyeHeight = 1.6f; // origem do raycast (olhos)
    public float playerEyeOffset = 1f; // ponto aproximado no jogador
    public float recognitionCd = 0.15f; // tempo para confirmar visão (pode ser 0 para instantâneo)

    [Header("Search / Timers")]
    public float offAggroCd = 1f;
    public float offSearchingCd = 3f;

    [Header("Movement speeds")]
    public float aggroVelocity = 7f;
    public float patrolVelocity = 4f;
    public float searchingVelocity = 3f;
    public float speedSmooth = 5f;

    private Vector3 LastPlayerPosition = Vector3.zero;
    private Transform seenPlayer = null;
    private bool canSeePlayer = false;
    private float recognitionTimer = 0f;
    private float offAggroTimer = 0f;

    public enum EnemyState { patrol, searching, aggro }
    public EnemyState currState = EnemyState.patrol;

    private bool isSearching = false;

    void Start()
    {
        if (enemy == null) enemy = transform;
        if (Agent == null) Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = true;
        currState = EnemyState.patrol;
    }

    void Update()
    {
        Recognition();
        StateHandler();
    }

    void Recognition()
    {
        // reset
        canSeePlayer = false;
        seenPlayer = null;

        // 1) detecta colliders de jogador no raio
        Collider[] hits = Physics.OverlapSphere(enemy.position, recognitionRange, whatIsPlayer);

        if (hits.Length == 0)
        {
            // fallback por tag (ajuda se layer estiver errada)
            Collider[] all = Physics.OverlapSphere(enemy.position, recognitionRange);
            foreach (var c in all)
            {
                if (c != null && c.CompareTag("Player"))
                {
                    hits = new Collider[] { c };
                    Debug.Log("[Enemy] Fallback: encontrou Player por tag.");
                    break;
                }
            }
        }

        // 2) para cada candidato, checa ângulo e linha de visão
        foreach (var h in hits)
        {
            if (h == null) continue;

            Vector3 dirToTarget = (h.transform.position - enemy.position);
            float distance = dirToTarget.magnitude;
            Vector3 dirNorm = dirToTarget.normalized;

            float angle = Vector3.Angle(enemy.forward, dirNorm);
            Debug.DrawRay(enemy.position + Vector3.up * eyeHeight, dirNorm * Mathf.Min(distance, recognitionRange), Color.cyan);

            if (angle <= visionAngle * 0.5f)
            {
                // Raycast do "olho" até o ponto aproximado do jogador
                Vector3 eyePos = enemy.position + Vector3.up * eyeHeight;
                Vector3 targetPos = h.transform.position + Vector3.up * playerEyeOffset;
                Vector3 rayDir = (targetPos - eyePos).normalized;
                float rayDist = Vector3.Distance(eyePos, targetPos);

                // Raycast com mask de bloqueio — se não atingir nada ou primeiro hit for o jogador, visão ok
                if (Physics.Raycast(eyePos, rayDir, out RaycastHit hit, rayDist + 0.05f, visionBlockMask))
                {
                    // se o primeiro hit for o próprio jogador (ou filho), ok
                    if (hit.collider.transform == h.transform || hit.collider.transform.IsChildOf(h.transform))
                    {
                        canSeePlayer = true;
                        seenPlayer = h.transform;
                        LastPlayerPosition = seenPlayer.position;
                        break;
                    }
                    else
                    {
                        // bloqueado por algo entre inimigo e jogador
                        Debug.Log($"[Enemy] visão bloqueada por {hit.collider.name}");
                    }
                }
                else
                {
                    // não houve hits na camada de bloqueio => visão limpa
                    canSeePlayer = true;
                    seenPlayer = h.transform;
                    LastPlayerPosition = seenPlayer.position;
                    break;
                }
            }
        }

        // 3) confirmação gradual (pequeno delay para evitar flicker)
        if (canSeePlayer && seenPlayer != null)
        {
            recognitionTimer += Time.deltaTime;
            if (recognitionTimer >= recognitionCd)
            {
                recognitionTimer = 0f;
                currState = EnemyState.aggro;
                offAggroTimer = 0f;
                Debug.Log("[Enemy] Viu o player -> AGGRO");
            }
        }
        else
        {
            recognitionTimer = 0f;
        }
    }

    void StateHandler()
    {
        float targetSpeed = patrolVelocity;

        switch (currState)
        {
            case EnemyState.aggro:
                targetSpeed = aggroVelocity;
                if (canSeePlayer && seenPlayer != null)
                {
                    LastPlayerPosition = seenPlayer.position;
                    if (Agent.isOnNavMesh) Agent.SetDestination(LastPlayerPosition);

                    Vector3 lookDir = (seenPlayer.position - enemy.position).normalized;
                    if (lookDir != Vector3.zero)
                        enemy.DORotateQuaternion(Quaternion.LookRotation(lookDir), 0.15f).SetEase(DG.Tweening.Ease.OutSine);
                }
                else
                {
                    offAggroTimer += Time.deltaTime;
                    if (offAggroTimer >= offAggroCd)
                    {
                        offAggroTimer = 0f;
                        currState = EnemyState.searching;
                        if (!isSearching && LastPlayerPosition != Vector3.zero)
                            StartCoroutine(SearchNearby(LastPlayerPosition));
                    }
                }
                break;

            case EnemyState.searching:
                targetSpeed = searchingVelocity;
                if (canSeePlayer && seenPlayer != null)
                {
                    currState = EnemyState.aggro;
                }
                break;

            case EnemyState.patrol:
            default:
                targetSpeed = patrolVelocity;
                break;
        }

        if (Agent.isOnNavMesh)
            Agent.speed = Mathf.Lerp(Agent.speed, targetSpeed, Time.deltaTime * speedSmooth);
    }

    IEnumerator SearchNearby(Vector3 center)
    {
        isSearching = true;
        if (Agent.isOnNavMesh) Agent.SetDestination(center);

        yield return new WaitUntil(() => !Agent.pathPending && Agent.remainingDistance < 0.6f);

        List<Vector3> points = GenerateSearchPoints(center, 8f, 6);
        foreach (var p in points)
        {
            if (currState != EnemyState.searching)
            {
                isSearching = false;
                yield break;
            }

            if (Agent.isOnNavMesh) Agent.SetDestination(p);
            yield return new WaitUntil(() => !Agent.pathPending && Agent.remainingDistance < 0.6f);
            yield return new WaitForSeconds(1f);

            if (canSeePlayer && seenPlayer != null)
            {
                currState = EnemyState.aggro;
                isSearching = false;
                yield break;
            }
        }

        yield return new WaitForSeconds(offSearchingCd);
        if (currState == EnemyState.searching) currState = EnemyState.patrol;
        isSearching = false;
    }

    List<Vector3> GenerateSearchPoints(Vector3 center, float radius, int count)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 point = center + dir * radius;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                points.Add(hit.position);
        }
        return points;
    }

    void OnDrawGizmos()
    {
        if (enemy == null) enemy = transform;
        Gizmos.color = canSeePlayer ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(enemy.position, recognitionRange);

        Vector3 eyePos = enemy.position + Vector3.up * eyeHeight;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * enemy.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle * 0.5f, 0) * enemy.forward;

        Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
        Gizmos.DrawRay(eyePos, leftBoundary * recognitionRange);
        Gizmos.DrawRay(eyePos, rightBoundary * recognitionRange);
    }
}

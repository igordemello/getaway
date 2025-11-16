using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent Agent;
    public Transform enemy;

    [Header("Layers")]
    public LayerMask whatIsPlayer;
    public LayerMask visionBlockMask;

    [Header("Vision Settings")]
    public float recognitionRange = 15f;
    [Range(10, 180)] public float visionAngle = 90f;
    public float eyeHeight = 1.6f;
    public float playerEyeOffset = 1f;
    public float recognitionCd = 0.15f;

    [Header("Hearing Settings")]
    public float hearingRange = 18f; 
    public float hearingSensitivity = 0.3f; 
    public float directAggroThreshold = 0.8f; 

    [Header("Search / Timers")]
    public float offAggroCd = 1f;
    public float offSearchingCd = 3f;

    [Header("Movement Speeds")]
    public float aggroVelocity = 7f;
    public float patrolVelocity = 4f;
    public float searchingVelocity = 3f;
    public float speedSmooth = 5f;

    private Vector3 LastPlayerPosition = Vector3.zero;
    private Transform seenPlayer = null;
    private bool canSeePlayer = false;
    private float recognitionTimer = 0f;
    private float offAggroTimer = 0f;
    private bool isSearching = false;

    public enum EnemyState { patrol, searching, aggro }
    public EnemyState currState = EnemyState.patrol;

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
        Hear();        
        StateHandler();
    }

    void Recognition()
    {
        canSeePlayer = false;
        seenPlayer = null;

        Collider[] hits = Physics.OverlapSphere(enemy.position, recognitionRange, whatIsPlayer);

        if (hits.Length == 0)
        {
            Collider[] all = Physics.OverlapSphere(enemy.position, recognitionRange);
            foreach (var c in all)
            {
                if (c != null && c.CompareTag("Player"))
                {
                    hits = new Collider[] { c };
                    break;
                }
            }
        }

        foreach (var h in hits)
        {
            if (h == null) continue;

            Vector3 dirToTarget = (h.transform.position - enemy.position);
            float distance = dirToTarget.magnitude;
            Vector3 dirNorm = dirToTarget.normalized;
            float angle = Vector3.Angle(enemy.forward, dirNorm);

            if (angle <= visionAngle * 0.5f)
            {
                Vector3 eyePos = enemy.position + Vector3.up * eyeHeight;
                Vector3 targetPos = h.transform.position + Vector3.up * playerEyeOffset;
                Vector3 rayDir = (targetPos - eyePos).normalized;
                float rayDist = Vector3.Distance(eyePos, targetPos);

                if (Physics.Raycast(eyePos, rayDir, out RaycastHit hit, rayDist + 0.05f, visionBlockMask))
                {
                    if (hit.collider.transform == h.transform || hit.collider.transform.IsChildOf(h.transform))
                    {
                        canSeePlayer = true;
                        seenPlayer = h.transform;
                        LastPlayerPosition = seenPlayer.position;
                        break;
                    }
                }
                else
                {
                    canSeePlayer = true;
                    seenPlayer = h.transform;
                    LastPlayerPosition = seenPlayer.position;
                    break;
                }
            }
        }

        if (canSeePlayer && seenPlayer != null)
        {
            recognitionTimer += Time.deltaTime;
            if (recognitionTimer >= recognitionCd)
            {
                recognitionTimer = 0f;
                currState = EnemyState.aggro;
                offAggroTimer = 0f;
            }
        }
        else
        {
            recognitionTimer = 0f;
        }
    }
    void Hear()
    {
        Collider[] sounds = Physics.OverlapSphere(enemy.position, hearingRange);

        foreach (var s in sounds)
        {
            SoundSource source = s.GetComponent<SoundSource>();
            if (source != null && source.isActive)
            {
                float distance = Vector3.Distance(enemy.position, source.transform.position);
                float perceivedVolume = source.volume / Mathf.Max(1f, distance);

                if (perceivedVolume > hearingSensitivity)
                {
                    Debug.DrawLine(enemy.position + Vector3.up, source.transform.position, Color.yellow, 0.25f);
                    Debug.Log($"[Enemy] Ouviu som de {s.name} com intensidade {perceivedVolume:F2}");

                    LastPlayerPosition = source.transform.position;

                    if (perceivedVolume > directAggroThreshold)
                    {
                        currState = EnemyState.aggro;
                        offAggroTimer = 0f;
                        Debug.Log("[Enemy] Som alto -> AGGRO!");
                    }
                    else if (currState == EnemyState.patrol)
                    {
                        currState = EnemyState.searching;
                        if (!isSearching)
                            StartCoroutine(SearchNearby(LastPlayerPosition));
                    }
                    break;
                }
            }
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
                        enemy.DORotateQuaternion(Quaternion.LookRotation(lookDir), 0.15f).SetEase(Ease.OutSine);
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

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(enemy.position, hearingRange);
    }

    void OnDestroy()
    {
        transform.DOKill();
        enemy.DOKill();
    }
}

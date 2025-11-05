using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent Agent;
    public Transform enemy;
    public LayerMask whatIsPlayer;

    private Vector3 LastPlayerPosition;

    [Header("Vision Settings")]
    public float recognitionRange = 15f;
    [Range(10, 180)] public float visionAngle = 90f;

    [Header("Cooldowns")]
    public float recognitionCd = 1f;
    public float offAggroCd = 1f;
    public float offSearchingCd = 3f;
    private float recognitionTimer = 0f;
    private float offAggroTimer = 0f;

    [Header("Search Settings")]
    public float searchRadius = 10f;
    public int searchPoints = 6;
    public float waitAtPoint = 1f;
    private bool isSearching = false;

    [Header("Velocities")]
    public float aggroVelocity = 7f;
    public float patrolVelocity = 5f;
    public float searchingVelocity = 3f;
    public float speedSmooth = 3f;

    public enum EnemyState { patrol, searching, aggro }
    public EnemyState currState;

    private bool canSeePlayer;
    private Transform seenPlayer;

    void Start()
    {
        currState = EnemyState.patrol;
        Agent.updateRotation = false;
    }

    void FixedUpdate()
    {
        Recognition();
        StateHandler();
    }
    void Recognition()
    {
        seenPlayer = null;
        canSeePlayer = false;

        Collider[] hits = Physics.OverlapSphere(enemy.position, recognitionRange, whatIsPlayer);
        foreach (var h in hits)
        {
            Vector3 dirToTarget = (h.transform.position - enemy.position).normalized;
            float angle = Vector3.Angle(enemy.forward, dirToTarget);

            if (angle < visionAngle * 0.5f)
            {
                if (!Physics.Linecast(enemy.position, h.transform.position, out RaycastHit obstacle))
                {
                    seenPlayer = h.transform;
                    canSeePlayer = true;
                    break;
                }
                else if (obstacle.collider.transform == h.transform)
                {
                    seenPlayer = h.transform;
                    canSeePlayer = true;
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
                    offAggroTimer = 0f;
                    recognitionTimer = 0f;
                    LastPlayerPosition = seenPlayer.position;
                    Agent.SetDestination(LastPlayerPosition);

                    Vector3 lookDir = (seenPlayer.position - enemy.position).normalized;
                    if (lookDir != Vector3.zero)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(lookDir);
                        enemy.DORotateQuaternion(lookRot, 0.2f).SetEase(Ease.OutSine);
                    }
                }
                else
                {
                    offAggroTimer += Time.deltaTime;
                    if (offAggroTimer >= offAggroCd)
                    {
                        offAggroTimer = 0f;
                        currState = EnemyState.searching;
                    }
                }
                break;
            case EnemyState.searching:
                targetSpeed = searchingVelocity;

                if (canSeePlayer && seenPlayer != null)
                {
                    LastPlayerPosition = seenPlayer.position;
                    Agent.SetDestination(LastPlayerPosition);

                    Vector3 lookSearch = (seenPlayer.position - enemy.position).normalized;
                    if (lookSearch != Vector3.zero)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(lookSearch);
                        enemy.DORotateQuaternion(lookRot, 0.3f).SetEase(Ease.OutSine);
                    }

                    recognitionTimer += Time.deltaTime;
                    if (recognitionTimer >= recognitionCd)
                    {
                        recognitionTimer = 0f;
                        currState = EnemyState.aggro;
                    }
                }
                else
                {
                    if (!isSearching && LastPlayerPosition != Vector3.zero)
                        StartCoroutine(SearchNearby(LastPlayerPosition));
                }
                break;

            case EnemyState.patrol:
                targetSpeed = patrolVelocity;

                if (canSeePlayer && seenPlayer != null)
                {
                    LastPlayerPosition = seenPlayer.position;
                    Agent.SetDestination(LastPlayerPosition);

                    Vector3 lookPatrol = (seenPlayer.position - enemy.position).normalized;
                    if (lookPatrol != Vector3.zero)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(lookPatrol);
                        enemy.DORotateQuaternion(lookRot, 0.3f).SetEase(Ease.OutSine);
                    }

                    currState = EnemyState.aggro;
                }
                else
                {
                    // Aqui você pode adicionar movimento entre waypoints
                }
                break;
        }

        Agent.speed = Mathf.Lerp(Agent.speed, targetSpeed, Time.deltaTime * speedSmooth);
    }

    IEnumerator SearchNearby(Vector3 center)
    {
        isSearching = true;
        Agent.SetDestination(center);
        yield return new WaitUntil(() => !Agent.pathPending && Agent.remainingDistance < 0.5f);

        List<Vector3> points = GenerateSearchPoints(center, searchRadius, searchPoints);

        foreach (var p in points)
        {
            if (currState != EnemyState.searching)
            {
                isSearching = false;
                yield break;
            }

            Agent.SetDestination(p);
            Vector3 lookDir = (p - enemy.position).normalized;
            enemy.DORotateQuaternion(Quaternion.LookRotation(lookDir), 0.6f).SetEase(Ease.OutSine);

            yield return new WaitUntil(() => !Agent.pathPending && Agent.remainingDistance < 0.5f);
            yield return new WaitForSeconds(waitAtPoint);

            enemy.DORotateQuaternion(Quaternion.LookRotation(Quaternion.Euler(0, Random.Range(-20f, 20f), 0) * lookDir), 0.4f)
                 .SetLoops(2, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(0.6f);

            if (canSeePlayer && seenPlayer != null)
            {
                LastPlayerPosition = seenPlayer.position;
                currState = EnemyState.aggro;
                isSearching = false;
                yield break;
            }
        }

        yield return new WaitForSeconds(offSearchingCd);

        if (currState == EnemyState.searching)
            currState = EnemyState.patrol;

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
}

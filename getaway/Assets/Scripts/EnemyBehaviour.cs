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

    [Header("Range variables")]
    public float recognitionRange = 10f;
    public float soundRange = 10f;

    [Header("Cooldown variables")]
    public float recognitionCd = 3f;
    private float recognitionTimer = 0f;
    public float OffAggroCd = 3f;
    private float OffAggroTimer = 0f;
    public float OffPatrolCd = 3f;
    private float OffPatrolTimer = 0f;

    [Header("Search Settings")]
    public float searchRadius = 10f;
    public int searchPoints = 6;
    public float waitAtPoint = 1f;
    private bool isSearching = false;

    public enum EnemyState
    {
        searching,
        patrol,
        aggro
    }

    public EnemyState currState;

    void Start()
    {
        LastPlayerPosition = Vector3.zero;
        currState = EnemyState.searching;
    }

    void FixedUpdate()
    {
        Recognition();
        StateHandler();
    }

    void StateHandler()
    {
        switch (currState)
        {
            case EnemyState.aggro:
                Agent.speed = 7f;
                break;
            case EnemyState.patrol:
                Agent.speed = 5f;
                break;
            case EnemyState.searching:
                Agent.speed = 3f;
                break;
        }
    }

    void Recognition()
    {
        RaycastHit hit;

        if (currState == EnemyState.aggro)
        {
            if (Physics.Raycast(enemy.position, enemy.forward, out hit, recognitionRange, whatIsPlayer))
            {
                OffAggroTimer = 0f;
                recognitionTimer = 0f;
                OffPatrolTimer = 0f;
                LastPlayerPosition = hit.collider.transform.position;
                Rigidbody rb = hit.collider.attachedRigidbody;
                Agent.SetDestination(LastPlayerPosition);
                float distance = Vector3.Distance(enemy.position, LastPlayerPosition);
                enemy.DORotateQuaternion(Quaternion.LookRotation(enemy.forward + rb.linearVelocity / distance), 0.5f);
            }
            else
            {
                OffAggroTimer += Time.deltaTime;
                if (OffAggroTimer >= OffAggroCd)
                {
                    OffAggroTimer = 0f;
                    currState = EnemyState.patrol;
                }
            }
        }

        else if (currState == EnemyState.patrol)
        {
            OffAggroTimer = 0f;
            recognitionTimer = 0f;

            if (Physics.Raycast(enemy.position, enemy.forward, out hit, recognitionRange, whatIsPlayer))
            {
                LastPlayerPosition = hit.collider.transform.position;
                Rigidbody rb = hit.collider.attachedRigidbody;
                Agent.SetDestination(LastPlayerPosition);
                float distance = Vector3.Distance(enemy.position, LastPlayerPosition);
                enemy.DORotateQuaternion(Quaternion.LookRotation(enemy.forward + rb.linearVelocity / distance), 0.5f);
                currState = EnemyState.aggro;
            }
            else
            {
                OffPatrolTimer += Time.deltaTime;
                if (OffPatrolTimer >= OffPatrolCd)
                {
                    OffPatrolTimer = 0f;
                    currState = EnemyState.searching;
                }
            }
        }

        else if (currState == EnemyState.searching)
        {
            OffAggroTimer = 0f;
            OffPatrolTimer = 0f;

            if (Physics.Raycast(enemy.position, enemy.forward, out hit, recognitionRange, whatIsPlayer))
            {
                LastPlayerPosition = hit.collider.transform.position;
                Rigidbody rb = hit.collider.attachedRigidbody;
                Agent.SetDestination(LastPlayerPosition);
                float distance = Vector3.Distance(enemy.position, LastPlayerPosition);
                enemy.DORotateQuaternion(Quaternion.LookRotation(enemy.forward + rb.linearVelocity / distance), 0.5f);
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
                {
                    StartCoroutine(SearchNearby(LastPlayerPosition));
                }
            }
        }
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
            yield return new WaitUntil(() => !Agent.pathPending && Agent.remainingDistance < 0.5f);
            yield return new WaitForSeconds(waitAtPoint);

            RaycastHit hit;
            if (Physics.Raycast(enemy.position, enemy.forward, out hit, recognitionRange, whatIsPlayer))
            {
                LastPlayerPosition = hit.collider.transform.position;
                currState = EnemyState.aggro;
                isSearching = false;
                yield break;
            }
        }

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
            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, 2f, NavMesh.AllAreas))
                points.Add(hit.position);
        }

        return points;
    }
}

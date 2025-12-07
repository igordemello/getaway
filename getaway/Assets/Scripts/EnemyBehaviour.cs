using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using UnityEngine.Jobs;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent Agent;
    public Transform enemy;
    public Transform player;

    [Header("Patrol Path")]
    public Path path;
    public float waypointArriveDistance = 0.4f;
    public float MaxTimeAtWaypoint = 1f;
    private float timerAtWaypoint = 0f;
    private Vector3 currentPatrolTarget;
    private bool hasPath => path != null && path.waypoints != null && path.waypoints.Length > 0;
    private bool patrolInitialized = false;
    private bool shouldReturnToPatrol = false;

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
    public float hearingRange; 
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

    [Header("Combat Distance")]
    public float stopDistance = 8f;

    private Vector3 LastPlayerPosition = Vector3.zero;
    private Transform seenPlayer = null;
    private bool canSeePlayer = false;
    private float recognitionTimer = 0f;
    private float offAggroTimer = 0f;
    private bool isSearching = false;

    public enum EnemyState { patrol, searching, aggro }
    public EnemyState currState = EnemyState.patrol;

    [Header("Shooting Settings")]
    public Transform firePoint;
    public float shootDamage = 20f;
    public float shootRange = 40f;
    public float shootRate = 0.7f;
    public LayerMask shootMask;
    public GunRecoil gunRecoil;
    public ParticleSystem muzzle;
    public GameObject impact;
    public GameObject blood_impact;

    private float shootTimer = 0f;

    [Header("Weapon Aim Settings")]
    public Transform weaponPivot;
    public float aimSpeed = 10f;
    public float aimError = 2f;
    public float aimErrorChangeSpeed = 2f;
    private Vector3 currentErrorOffset;

    void Start()
    {
        if (enemy == null) enemy = transform;
        if (Agent == null) Agent = GetComponent<NavMeshAgent>();

        if (hasPath)
        {
            currentPatrolTarget = path.GetCurrentWayPoint();
            Agent.SetDestination(currentPatrolTarget);
            patrolInitialized = true;
        }

        Agent.updateRotation = true;
        currState = EnemyState.patrol;
    }

    void FixedUpdate()
    {
        Recognition(); 
        Hear();        
        StateHandler();
        //print(Vector3.Distance(player.position, enemy.position));
    }

    void HandlePatrol()
    {
        if (!hasPath) return;

        if (!patrolInitialized)
        {
            currentPatrolTarget = path.GetCurrentWayPoint();
            Agent.SetDestination(currentPatrolTarget);
            patrolInitialized = true;
        }

        if (!Agent.pathPending && Agent.remainingDistance <= waypointArriveDistance)
        {
            timerAtWaypoint += Time.deltaTime;
            if (timerAtWaypoint >= MaxTimeAtWaypoint)
            {
                timerAtWaypoint = 0f;
                currentPatrolTarget = path.GetNextWayPoint();
                Agent.SetDestination(currentPatrolTarget);

            } 
        }
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
                //print("fonte de som");
                float distance = Vector3.Distance(enemy.position, source.transform.position);
                //print(distance);
                float perceivedVolume = source.volume / (Mathf.Max(1f, distance)* Mathf.Max(1f, distance));
                //print(perceivedVolume);
                if (perceivedVolume > hearingSensitivity)
                {
                   // print("som percebido");
                   // Debug.DrawLine(enemy.position + Vector3.up, source.transform.position, Color.yellow, 0.25f);
                    //Debug.Log($"[Enemy] Ouviu som de {s.name} com intensidade {perceivedVolume:F2}");

                    LastPlayerPosition = source.transform.position;
                   // Agent.ResetPath();
                    Agent.SetDestination(LastPlayerPosition);
                    if (perceivedVolume > directAggroThreshold)
                    {
                        currState = EnemyState.aggro;
                        offAggroTimer = 0f;
                       // Debug.Log("[Enemy] Som alto -> AGGRO!");
                        
                        
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

                    float distToPlayer = Vector3.Distance(enemy.position, LastPlayerPosition);

                    if (distToPlayer > stopDistance)
                    {
                        if (Agent.isOnNavMesh)
                            Agent.SetDestination(LastPlayerPosition);
                    }
                    else
                    {
                        if (Agent.isOnNavMesh)
                            Agent.ResetPath();
                    }

                    Vector3 lookDir = (seenPlayer.position - enemy.position).normalized;
                    if (lookDir != Vector3.zero)
                        enemy.DORotateQuaternion(Quaternion.LookRotation(lookDir), 0.15f).SetEase(Ease.OutSine);

                    AimWeaponAtPlayer(seenPlayer);

                    ShootAtPlayer();
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
                HandlePatrol();
                break;
        }

        if (shouldReturnToPatrol && currState != EnemyState.aggro)
        {
            currState = EnemyState.patrol;
            patrolInitialized = false;
            shouldReturnToPatrol = false;
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
        if (currState == EnemyState.searching)
        {
            shouldReturnToPatrol = true;
        }
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

    void ShootAtPlayer()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;

        if (firePoint == null) return;

        shootTimer = shootRate;
        gunRecoil.Fire();
        muzzle.Play();

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, shootRange, shootMask))
        {
            //Debug.Log(hit.transform.name);

            var target = hit.transform.GetComponent<Target>();
            Debug.Log(target);
            if (target != null)
            {
                target.TakeDamage(shootDamage);
            }

            Debug.DrawLine(firePoint.position, hit.point, Color.red, 0.2f);

            if (hit.transform.CompareTag("Player"))
            {
                GameObject impactGO_blood = Instantiate(blood_impact, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO_blood, 1f);
            }
            else
            {
                GameObject impactGO = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 1f);
            }
            
        }
        else
        {
            Debug.DrawLine(firePoint.position, firePoint.position + firePoint.forward * shootRange, Color.yellow, 0.2f);
        }
    }

    void AimWeaponAtPlayer(Transform target)
    {
        if (weaponPivot == null || target == null) return;

        Vector3 randomOffset = new Vector3(
            Random.Range(-aimError, aimError),
            Random.Range(-aimError, aimError),
            Random.Range(-aimError, aimError)
        );

        currentErrorOffset = Vector3.Lerp(currentErrorOffset, randomOffset, Time.deltaTime * aimErrorChangeSpeed);

        Vector3 dir = (target.position - weaponPivot.position).normalized;
        dir += currentErrorOffset * 0.01f;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        weaponPivot.rotation = Quaternion.Lerp(
            weaponPivot.rotation,
            targetRot,
            Time.deltaTime * aimSpeed
        );
    }

    void OnDestroy()
    {
        transform.DOKill();
        enemy.DOKill();
    }
}

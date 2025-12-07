using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using UnityEngine.Jobs;

public class TurretBehaviour : MonoBehaviour
{
    [Header("Components")]
    public Transform enemy;
    public Transform player;
    public Transform turretHead; 

    [Header("Layers")]
    public LayerMask whatIsPlayer;
    public LayerMask visionBlockMask;

    [Header("Vision Settings")]
    public float recognitionRange = 15f;
    [Range(10, 180)] public float visionAngle = 90f;
    public float eyeHeight =1.6f;
    public float playerEyeOffset = 1f;

    [Header("Has light variable")]
    public bool hasLight = true;

    private Vector3 LastPlayerPosition = Vector3.zero;
    private Transform seenPlayer = null;
    private bool canSeePlayer = false;

    public enum EnemyState { searching, aggro }
    public EnemyState currState = EnemyState.searching;

    [Header("Shooting Settings")]
    public Transform firePoint1;
    public Transform firePoint2;
    public float shootDamage = 20f;
    public float shootRange = 40f;
    public float shootRate = 0.7f;
    public LayerMask shootMask;
    public GunRecoil gunRecoil;
    public ParticleSystem muzzle1;
    public ParticleSystem muzzle2;
    public GameObject impact;

    private Transform actualFirePoint;
    private ParticleSystem actualMuzzle;
    private float shootTimer = 0f;

    [Header("Weapon Aim Settings")]
    public float aimSpeed = 10f;
    public float aimError = 2f;
    public float aimErrorChangeSpeed = 2f;
    private Vector3 currentErrorOffset;

    void Start()
    {
        currState = EnemyState.searching;
        hasLight = true;
        actualFirePoint = firePoint1; 
    }

    void FixedUpdate()
    {
        Recognition();
        StateHandler();
    }

  
    void ChangeFirePoint()
    {
        actualFirePoint = (actualFirePoint == firePoint1) ? firePoint2 : firePoint1;
        actualMuzzle = (actualFirePoint == firePoint1) ? muzzle2 : muzzle1;
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
            currState = EnemyState.aggro;
            return;
        }

        currState = EnemyState.searching;
    }

    void StateHandler()
    {
        if (!hasLight) return;

        switch (currState)
        {
            case EnemyState.aggro:
                AimWeaponAtPlayer(seenPlayer);
                ShootAtPlayer();
                break;

            case EnemyState.searching:
                AimWeaponAtPlayer(null);
                break;
        }
    }

    void ShootAtPlayer()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;

        shootTimer = shootRate;

        
        ChangeFirePoint();

        if (actualFirePoint == null) return;

        gunRecoil.Fire();
        actualMuzzle.Play();

        RaycastHit hit;
        if (Physics.Raycast(actualFirePoint.position, player.position - actualFirePoint.position, out hit, shootRange, shootMask))
        {
            var target = hit.transform.GetComponent<Target>();
            if (target != null) target.TakeDamage(shootDamage);

            Debug.DrawLine(actualFirePoint.position, hit.point, Color.red, 0.2f);

            GameObject impactGO = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 1f);
        }
        else
        {
            Debug.DrawLine(actualFirePoint.position, actualFirePoint.position + actualFirePoint.forward * shootRange, Color.yellow, 0.2f);
        }
    }

  
    void AimWeaponAtPlayer(Transform target)
    {
        if (turretHead == null || target == null) return;

        Vector3 randomOffset = new Vector3(
            Random.Range(-aimError, aimError),
            Random.Range(-aimError, aimError),
            Random.Range(-aimError, aimError)
        );

        currentErrorOffset = Vector3.Lerp(currentErrorOffset, randomOffset, Time.deltaTime * aimErrorChangeSpeed);

        Vector3 dir = (target.position+100*Vector3.up- actualFirePoint.position).normalized;
        dir += currentErrorOffset * 0.01f;

        

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        turretHead.rotation = Quaternion.Lerp(
            turretHead.rotation,
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

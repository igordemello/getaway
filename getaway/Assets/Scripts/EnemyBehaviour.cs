using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{

    [Header("Components")]
    public NavMeshAgent Agent;
    private Vector3 LastPlayerPosition;
    public Transform enemy;
    public LayerMask whatIsPlayer;


    [Header("Range variables")]
    public float recognitionRange;
    public float soundRange;


    [Header("Cooldown variables")]
    public float recognitionCd = 3f;
    private float recognitionTimer = 0f ;
    public float OffAggroCd = 3f;
    private float OffAggroTimer = 0f;
    public float OffPatrolCd = 3f;
    private float OffPatrolTimer = 0f;

    public enum EnemyState {
        searching,
        patrol,
        aggro
    }

    public EnemyState currState;

    void Start()
    {
        LastPlayerPosition = Vector3.zero;
        currState = EnemyState.patrol;
    }

    void Update()
    {
        Recognition();
        StateHandler();
    }
    void StateHandler() {
        if (currState == EnemyState.aggro) {
            Agent.speed = 7f;
           
        }
        else if (currState == EnemyState.patrol) {
            Agent.speed = 5f;
            
        }
        else if (currState == EnemyState.searching) {
            Agent.speed = 3f;
            
        }



    }
    void Recognition() {
        RaycastHit hit;
        if (currState == EnemyState.aggro) {
            print("aggro");
            if (Physics.Raycast(enemy.position, enemy.forward, out hit, recognitionRange, whatIsPlayer))
            {
                OffAggroTimer = 0f;
                print("Player spotted");
                LastPlayerPosition = hit.collider.GetComponent<Transform>().position;
                Rigidbody rb = hit.collider.attachedRigidbody;
                Agent.SetDestination(LastPlayerPosition);
                float distance = Vector3.Distance(enemy.position, LastPlayerPosition);
                enemy.DORotateQuaternion(Quaternion.LookRotation(enemy.forward + rb.linearVelocity/distance), 0.5f);

            }
            else {
                OffAggroTimer += Time.deltaTime;
                if (OffAggroTimer >= OffAggroCd) {
                    OffAggroTimer = 0f;
                    currState = EnemyState.patrol;
                }
            }

        }

        else if (currState == EnemyState.patrol) {
            print("patrol");
            if (Physics.Raycast(enemy.position, enemy.forward, out hit, recognitionRange, whatIsPlayer))
            {
                OffPatrolTimer = 0f;
                print("Player spotted");
                LastPlayerPosition = hit.collider.GetComponent<Transform>().position;
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

        else if (currState == EnemyState.searching) {
            print("searching");
            return;

        }
        

    }
}

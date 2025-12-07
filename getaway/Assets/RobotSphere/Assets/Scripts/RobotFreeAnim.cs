using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class RobotFreeAnim : MonoBehaviour
{
    Animator anim;
    NavMeshAgent agent;
    float moveThreshold = 0.1f;

    void Awake()
    {
        anim = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();
    }

    void Update()
    {
        float velocidade = 0f;

        if (agent != null)
        {
            velocidade = agent.velocity.magnitude;
        }
        else
        {
            velocidade = (transform.position - lastPos).magnitude / Time.deltaTime;
            lastPos = transform.position;
        }

        anim.SetBool("Walk_Anim", velocidade > moveThreshold);
    }

    Vector3 lastPos;
}




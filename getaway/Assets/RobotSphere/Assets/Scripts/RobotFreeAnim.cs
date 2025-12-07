using UnityEngine;

public class RobotFreeAnim : MonoBehaviour
{
    Animator anim;

    Vector3 lastPosition;
    float moveThreshold = 0.01f; // sensibilidade para detectar movimento

    void Awake()
    {
        anim = GetComponent<Animator>();
        lastPosition = transform.position;
    }

    void Update()
    {
        CheckMovementAnim();
        //CheckExtraAnims();
    }

    void CheckMovementAnim()
    {
        // Calcula a velocidade real do robô
        float velocidade = (transform.position - lastPosition).magnitude / Time.deltaTime;

        // Se estiver se movendo → anda
        if (velocidade > moveThreshold)
            anim.SetBool("Walk_Anim", true);
        else
            anim.SetBool("Walk_Anim", false);

        lastPosition = transform.position;
    }

//    void CheckExtraAnims()
//    {
//        // Roll
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            anim.SetBool("Roll_Anim", !anim.GetBool("Roll_Anim"));
//        }

//        // Open / Close
//        if (Input.GetKeyDown(KeyCode.LeftControl))
//        {
//            anim.SetBool("Open_Anim", !anim.GetBool("Open_Anim"));
//        }
//    }
    }


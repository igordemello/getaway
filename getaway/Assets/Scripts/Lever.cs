using UnityEngine;

public class Lever : MonoBehaviour
{
    private enum State { up,down};
    private State current_state = State.up;
    private  Animator anim ;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    public void Interact()
    {
        if (current_state == State.up)
        {
            if (anim)
            {
                anim.SetTrigger("Lever_Down");
            }
            current_state = State.down;
        }
        else if (current_state == State.down)
        {
            if (anim)
            {
                anim.SetTrigger("Lever_Up");
            }
            current_state = State.up;
        }

        if (LightManager.Instance)
            LightManager.Instance.ToggleAll();
    }

}

using UnityEngine;

public class Lever : MonoBehaviour
{
    private enum State { up,down};
    private State current_state = State.up;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Interact()
    {
        Animator anim = GetComponent<Animator>();
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

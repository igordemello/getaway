using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour
{
    [Header("References")]
    
    public Animator animator;

    private enum DoorState {Closed, Open}
    private DoorState currentState = DoorState.Closed;
    private bool isAnimating = false;
    
    public void Interact()
    {
        if (isAnimating) return;

        if (currentState == DoorState.Closed) OpenDoor();
        else CloseDoor();
    }

    private void OpenDoor()
    {
        isAnimating = true;
        animator.Play("open", 0, 0f);
        currentState = DoorState.Open;

    }

    private void CloseDoor()
    {
        isAnimating = true;
        animator.Play("close", 0, 0f);
        currentState = DoorState.Closed;

    }

    public void OnDoorAnimationEnd()
    {
        print("isAnimating eh false");
        isAnimating = false;
    }
}

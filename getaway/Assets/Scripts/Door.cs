using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("References")]
    
    public Animator animator;
    public PlayerInventory inv;
    public string requiredItem;
    public TextMeshProUGUI notify;

    private enum DoorState {Closed, Open}
    private DoorState currentState = DoorState.Closed;
    private bool isAnimating = false;
    
    public void Interact()
    {
        if (isAnimating) return;

        if (currentState == DoorState.Closed) OpenDoor();
        else CloseDoor();
    }

    public void setEnergy(bool hasEnrgy)
    {
        if (hasEnrgy && currentState == DoorState.Closed)
        {
            return;
        }
        if (hasEnrgy && currentState == DoorState.Open)
        {
            CloseDoor();
            currentState = DoorState.Closed;
            return;
        }
        if (!hasEnrgy && currentState == DoorState.Closed)
        {
            OpenDoor();
            currentState = DoorState.Open;
            return;
        }
        if (!hasEnrgy && currentState == DoorState.Open)
        {
            return;
        }
        
    }

    private void OpenDoor()
    {
        if(inv && !string.IsNullOrEmpty(requiredItem))
        {
            bool hasItem = false;
            foreach (string item in inv.items)
            {
                if (item == requiredItem)
                {
                    hasItem = true;
                    break;
                }
            }
            if (!hasItem)
            {
                ShowNotify("Acesso negado", 2f);
                return;
            }
        }
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

    private Coroutine notifyRoutine;

    private void ShowNotify(string message, float duration)
    {
        if (notifyRoutine != null)
            StopCoroutine(notifyRoutine);

        notifyRoutine = StartCoroutine(NotifyRoutine(message, duration));
    }

    private IEnumerator NotifyRoutine(string message, float duration)
    {
        notify.text = message;
        yield return new WaitForSeconds(duration);
        notify.text = "";
    }
}

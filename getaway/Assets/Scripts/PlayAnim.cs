using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayAnim : MonoBehaviour
{
    [SerializeField] private Animator myDoor = null;

    [SerializeField] private string doorOpen;
    [SerializeField] private string doorClose;

    private enum DoorState
    {
        Closed,
        Open
    }

    private DoorState currentState = DoorState.Closed;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (currentState == DoorState.Closed)
        {
            OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (currentState == DoorState.Open)
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        myDoor.Play(doorOpen, 0, 0f);
        currentState = DoorState.Open;

    }

    private void CloseDoor()
    {
        myDoor.Play(doorClose, 0, 0f);
        currentState = DoorState.Closed;

    }
}
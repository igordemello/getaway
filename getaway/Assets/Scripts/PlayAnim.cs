using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnim : MonoBehaviour
{
    [SerializeField] private Animator myDoor = null;

    [SerializeField] private string doorOpen = "DoorOpen";
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter chamado! Objeto que entrou: " + other.name + " | Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Porta aberta! Tag do objeto: " + other.tag);
            myDoor.Play(doorOpen, 0, 0.0f);
        }
    }
}
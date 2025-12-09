using System.Linq;
using UnityEngine;

public class coletavel : MonoBehaviour
{
    public string itemName;
    public PlayerInventory playerInventory;

    public void Interact()
    {
        playerInventory.items.Add(itemName);
        Destroy(gameObject);
    }
}

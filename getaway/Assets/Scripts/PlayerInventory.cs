using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<string> items = new List<string>();

    public Image passcardDisplay;

    void Update()
    {
        passcardDisplay.enabled = items.Contains("passcard1");
    }
}



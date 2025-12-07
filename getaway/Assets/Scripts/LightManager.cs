using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance;

    public List<LightController> lamps = new List<LightController>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterLamp(LightController lamp)
    {
        if (!lamps.Contains(lamp))
            lamps.Add(lamp);
    }

    public void ToggleAll()
    {
        foreach (var lamp in lamps)
            if (lamp)
                lamp.toggleEnergy();
    }
}
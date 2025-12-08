using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance;

    public List<LightController> lamps = new List<LightController>();
    public List<TurretBehaviour> turrets = new List<TurretBehaviour>();
    public List<Door> doors = new List<Door>();

    private bool hasEnergy = true;

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
    public void RegisterTurret(TurretBehaviour turret)
    {
        if (!turrets.Contains(turret))
            turrets.Add(turret);
    }
    public void RegisterDoor(Door door) {
        if (!doors.Contains(door)) 
            doors.Add(door);

    }

    public void ToggleAll()
    {
        hasEnergy = !hasEnergy;

        foreach (var lamp in lamps)
            if (lamp)
                lamp.toggleEnergy();
        foreach (var turret in turrets)
            if (turret)
                turret.setEnergy(hasEnergy);
        foreach (var door in doors)
            if (door)
                door.setEnergy(hasEnergy);
    }
}
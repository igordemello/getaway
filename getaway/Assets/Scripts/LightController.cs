using UnityEngine;

public class LightController : MonoBehaviour
{

    public Light lampLight; 
    //public Renderer lampRenderer; //modelo da lampada se precisar
    //dps se precisar crio variável para os materiais caso use um modelo
    public bool isOn = true;

    void Start()
    {
        UpdateLamp();
    }

    public void toggleEnergy()
    {
        isOn = !isOn;
        UpdateLamp();
    }

    //se precisar colocar func de on e off
    void UpdateLamp()
    {
        if (lampLight != null)
            lampLight.enabled = isOn;
    }
}

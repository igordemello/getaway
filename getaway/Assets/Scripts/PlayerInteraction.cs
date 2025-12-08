using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Camera playerCam;
    public Image aim;

    [Header("Settings")]
    public float maxDistanceToInteract = 10f;
    private bool interactInput;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Interagir.performed += ctx => interactInput = true;
        controls.Player.Interagir.canceled += ctx => interactInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, maxDistanceToInteract))
        {
            if (hit.transform.CompareTag("Door") || hit.transform.CompareTag("CanPickUp"))
            {
                SetAimColor(Color.red);
                if (interactInput)
                {
                    Door door = hit.transform.GetComponent<Door>();
                    if (door != null) door.Interact();
                }
            }
            else
            {
                SetAimColor(Color.black);
            }
        }
        else
            {
                SetAimColor(Color.black);
            }
    }

    void SetAimColor(Color color)
    {
    if (aim.color != color)
        aim.color = color;
    }
}

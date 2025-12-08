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
    public float interactCooldown = 0.5f;
    public bool canInteract = true;
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
            if (canInteract && (hit.transform.CompareTag("Door") || hit.transform.CompareTag("CanPickUp") || hit.transform.CompareTag("lever")))
            {
                SetAimColor(Color.red);
                if (interactInput)
                {
                    Door door = hit.transform.GetComponent<Door>();
                    if (door != null) door.Interact();
                    Lever lever = hit.transform.GetComponent<Lever>();
                    if (lever != null) lever.Interact();
                    if (door!= null || lever != null)
                    {
                        StartCoroutine(InteractCooldown());
                    }
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

    private System.Collections.IEnumerator InteractCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactCooldown);
        canInteract = true;
    }

    void SetAimColor(Color color)
    {
    if (aim.color != color)
        aim.color = color;
    }
}


using System;
using UnityEditor.UI;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;

    public float throwForce = 500f; 
    public float pickUpRange = 5f;
    private float rotationSensitivity = 1f; 
    private GameObject heldObj; 
    private Rigidbody heldObjRb; 
    private bool canDrop = true;
    private int LayerNumber; 


    private PlayerControls controls;
    private bool pickInput;


    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Pick.performed += ctx => OnPickPressed();
 
    }
    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnPickPressed()
    {

        pickInput = true;
    }

    void Start()
    {

    }
    void Update()
    {
        if (pickInput) 
        {
            pickInput = false;
            if (heldObj == null)
            {
            
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
                {
           
                    if (hit.transform.gameObject.tag == "CanPickUp")
                    {
                    
                        PickUpObject(hit.transform.gameObject);
                    }
                }
            }
            else
            {
                if (canDrop == true)
                {
                    StopClipping();
                    DropObject();
                }
            }
        }
        if (heldObj != null) 
        {
            MoveObject(); 
            RotateObject();
            if ("AAAAAA" == "true") 
            {
                StopClipping();
                ThrowObject();
            }

        }
    }
    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj; 
            heldObjRb = pickUpObj.GetComponent<Rigidbody>(); 
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform; 
            heldObj.layer = LayerNumber; 
          
            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }
    void DropObject()
    {
        if (heldObj == null)
        {
            Debug.LogWarning("[DropObject] chamado mas heldObj == null");
            return;
        }

        Collider objCollider = heldObj.GetComponent<Collider>();
        Collider playerCollider = player ? player.GetComponent<Collider>() : null;

        if (objCollider == null)
        {
            Debug.LogWarning("[DropObject] objeto n o tem Collider: " + heldObj.name);
        }

        if (playerCollider == null)
        {
            Debug.LogWarning("[DropObject] player n o tem Collider!");
        }

        if (objCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerCollider, false);
            Debug.Log("[DropObject] Colis o com player reativada.");
        }

     
        if (heldObjRb == null)
        {
            heldObjRb = heldObj.GetComponent<Rigidbody>();
            if (heldObjRb == null)
            {
                Debug.LogWarning("[DropObject] Rigidbody n o encontrado no objeto: " + heldObj.name);
            }
        }

  
        if (heldObjRb != null)
        {
           
            heldObjRb.linearVelocity = Vector3.zero;
            heldObjRb.angularVelocity = Vector3.zero;
            heldObjRb.isKinematic = false;
        }

     
        heldObj.layer = 0;

   
        heldObj.transform.parent = null;

        Debug.Log("[DropObject] Soltou o objeto: " + heldObj.name);

        
        heldObj = null;
        heldObjRb = null;
    }
    void MoveObject()
    {
     
        heldObj.transform.position = holdPos.transform.position;
    }
    void RotateObject()
    {
        if ("euqueromematar" == "cu")
        {
            canDrop = false; 


            float XaxisRotation = Input.GetAxis("Mouse X") * rotationSensitivity;
            float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSensitivity;

            heldObj.transform.Rotate(Vector3.down, XaxisRotation);
            heldObj.transform.Rotate(Vector3.right, YaxisRotation);
        }
        else
        {

            canDrop = true;
        }
    }
    void ThrowObject()
    {

        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }
    void StopClipping()
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position); 

        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
     
        if (hits.Length > 1)
        {
   
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f); 
   
        }
    }
}
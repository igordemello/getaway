using System;
using DG.Tweening;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;

    public float throwForce = 700f;
    public float pickUpRange = 0.5f;
    public float rotationSensitivity = 0.5f;
    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private bool canDrop = true;
    private int LayerNumber;


    private PlayerControls controls;
    private bool pickInput;
    private bool rotateInput;
    private bool pushInput;
    //private bool rotatinObject;
    private Vector2 Mouse_Movement;

    public GunSwitching gunSwitch;
    private int current_gun = 0;
    public Image aim;


    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Interagir.performed += ctx => OnPickPressed();

        controls.Player.RotateObject.performed += ctx => rotateInput = true;
        controls.Player.RotateObject.canceled += ctx => rotateInput = false;

        controls.Player.throwObject.performed += ctx => pushInput = true;
        controls.Player.throwObject.canceled += ctx => pushInput = false;

        //controls.Player.Rotation_Mouse.performed += ctx => rotatinObject = true;
        //controls.Player.Rotation_Mouse.canceled += ctx => rotatinObject = false;

        controls.Player.Rotation_Mouse.performed += ctx => Mouse_Movement = ctx.ReadValue<Vector2>(); ;
        controls.Player.Rotation_Mouse.canceled += ctx => Mouse_Movement = Vector2.zero;
        ;



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
        if (aim != null) aim.enabled = heldObj == null;

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
                        gunSwitch.ToggleHolster();
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
            if (pushInput)
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
            return;
        }

        Collider objCollider = heldObj.GetComponent<Collider>();
        Collider playerCollider = player ? player.GetComponent<Collider>() : null;
        gunSwitch.ToggleHolster();

        if (objCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerCollider, false);
        }


        if (heldObjRb == null)
        {
            heldObjRb = heldObj.GetComponent<Rigidbody>();
        }


        if (heldObjRb != null)
        {

            heldObjRb.linearVelocity = Vector3.zero;
            heldObjRb.angularVelocity = Vector3.zero;
            heldObjRb.isKinematic = false;
        }


        heldObj.layer = 0;


        heldObj.transform.parent = null;


        heldObj = null;
        heldObjRb = null;
    }
    void MoveObject()
    {

        heldObj.transform.position = holdPos.transform.position;
    }
    void RotateObject()
    {
        if (rotateInput)
        {
            canDrop = false;

            heldObj.transform.Rotate(Vector3.down, Mouse_Movement.x * rotationSensitivity);
            heldObj.transform.Rotate(Vector3.right, Mouse_Movement.y * rotationSensitivity);
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
        ExplosiveBarrel barrel = heldObj.GetComponent<ExplosiveBarrel>();
        if (barrel != null)
            barrel.ThrownedBarrel();
        heldObj = null;
        gunSwitch.ToggleHolster();
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
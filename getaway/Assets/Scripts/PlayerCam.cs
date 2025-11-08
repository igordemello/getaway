using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    private PlayerControls controls;
    private Vector2 lookInput;
    public Vector2 moveInput;

    private float lastMoveX;

    public float rotateCamByInputX = 5f;

    private float currentTilt;

    private bool leanLeftInput = false;
    private bool leanRightInput = false;


    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.LeanLeft.performed += ctx => leanLeftInput = true;
        controls.Player.LeanLeft.canceled += ctx => leanLeftInput = false;

        controls.Player.LeanRight.performed += ctx => leanRightInput = true;
        controls.Player.LeanRight.canceled += ctx => leanRightInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        DoFov(60f);
    }

    private void Update()
    {
        // get mouse input
        float mouseX = lookInput.x * Time.fixedDeltaTime * sensX;
        float mouseY = lookInput.y * Time.fixedDeltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);


        float targetTilt = moveInput.x * -rotateCamByInputX;

        if (leanLeftInput)
        {
            DoFov(55f);
            targetTilt = -(-rotateCamByInputX * 3);
        }
        else if (leanRightInput)
        {
            DoFov(55f);
            targetTilt = (-rotateCamByInputX * 3);
        }
        else
        {
            DoFov(60f);
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * 5f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, currentTilt);
        lastMoveX = moveInput.x;
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

}
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
    private Vector2 moveInput;

    private float lastMoveX;

    public float rotateCamByInputX = 5f;

    private float currentTilt;


    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
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

        Debug.Log(moveInput.x);
        
        float targetTilt = moveInput.x * -rotateCamByInputX;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * 5f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, currentTilt);
        lastMoveX = moveInput.x;
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float xTilt, float yTilt, float zTilt)
    {
        transform.DOLocalRotate(new Vector3(xTilt, yTilt, zTilt), 0.25f);
    }
}
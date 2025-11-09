using UnityEngine;
using static PlayerMovement;
public class GunSway : MonoBehaviour {
    [Header("Sway Settings")]
    [SerializeField] private float smooth; 
    [SerializeField] private float swayMultiplier; 
    public float maxSway = 10f;
    private PlayerControls controls; 
    private Vector2 lookInput;

    [Header("References")]
    public PlayerMovement pm;

    [Header("Weapon Slide Rotation X")]
    public float rotationOffsetSlide = 32f;

    private bool leanLeftInput = false;
    private bool leanRightInput = false;

    private Vector3 initialLocalPosition;

    private void Awake() 
    { 
        controls = new PlayerControls(); 
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>(); 
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.LeanLeft.performed += ctx =>
        {
            leanLeftInput = !leanLeftInput;
            if (leanLeftInput) leanRightInput = false;
        };

        controls.Player.LeanRight.performed += ctx =>
        {
            leanRightInput = !leanRightInput;
            if (leanRightInput) leanLeftInput = false;
        };
    } 
    private void OnEnable() => controls.Enable(); 
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
    }

    private void Update() 
    { 
        float mouseX = lookInput.x * swayMultiplier;         
        float mouseY = lookInput.y * swayMultiplier;

        
        mouseX = Mathf.Clamp(mouseX, -maxSway, maxSway);
        mouseY = Mathf.Clamp(mouseY, -maxSway, maxSway);

        lookInput = Vector2.ClampMagnitude(lookInput, 1f);
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);


        if (pm.state == MovementState.sliding)
        {
            rotationX *= Quaternion.Euler(-rotationOffsetSlide, 0f, 0f);
        }

        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up); 
        Quaternion targetRotation = rotationX * rotationY;

        float leanZ = 0f;
        float leanY = 0f;
        float leanXOffset = 0f;

        if (leanLeftInput)
        {
            leanZ = 20f;
            leanY = -10f;
            leanXOffset = -0.15f;
        }
        else if (leanRightInput)
        {
            leanZ = -20f;
            leanY = 10f;
            leanXOffset = 0.15f;
        }

        targetRotation *= Quaternion.Euler(0f, leanY, leanZ);

        Vector3 targetPosition = initialLocalPosition + new Vector3(leanXOffset, 0f, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smooth * Time.deltaTime);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime); 
    } 
}
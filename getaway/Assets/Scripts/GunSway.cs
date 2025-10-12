using UnityEngine;
public class GunSway : MonoBehaviour {
    [Header("Sway Settings")]
    [SerializeField] private float smooth; 
    [SerializeField] private float swayMultiplier; 
    public float maxSway = 10f;
    private PlayerControls controls; 
    private Vector2 lookInput; 
    
    private void Awake() 
    { 
        controls = new PlayerControls(); 
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>(); 
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero; 
    } 
    private void OnEnable() => controls.Enable(); 
    private void OnDisable() => controls.Disable(); 
    private void Update() 
    { 
        float mouseX = lookInput.x * swayMultiplier;         
        float mouseY = lookInput.y * swayMultiplier;

        
        mouseX = Mathf.Clamp(mouseX, -maxSway, maxSway);
        mouseY = Mathf.Clamp(mouseY, -maxSway, maxSway);

        lookInput = Vector2.ClampMagnitude(lookInput, 1f);
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right); 
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up); 
        Quaternion targetRotation = rotationX * rotationY; 
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime); 
    } 
}
using UnityEngine;

public class GunSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float smooth = 8f; // suavização
    [SerializeField] private float rotationAmount = 4f; // quanto rotacionar por direção

    private PlayerControls controls;
    private Vector2 lookInput;

    private Quaternion targetRotation;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        // Determina a rotação alvo com base na direção do movimento
        float rotX = 0f;
        float rotY = 0f;

        if (lookInput.x < 0) rotY -= rotationAmount;  // esquerda
        if (lookInput.x > 0) rotY += rotationAmount;  // direita

        if (lookInput.y > 0) rotX -= rotationAmount;  // cima
        if (lookInput.y < 0) rotX += rotationAmount;  // baixo

        targetRotation = Quaternion.Euler(rotX, rotY, 0);

        // Suaviza a transição para o novo ângulo
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            smooth * Time.fixedDeltaTime
        );
    }
}

using UnityEngine;

public class GunUIMovement : MonoBehaviour
{
    public RectTransform gunSprite;
    public PlayerMovement pm;

    [Header("Bob Config")]
    public float walkAmplitudeX = 15f;
    public float walkAmplitudeY = 8f;
    public float walkFrequency = 4f;
    public float sprintMultiplier = 2f;

    private float sprintAmplitudeX;
    private float sprintAmplitudeY;
    private float sprintFrequency;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private float moveTimer;

    void Start()
    {
        originalLocalPos = gunSprite.localPosition;
        originalLocalRot = gunSprite.localRotation;

        sprintAmplitudeX = walkAmplitudeX * sprintMultiplier;
        sprintAmplitudeY = walkAmplitudeY * sprintMultiplier;
        sprintFrequency = walkFrequency * sprintMultiplier;
    }

    void Update()
    {
        float amplitudeX = 0f;
        float amplitudeY = 0f;
        float frequency = 0f;

        float speed = pm.GetComponent<Rigidbody>().linearVelocity.magnitude;

        if (speed > 0.1f)
        {
            if (pm.state == PlayerMovement.MovementState.walking || pm.state == PlayerMovement.MovementState.air)
            {
                amplitudeX = walkAmplitudeX;
                amplitudeY = walkAmplitudeY;
                frequency = walkFrequency;
            }
            else if (pm.state == PlayerMovement.MovementState.sprinting)
            {
                amplitudeX = sprintAmplitudeX;
                amplitudeY = sprintAmplitudeY;
                frequency = sprintFrequency;
            }
        }

        if (pm.state == PlayerMovement.MovementState.sliding)
        {
            Quaternion slideRot = originalLocalRot * Quaternion.Euler(0, 0, -15f);
            gunSprite.localRotation = Quaternion.Lerp(gunSprite.localRotation, slideRot, Time.deltaTime * 10f);
        }
        else
        {
            gunSprite.localRotation = Quaternion.Lerp(gunSprite.localRotation, originalLocalRot, Time.deltaTime * 10f);
        }

        if (frequency > 0f)
        {
            moveTimer += Time.deltaTime * frequency;

            float offsetX = Mathf.Sin(moveTimer) * amplitudeX;
            float offsetY = Mathf.Cos(moveTimer * 2f) * amplitudeY * 0.5f;

            Vector3 targetPos = originalLocalPos + new Vector3(offsetX, offsetY, 0);
            gunSprite.localPosition = Vector3.Lerp(gunSprite.localPosition, targetPos, Time.deltaTime * 10f);
        }
        else
        {
            gunSprite.localPosition = Vector3.Lerp(gunSprite.localPosition, originalLocalPos, Time.deltaTime * 10f);
            moveTimer = 0f;
        }
    }
}

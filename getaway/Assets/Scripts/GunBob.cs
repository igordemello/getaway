using UnityEngine;

public class GunBob : MonoBehaviour
{

    [Header("References")]
    public PlayerMovement playerMovement;
    public Transform weaponHolder;

    [Header("Bob Settings")]
    public float walkBobAmount = 0.015f;
    public float sprintBobAmount = 0.03f;
    public float crouchBobAmount = 0.01f;
    public float slidingBobAmount = 0.03f;

    public float walkBobSpeed = 8f;
    public float sprintBobSpeed = 13f;
    public float crouchBobSpeed = 5f;
    public float slidingBobSpeed = 13f;


    [Header("Return")]
    public float smoothReturn = 6f;

    private float bobTimer;
    private Vector3 startLocalPos;

    private void Start()
    {
        if (weaponHolder == null)
        {
            weaponHolder = transform;
        }

        startLocalPos = weaponHolder.localPosition;
    }

    private void Update()
    {
        if (playerMovement == null)
        {
            return;
        }
        ApplyHeadBob();
    }

    private void ApplyHeadBob()
    {
        bool isMoving = playerMovement.grounded && (Mathf.Abs(playerMovement.rb.linearVelocity.x) > 0.1f || Mathf.Abs(playerMovement.rb.linearVelocity.z) > 0.1f);

        float bobSpeed = 0f;
        float bobAmount = 0f;

        switch (playerMovement.state)
        {
            case PlayerMovement.MovementState.walking:
                bobSpeed = walkBobSpeed;
                bobAmount = walkBobAmount;
                break;

            case PlayerMovement.MovementState.sprinting:
                bobSpeed = sprintBobSpeed;
                bobAmount = sprintBobAmount;
                break;

            case PlayerMovement.MovementState.sliding:
                bobSpeed = slidingBobSpeed;
                bobAmount = slidingBobAmount;
                break;

            case PlayerMovement.MovementState.crouching:
                bobSpeed = crouchBobSpeed;
                bobAmount = crouchBobAmount;
                break;

            default:
                bobSpeed = 0f;
                bobAmount = 0f;
                break;
        }

        if (isMoving && bobSpeed > 0f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffsetY = Mathf.Sin(bobTimer) * bobAmount;
            float bobOffsetX = Mathf.Cos(bobTimer / 2) * bobAmount * 0.5f;

            Vector3 targetPos = startLocalPos + new Vector3(bobOffsetX, bobOffsetY, 0f);
            weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition, targetPos, Time.deltaTime * smoothReturn);
        }
        else
        {
            bobTimer = 0f;
            weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition, startLocalPos, Time.deltaTime * smoothReturn);
        }
    }

}

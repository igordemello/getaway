using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float wallrunSpeed;
    public float climbSpeed;
    public float swingingSpeed;

    public float dashSpeed;
    public float maxYSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Double Jump")]
    public int maxJumps = 2;
    public float doubleJumpForce = 12f;
    private int jumpsRemaining;
    private bool jumpPressed;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Camera")]
    public Transform cameraPos;
    private float cameraStartY;
    public float crouchCameraY = 0.5f;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("UI")]
    public TextMeshProUGUI textSpeed;

    [Header("References")]
    public Transform orientation;
    public Climbing climbingScript;
    public PlayerCam cam;

    [Header("Sound System")]
    public SoundSource soundSource;

    private PlayerControls controls;
    private Vector2 moveInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool crouchInput;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public Rigidbody rb;

    public MovementState state;

    private float speedTransitionRate = 5f;

    [Header("Speed Transition Rates")]
    public float speedTransitionRateSprintToSlide = 2f;
    public float speedTransitionRateWalkToSprint = 6f;
    public float speedTransitionRateDefault = 5f;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        dashing,
        air,
        unlimited,
        freeze,
        sliding,
        swinging
    }

    public bool wallrunning;
    public bool climbing;
    public bool dashing;
    public bool freeze;
    public bool unlimited;
    public bool restricted;
    public bool activeGrapple;
    public bool swinging;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx =>
        {
            jumpInput = true;
            jumpPressed = true;
        };
        controls.Player.Jump.canceled += ctx => jumpInput = false;

        controls.Player.Sprint.performed += ctx => sprintInput = true;
        controls.Player.Sprint.canceled += ctx => sprintInput = false;

        controls.Player.Crouch.performed += ctx => crouchInput = true;
        controls.Player.Crouch.canceled += ctx => crouchInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        jumpsRemaining = maxJumps;

        startYScale = transform.localScale.y;
        cameraStartY = cameraPos.localPosition.y;
        textSpeed.text = moveSpeed.ToString();

        if (soundSource == null)
            soundSource = GetComponent<SoundSource>();
    }

    private void Update()
    {
        textSpeed.text = moveSpeed.ToString();

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, (whatIsGround | whatIsWall));

        MyInput();
        SpeedControl();
        StateHandler();

        if (state != MovementState.dashing && state != MovementState.air && grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        if (grounded && !activeGrapple)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if (jumpPressed)
            jumpPressed = false;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        if (restricted) return;

        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        if (grounded && rb.linearVelocity.y <= 0f)
        {
            jumpsRemaining = maxJumps;
        }

        if (jumpPressed && jumpsRemaining > 0 && readyToJump)
        {
            PerformJump();
        }

        if (crouchInput)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, crouchCameraY, cameraPos.localPosition.z);
        }
        else if (!crouchInput && (state == MovementState.crouching || state == MovementState.sliding))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, cameraStartY, cameraPos.localPosition.z);
        }

        if (sprintInput) {
            cam.DoFov(80f);
            soundSource.PlaySound(5f,1f);
        }
        else if (!sprintInput && state == MovementState.sprinting)
            cam.DoFov(60f);
    }

    private void PerformJump()
    {
        if (jumpsRemaining == maxJumps)
        {
            ExecuteFirstJump();
        }
        else
        {
            ExecuteExtraJump();
        }

        jumpsRemaining--;

        readyToJump = false;
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ExecuteFirstJump()
    {
        exitingSlope = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ExecuteExtraJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
    }

    bool keepMomentum;

    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
            return;
        }
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (swinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingingSpeed;
        }
        else if (grounded && sprintInput)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }

        if (crouchInput)
        {
            if (state == MovementState.sprinting && !(lastDesiredMoveSpeed > desiredMoveSpeed))
            {
                state = MovementState.sliding;
                desiredMoveSpeed = crouchSpeed;
            }
            else
            {
                state = MovementState.crouching;
                desiredMoveSpeed = crouchSpeed;
            }
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();

            if (state == MovementState.sliding && lastDesiredMoveSpeed > desiredMoveSpeed)
                speedTransitionRate = speedTransitionRateSprintToSlide;
            else if (state == MovementState.sprinting && lastDesiredMoveSpeed < desiredMoveSpeed)
                speedTransitionRate = speedTransitionRateWalkToSprint;
            else
                speedTransitionRate = speedTransitionRateDefault;

            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        if (state == MovementState.sliding)
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude <= crouchSpeed + 0.1f)
            {
                state = MovementState.crouching;
                desiredMoveSpeed = crouchSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * speedTransitionRate;
            yield return null;
        }
    }

    private void MovePlayer()
    {
        if (dashing || activeGrapple || swinging || restricted || climbingScript.exitingWall) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            Vector3 airMove = moveDirection.normalized * moveSpeed * airMultiplier;

            if (rb.linearVelocity.magnitude < moveSpeed * 1.3f)
                rb.AddForce(airMove, ForceMode.Acceleration);
            else
                rb.AddForce(airMove * 0.5f, ForceMode.Acceleration);
        }

        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (dashing || activeGrapple) return;

        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
    }

    private void Jump()
    {
        if (jumpsRemaining > 0 && readyToJump)
        {
            PerformJump();
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public Vector3 CalculatedJumpVelocity(Vector3 startpoint, Vector3 endpoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endpoint.y - startpoint.y;
        Vector3 displacementXZ = new Vector3(endpoint.x - startpoint.x, 0, endpoint.z - startpoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private bool enableMovementOnNextTouch;
    private Vector3 velocityToSet;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        velocityToSet = CalculatedJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        activeGrapple = true;
        Invoke(nameof(SetVelocity), 0.1f);
        Invoke(nameof(ResetRestrictions), Vector3.Distance(rb.position, targetPosition) / 10);
    }

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    public void ResetJumps()
    {
        jumpsRemaining = maxJumps;
    }

    public void AddJump()
    {
        jumpsRemaining++;
    }

    public void SetJumps(int amount)
    {
        jumpsRemaining = Mathf.Max(0, amount);
    }

    public int GetRemainingJumps()
    {
        return jumpsRemaining;
    }

    public int GetMaxJumps()
    {
        return maxJumps;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6 && enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponent<Grappling>().StopGrapple();
        }
        if (collision.gameObject.layer == 10 ) {
            soundSource.PlaySound(7f,1f);
        }
    }
}
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

    public float dashSpeed;
    public float maxYSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

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
    }

    public bool wallrunning;
    public bool climbing;
    public bool dashing;
    public bool freeze;
    public bool unlimited;
    public bool restricted;
    public bool activeGrapple;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpInput = true;
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

        startYScale = transform.localScale.y;

        cameraStartY = cameraPos.localPosition.y;

        textSpeed.text = moveSpeed.ToString();
    }

    private void Update()
    {
        // ui text speed
        textSpeed.text = moveSpeed.ToString();

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, (whatIsGround | whatIsWall));

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (state != MovementState.dashing && state != MovementState.air && grounded)
        {
            rb.linearDamping = groundDrag;

        }
        else
        {
            rb.linearDamping = 0;
        }
        if (grounded && !activeGrapple)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
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

        // when to jump
        if (jumpInput && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crounch
        if (crouchInput)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, crouchCameraY, cameraPos.localPosition.z);
        }
        // stop crounch
        else if (!crouchInput && (state == MovementState.crouching || state == MovementState.sliding))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);

            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, cameraStartY, cameraPos.localPosition.z);
        }

        if (sprintInput)
        {
            cam.DoFov(80f);
        }
        else if (!sprintInput && state == MovementState.sprinting)
        {
            cam.DoFov(60f);
        }
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

        // Mode - Unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
            return;
        }

        //Mode - Dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
        }

        // Mode - Climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        // Mode - WallRunning
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }


        // Mode - Sprinting
        else if (grounded && sprintInput)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // Mode - crouching
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

        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();

            // Controla o tempo de transição de acordo com o estado atual e o próximo
            if (state == MovementState.sliding && lastDesiredMoveSpeed > desiredMoveSpeed)
                speedTransitionRate = speedTransitionRateSprintToSlide; // quanto menor mais dura o slide
            else if (state == MovementState.sprinting && lastDesiredMoveSpeed < desiredMoveSpeed)
                speedTransitionRate = speedTransitionRateWalkToSprint; // acelera mais rápido ao começar a correr
            else
                speedTransitionRate = speedTransitionRateDefault; // padrão

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
        // smoothly lerp movementSpeed to desired 
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        //float lerpSpeed = 5f; // aumentar isso para diminuir o tempo entre a velocidade normal e a velocidade de corrida

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            //time += Time.deltaTime * lerpSpeed;
            time += Time.deltaTime * speedTransitionRate;
            yield return null;
        }
    }

    private void MovePlayer()

    {
        if (activeGrapple) return;

        if (restricted) return; // se der merda inverte a ordem com o de baixo

        if (climbingScript.exitingWall) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // on ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
        {
            Vector3 airMove = moveDirection.normalized * moveSpeed * airMultiplier;

            if (rb.linearVelocity.magnitude < moveSpeed * 1.3f)
                rb.AddForce(airMove, ForceMode.Acceleration);
            else
                rb.AddForce(airMove * 0.5f, ForceMode.Acceleration);
        }

        // turn gravity off while on slope
        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limited velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
        }


    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity 
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        velocityToSet = CalculatedJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        activeGrapple = true;
        Invoke(nameof(SetVelocity), 0.1f);
        Invoke(nameof(ResetRestrictions), Vector3.Distance(rb.position, targetPosition) / 10);
    }
    private Vector3 velocityToSet;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;
    }
    public void ResetRestrictions()
    {
        activeGrapple = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6 && enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponent<Grappling>().StopGrapple();
        }
    }
}

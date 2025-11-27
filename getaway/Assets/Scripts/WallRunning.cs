using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningAdvanced : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce = 100f;
    public float wallJumpSideForce = 100f;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Wall Kick - Simples ×5 Horizontal")]
    public float wallKickHorizontalMultiplier = 5f; // ← 5× força horizontal
    public float wallKickCooldown = 2f;
    private bool wallKickInput;
    private bool canWallKick = true;

    [Header("Input")]
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool jumpInput;
    private bool upwardsRunInput;
    private bool downwardsRunInput;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;
    private Vector3 lastWallNormal;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("Anti Return Force")]
    public float antiReturnForce = 10f;
    public float antiReturnDuration = 0.5f;
    private float antiReturnTimer;
    private bool applyingAntiReturn;

    [Header("References")]
    public Transform orientation;
    public PlayerCam cam;
    private PlayerMovement pm;
    private LedgeGrabbing lg;
    private Rigidbody rb;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpInput = true;
        controls.Player.Jump.canceled += ctx => jumpInput = false;

        controls.Player.WallRunUP.performed += ctx => upwardsRunInput = true;
        controls.Player.WallRunUP.canceled += ctx => upwardsRunInput = false;

        controls.Player.WallRunDOWN.performed += ctx => downwardsRunInput = true;
        controls.Player.WallRunDOWN.canceled += ctx => downwardsRunInput = false;

        // ← NOVO INPUT: Wall Kick com a tecla V
        controls.Player.WallKick.performed += ctx => wallKickInput = true;
        controls.Player.WallKick.canceled += ctx => wallKickInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        lg = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
        HandleAntiReturnForce();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        upwardsRunning = upwardsRunInput;
        downwardsRunning = downwardsRunInput;

        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall && !applyingAntiReturn)
        {
            if (!pm.wallrunning)
                StartWallRun();

            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && pm.wallrunning)
            {
                WallJump();
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (jumpInput) WallJump();

            // ← NOVA VERIFICAÇÃO: Wall Kick
            if (wallKickInput && canWallKick)
            {
                WallKick();
            }
        }
        else if (exitingWall)
        {
            if (pm.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
        pm.wasWallrunning = true;
        wallRunTimer = maxWallRunTime;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        lastWallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        cam.DoFov(70f);
        if (wallLeft) cam.moveInput.x = 1;
        if (wallRight) cam.moveInput.x = -1;

        // ← RESETA WALL KICK AO INICIAR WALLRUN
        canWallKick = true;
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        float descentSpeed = wallClimbSpeed * 0.08f;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, -descentSpeed, rb.linearVelocity.z);

        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;
        cam.DoFov(60f);
        cam.moveInput.x = 0;
    }

    private void WallJump()
    {
        if (lg.holding || lg.exitingLedge) return;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        lastWallNormal = wallNormal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        StartAntiReturnForce();
    }

    // ← MÉTODO Wall Kick SIMPLES: WallJump com horizontal ×5
    private void WallKick()
    {
        if (lg.holding || lg.exitingLedge) return;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        lastWallNormal = wallNormal;

        // ← MESMA FÓRMULA DO WALLJUMP, MAS HORIZONTAL ×5
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * (wallJumpSideForce * wallKickHorizontalMultiplier);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        // Ativa cooldown
        canWallKick = false;
        Invoke(nameof(ResetWallKick), wallKickCooldown);

        Debug.Log($"Wall Kick ×{wallKickHorizontalMultiplier} - Mesma vertical, horizontal ×5");

        StartAntiReturnForce();
    }

    // ← NOVO MÉTODO: Reset do Wall Kick
    private void ResetWallKick()
    {
        canWallKick = true;
    }

    private void StartAntiReturnForce()
    {
        applyingAntiReturn = true;
        antiReturnTimer = antiReturnDuration;
    }

    private void HandleAntiReturnForce()
    {
        if (applyingAntiReturn)
        {
            if (antiReturnTimer > 0)
            {
                Vector3 antiReturnDirection = -lastWallNormal;
                rb.AddForce(antiReturnDirection * antiReturnForce, ForceMode.Force);
                antiReturnTimer -= Time.deltaTime;
            }
            else
            {
                applyingAntiReturn = false;
            }
        }
    }
}
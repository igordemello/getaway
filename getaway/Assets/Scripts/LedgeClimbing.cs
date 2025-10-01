using System;
using System.Collections;
using UnityEngine;

public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement pm;
    public Transform orientation;
    public Transform cam;
    public Rigidbody rb;

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed = 6f;
    public float maxLedgeGrabDistance = 2f;
    public float ledgeHoldOffset = 0.6f; // quanto ficar afastado da superfície
    public float minTimeOnLedge = 0.2f;
    private float timeOnLedge;
    public bool holding;

    [Header("Ledge Jumping")]
    public float ledgeJumpForwardForce = 6f;
    public float ledgeJumpUpwardForce = 4f;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength = 2f;
    public float ledgeSphereCastRadius = 0.5f;
    public LayerMask whatIsLedge;

    // dados do hit
    private RaycastHit ledgeHit;
    private Vector3 currLedgePoint;
    private Vector3 currLedgeNormal;
    private Vector3 lastLedgePoint = Vector3.positiveInfinity; // ponto da última ledge usada
    private float ignoreSameLedgeThreshold = 0.6f; // distancia pra considerar "mesma ledge"

    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTime = 0.3f;
    private float exitLedgeTimer;

    // Timeout para aproximação
    private float ledgeApproachStartTime;
    public float ledgeApproachMaxTime = 0.35f; // ajustar em ms para seu jogo

    [Header("Debug")]
    public bool debugGizmos = false;

    private PlayerControls controls;
    private Vector2 moveInput;
    private bool jumpInput;

    private void Update()
    {
        DetectLedge();
        HandleStates();
    }

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpInput = true;
        controls.Player.Jump.canceled += ctx => jumpInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void HandleStates()
    {
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;
        bool anyInputKeyPressed = horizontalInput != 0 || verticalInput != 0;

        if (holding)
        {
            HoldOnLedge();

            timeOnLedge += Time.deltaTime;

            if (timeOnLedge > minTimeOnLedge && anyInputKeyPressed)
                ExitLedgeHold();

            if (jumpInput)
                LedgeJump();
        }
        else if (exitingLedge)
        {
            exitLedgeTimer -= Time.deltaTime;
            if (exitLedgeTimer <= 0) exitingLedge = false;
        }
    }

    private void DetectLedge()
    {
        // origin: câmera (mais confiável em first-person)
        Vector3 origin = cam != null ? cam.position : transform.position;

        bool ledgeDetected = Physics.SphereCast(
            origin,
            ledgeSphereCastRadius,
            cam.forward,
            out ledgeHit,
            ledgeDetectionLength,
            whatIsLedge
        );

        if (!ledgeDetected) return;

        // use o ponto exato do hit para medir
        float hitDistance = ledgeHit.distance;
        Vector3 hitPoint = ledgeHit.point;

        // ignora se é a mesma ledge já usada recentemente (pequena tolerância)
        if (lastLedgePoint != Vector3.positiveInfinity &&
            Vector3.Distance(hitPoint, lastLedgePoint) < ignoreSameLedgeThreshold)
            return;

        if (hitDistance < maxLedgeGrabDistance && !holding)
            EnterLedgeHold(ledgeHit);
    }

    private void EnterLedgeHold(RaycastHit hit)
    {
        holding = true;

        pm.unlimited = true;
        pm.restricted = true;

        currLedgePoint = hit.point;
        currLedgeNormal = hit.normal;
        lastLedgePoint = currLedgePoint;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        timeOnLedge = 0f;
        ledgeApproachStartTime = Time.time;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void HoldOnLedge()
    {
        rb.useGravity = false;
        Vector3 targetPoint = currLedgePoint - currLedgeNormal * ledgeHoldOffset;
        Vector3 dirToTarget = targetPoint - transform.position;
        float distanceToTarget = dirToTarget.magnitude;
        if (Time.time - ledgeApproachStartTime > ledgeApproachMaxTime && distanceToTarget > 0.6f)
        {
            ExitLedgeHold(true); 
            return;
        }
        if (distanceToTarget > 0.6f)
        {
      
            rb.linearVelocity = dirToTarget.normalized * moveToLedgeSpeed;


            float maxAllowed = Mathf.Max(moveToLedgeSpeed * 1.5f, moveToLedgeSpeed);
            if (rb.linearVelocity.magnitude > maxAllowed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxAllowed;
        }
        else
        {
   
            rb.linearVelocity = Vector3.zero;
            if (!pm.freeze) pm.freeze = true;
            if (pm.unlimited) pm.unlimited = false;
        }

      
        if (Vector3.Distance(transform.position, currLedgePoint) > maxLedgeGrabDistance)
            ExitLedgeHold();
    }

    private void LedgeJump()
    {
  
        ExitLedgeHold();
        Invoke(nameof(ApplyLedgeJumpForce), 0.05f);
    }

    private void ApplyLedgeJumpForce()
    {
      
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;

        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void ExitLedgeHold(bool resetHorizontal = false)
    {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false;
        timeOnLedge = 0f;

        pm.restricted = false;
        pm.freeze = false;

        rb.useGravity = true;

        if (resetHorizontal) ResetHorizontalVelocity(pm.walkSpeed);

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetHorizontalVelocity(float maxSpeed)
    {
        Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flat.magnitude > maxSpeed && maxSpeed > 0f)
            flat = flat.normalized * maxSpeed;
        rb.linearVelocity = new Vector3(flat.x, rb.linearVelocity.y, flat.z);
    }

    private void ResetLastLedge()
    {
        lastLedgePoint = Vector3.positiveInfinity;
    }

    private void OnDrawGizmos()
    {
        if (!debugGizmos || cam == null) return;

        Gizmos.color = Color.cyan;
        Vector3 origin = cam.position;
        Gizmos.DrawWireSphere(origin + cam.forward * ledgeDetectionLength, ledgeSphereCastRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currLedgePoint, 0.05f);

        Gizmos.color = Color.magenta;
        Vector3 targetPoint = currLedgePoint - currLedgeNormal * ledgeHoldOffset;
        Gizmos.DrawWireSphere(targetPoint, 0.05f);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
public class LedgeGrabbing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("References")]
    public PlayerMovement pm;
    public Transform orientation;
    public Transform cam;
    public Rigidbody rb;


    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    private float timeOnLedge;

    public bool holdingLedge;
    [Header("Ledge Jumping")]
    public KeyCode jumpKey = KeyCode.Space;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpwardForce;

    private Transform lastLedge;
    private Transform currLedge;

    private RaycastHit ledgeHit;

    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTimer;
    public float exitLedgeTime;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        LedgeDetection();
        SubStateMachine();
        
    }

    private void LedgeDetection()
    {
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);
        if (!ledgeDetected) return;
        print("ledge");
        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);
        // if (ledgeHit.transform == lastLedge) return;// Prevents grabbing the same ledge multiple times in a row -> we will change that
        if (distanceToLedge < maxLedgeGrabDistance && !holdingLedge) EnterLedgeHold();
    }
    private void EnterLedgeHold()
    {
        holdingLedge = true;

        pm.unlimited = true;
        pm.restricted = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero; 
    }
    private void FreezeRigidBodyOnLedge()
    {
        rb.useGravity = false;

        Vector3 directionToLedge = (currLedge.position - transform.position);
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        if (distanceToLedge > 1f)
        {

            if (rb.linearVelocity.magnitude < moveToLedgeSpeed)
            {
                rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);
            }

        }
        else
        {
            if (!pm.freeze) pm.freeze = true;
            if (pm.unlimited) pm.unlimited = false;
        }
        if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
    }
    private void ResetLastLedge()
    {
        lastLedge = null;
    }

    private void ExitLedgeHold()
    {

        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holdingLedge = false;
        timeOnLedge = 0f;

        pm.restricted = false;
        pm.freeze = false;

        rb.useGravity = true;
        Invoke("ResetLastLedge", 1f);
    }
    private void SubStateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool anyInputKeyPressed = horizontalInput != 0 || verticalInput != 0;
        if (holdingLedge)
        {
            FreezeRigidBodyOnLedge();
            timeOnLedge += Time.deltaTime;
            if (timeOnLedge > minTimeOnLedge && anyInputKeyPressed) ExitLedgeHold();
            if (Input.GetKeyDown(jumpKey)) LedgeJump();
        }
        else if (exitingLedge)
        {

            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;

        }

    }
    private void LedgeJump()
    {
        ExitLedgeHold();
        Invoke("DelayedJumpForce", 0.05f);

    }
    private void DelayedJumpForce()
    {   
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }
}

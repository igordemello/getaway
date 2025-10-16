using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Importante: novo sistema de input

public class Swinging : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("Swing Time Limit")]
    [Tooltip("Tempo máximo (em segundos) que o jogador pode segurar a corda.")]
    public float maxSwingTime = 3f;
    private Coroutine swingTimerCoroutine;

    [Header("OdmGear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce = 10f;
    public float forwardThrustForce = 15f;
    public float extendCableSpeed = 5f;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius = 1f;
    public Transform predictionPoint;

    private PlayerControls controls;
    private Vector2 moveInput;
    private bool swingingInput;

    private Vector3 currentGrapplePosition;

    private void Awake()
    {
        // Instancia o mapa de controle gerado pelo novo Input System
        controls = new PlayerControls();

        // Movimento (ex: W, A, S, D ou analógico)
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Swing (gatilho ou botão)
        controls.Player.Swinging.performed += ctx => swingingInput = true;
        controls.Player.Swinging.canceled += ctx => swingingInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        // Entrada de Swing
        if (swingingInput && joint == null && predictionHit.point != Vector3.zero)
            StartSwing();
        else if (!swingingInput && joint != null)
            StopSwing();

        CheckForSwingPoints();

        if (joint != null)
            OdmGearMovement();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void CheckForSwingPoints()
    {
        if (joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
            out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
            out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        // Escolhe o ponto mais válido
        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;
        else
            realHitPoint = Vector3.zero;

        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    private void StartSwing()
    {
        if (predictionHit.point == Vector3.zero) return;

        // Se houver outro Grappling ativo, desativa
        if (GetComponent<Grappling>() != null)
            GetComponent<Grappling>().StopGrapple();
        pm.ResetRestrictions();

        pm.swinging = true;
        swingPoint = predictionHit.point;

        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;

        if (swingTimerCoroutine != null) StopCoroutine(swingTimerCoroutine);
        swingTimerCoroutine = StartCoroutine(SwingTimeLimit());
    }

    public void StopSwing()
    {
        pm.swinging = false;
        lr.positionCount = 0;
        if (joint != null) Destroy(joint);
        if (swingTimerCoroutine != null)
        {
            StopCoroutine(swingTimerCoroutine);
            swingTimerCoroutine = null;
        }
    }

    private IEnumerator SwingTimeLimit()
    {
        yield return new WaitForSeconds(maxSwingTime);
        swingTimerCoroutine = null;
        swingingInput = false;
        StopSwing();
    }

    private void OdmGearMovement()
    {
        // Converte o input 2D em movimento real no espaço (novo Input System)
        Vector3 moveDir = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        rb.AddForce(moveDir.normalized * horizontalThrustForce * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void DrawRope()
    {
        if (!joint || lr.positionCount < 2) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }
}

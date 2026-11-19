using System;
using UnityEngine;

/// <summary>
/// Gestiona la capacidad del jugador para recoger, sostener y soltar objetos con físicas naturales,
/// manteniendo un balanceo e inercia orgánicos de la mano (Hand Sway & Bobbing) sin rotación ni zoom manual.
/// </summary>
public class PlayerPickup : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Configuración de Soltado")]
    [SerializeField] private float dropDistance = 1.2f;
    [SerializeField] private float minDropDistance = 0.5f;

    [Header("Inercia y Balanceo de Mano (Hand Sway)")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float maxSwayAmount = 0.05f;
    [SerializeField] private float swaySmooth = 6f;
    [SerializeField] private float bobbingSpeed = 8f;
    [SerializeField] private float bobbingAmount = 0.012f;

    private PickupObject heldObject;
    private Collider playerCollider;
    private Vector3 currentSwayOffset;
    private float bobbingTimer;

    public bool HasObject => heldObject != null;
    public PickupObject HeldObject => heldObject;

    public event Action<PickupObject> OnObjectPickedUp;
    public event Action OnObjectDropped;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        playerCollider = GetComponent<Collider>();
        if (playerCollider == null)
        {
            playerCollider = GetComponent<CharacterController>();
        }

        if (holdPoint == null && playerCamera != null)
        {
            Transform existingHoldPoint = playerCamera.transform.Find("HoldPoint");
            if (existingHoldPoint != null)
            {
                holdPoint = existingHoldPoint;
            }
            else
            {
                GameObject newHoldPoint = new GameObject("HoldPoint");
                newHoldPoint.transform.SetParent(playerCamera.transform);
                newHoldPoint.transform.localPosition = new Vector3(0.32f, -0.22f, 0.75f);
                newHoldPoint.transform.localRotation = Quaternion.identity;
                holdPoint = newHoldPoint.transform;
            }
        }
    }

    private void Update()
    {
        if (heldObject == null) return;

        CalculateHandSway();
    }

    private void LateUpdate()
    {
        if (heldObject == null || holdPoint == null) return;

        UpdateHeldObjectTransform();
    }

    /// <summary>
    /// Calcula el balanceo orgánico de la mano al girar la cámara y al caminar.
    /// </summary>
    private void CalculateHandSway()
    {
        // Sway por movimiento de ratón
        float moveX = -Input.GetAxis("Mouse X") * swayAmount;
        float moveY = -Input.GetAxis("Mouse Y") * swayAmount;

        moveX = Mathf.Clamp(moveX, -maxSwayAmount, maxSwayAmount);
        moveY = Mathf.Clamp(moveY, -maxSwayAmount, maxSwayAmount);

        Vector3 targetSway = new Vector3(moveX, moveY, 0f);

        // Bobbing al caminar
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f);

        if (isMoving)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            targetSway.y += Mathf.Sin(bobbingTimer) * bobbingAmount;
            targetSway.x += Mathf.Cos(bobbingTimer * 0.5f) * (bobbingAmount * 0.5f);
        }
        else
        {
            bobbingTimer = 0f;
        }

        currentSwayOffset = Vector3.Lerp(currentSwayOffset, targetSway, Time.deltaTime * swaySmooth);
    }

    /// <summary>
    /// Aplica la posición natural en la mano con la inercia del movimiento.
    /// </summary>
    private void UpdateHeldObjectTransform()
    {
        Vector3 targetPosition = holdPoint.position +
                                 holdPoint.right * currentSwayOffset.x +
                                 holdPoint.up * currentSwayOffset.y;

        heldObject.transform.position = Vector3.Lerp(
            heldObject.transform.position,
            targetPosition,
            Time.deltaTime * 20f
        );
    }

    /// <summary>
    /// Intenta recoger el objeto especificado si las condiciones se cumplen.
    /// </summary>
    public bool TryPickup(PickupObject pickupObject)
    {
        if (heldObject != null || pickupObject == null || holdPoint == null)
            return false;

        heldObject = pickupObject;
        heldObject.Hold(holdPoint, playerCollider);
        OnObjectPickedUp?.Invoke(heldObject);
        return true;
    }

    /// <summary>
    /// Suelta el objeto sostenido de forma segura delante del jugador sin transferirle impulsos erráticos.
    /// </summary>
    public void DropObject()
    {
        if (heldObject == null) return;

        Vector3 dropPos = CalculateSafeDropPosition();
        PickupObject droppingObject = heldObject;
        heldObject = null;

        droppingObject.Drop(playerCollider, dropPos);
        OnObjectDropped?.Invoke();
    }

    /// <summary>
    /// Calcula un punto seguro para soltar el objeto evitando que quede incrustado dentro de muros o el suelo.
    /// </summary>
    private Vector3 CalculateSafeDropPosition()
    {
        Vector3 rayOrigin = playerCamera != null ? playerCamera.transform.position : transform.position + Vector3.up * 1.5f;
        Vector3 rayDirection = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        float targetDistance = dropDistance;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, dropDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Max(minDropDistance, hit.distance - 0.25f);
        }

        return rayOrigin + rayDirection * targetDistance;
    }
}

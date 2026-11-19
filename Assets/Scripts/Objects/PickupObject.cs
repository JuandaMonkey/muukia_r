using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona las físicas y el estado de un objeto que puede ser recogido y soltado por el jugador.
/// Implementa IInteractable para integrarse con el sistema de interacción.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PickupObject : MonoBehaviour, IInteractable
{
    [Header("Configuración de Recogida")]
    [SerializeField] private string itemName = "Objeto";
    [SerializeField] private Vector3 holdRotationOffset = Vector3.zero;

    private Rigidbody rb;
    private Collider[] colliders;
    private InteractableOutline outline;

    public bool IsBeingHeld { get; private set; }
    public string ItemName => itemName;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        outline = GetComponent<InteractableOutline>();

        // Configuración óptima de física para evitar comportamientos anómalos
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (colliders == null || colliders.Length == 0)
        {
            Debug.LogError($"[PickupObject] El objeto '{name}' necesita al menos un Collider.");
        }
    }

    public void Interact(GameObject interactor)
    {
        PlayerPickup playerPickup = interactor.GetComponent<PlayerPickup>();
        if (playerPickup != null && !IsBeingHeld)
        {
            playerPickup.TryPickup(this);
        }
    }

    /// <summary>
    /// Sostiene el objeto en el punto asignado (HoldPoint), desactivando colisiones con el jugador y congelando su física.
    /// </summary>
    public void Hold(Transform holdPoint, Collider playerCollider)
    {
        if (holdPoint == null) return;

        IsBeingHeld = true;

        // Desactivar outline si estuviera encendido
        if (outline != null)
        {
            outline.SetHighlighted(false);
        }

        // Ignorar colisiones entre los colliders de este objeto y el jugador
        if (playerCollider != null && colliders != null)
        {
            foreach (Collider col in colliders)
            {
                if (col != null && col.enabled)
                {
                    Physics.IgnoreCollision(col, playerCollider, true);
                }
            }
        }

        // Detener completamente cualquier fuerza previa acumulada
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Desactivar colliders mientras se sostiene para evitar atascos con muros
        SetCollidersEnabled(false);

        // Emparentar al holdPoint
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(holdRotationOffset);
    }

    /// <summary>
    /// Suelta el objeto en el mundo de manera limpia, permitiendo que la gravedad actúe sin impulsos artificiales.
    /// </summary>
    public void Drop(Collider playerCollider, Vector3 dropPosition)
    {
        IsBeingHeld = false;

        // Desemparentar antes de cambiar la posición para no heredar transformaciones
        transform.SetParent(null);
        transform.position = dropPosition;

        // Reactivar colliders
        SetCollidersEnabled(true);

        // Reactivar física
        rb.isKinematic = false;
        rb.useGravity = true;

        // Resetear de forma estricta cualquier velocidad residual
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Mantener temporalmente ignorada la colisión con el jugador durante 0.5s para evitar despenetración violenta
        if (playerCollider != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(RestorePlayerCollisionRoutine(playerCollider, 0.5f));
        }
    }

    /// <summary>
    /// Permite preparar el objeto para un futuro lanzamiento sin romper la física de caída natural.
    /// </summary>
    public void Throw(Collider playerCollider, Vector3 dropPosition, Vector3 throwForce)
    {
        Drop(playerCollider, dropPosition);
        rb.AddForce(throwForce, ForceMode.Impulse);
    }

    private void SetCollidersEnabled(bool enabledState)
    {
        if (colliders == null) return;
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = enabledState;
            }
        }
    }

    private IEnumerator RestorePlayerCollisionRoutine(Collider playerCollider, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerCollider != null && colliders != null)
        {
            foreach (Collider col in colliders)
            {
                if (col != null)
                {
                    Physics.IgnoreCollision(col, playerCollider, false);
                }
            }
        }
    }
}

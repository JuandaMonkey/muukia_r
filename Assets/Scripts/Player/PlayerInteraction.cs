using System;
using UnityEngine;

/// <summary>
/// Gestiona la detección por Raycast de objetos interactuables, el control del contorno (outline)
/// y la ejecución de interacciones contextuales.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private PlayerPickup playerPickup;
    private IInteractable currentInteractable;
    private GameObject currentInteractableGO;
    private InteractableOutline currentOutline;

    public IInteractable CurrentInteractable => currentInteractable;
    public bool IsTargetingInteractable => currentInteractable != null;

    public event Action<string> OnPromptChanged;
    public event Action OnPromptCleared;

    private void Awake()
    {
        playerPickup = GetComponent<PlayerPickup>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    private void Update()
    {
        CheckInteractionRaycast();
        HandleInput();
    }

    /// <summary>
    /// Ejecuta el Raycast central y gestiona el resaltado de contorno (outline) e indicadores de HUD.
    /// </summary>
    private void CheckInteractionRaycast()
    {
        // Si el jugador ya sostiene un objeto, la acción principal es soltarlo
        if (playerPickup != null && playerPickup.HasObject)
        {
            ClearTargetOutline();
            OnPromptChanged?.Invoke("Presiona E para soltar");
            return;
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (interactable != null)
            {
                GameObject targetGO = (interactable as Component)?.gameObject;

                // Si cambiamos de objetivo, actualizar outline
                if (targetGO != currentInteractableGO)
                {
                    ClearTargetOutline();

                    currentInteractable = interactable;
                    currentInteractableGO = targetGO;

                    if (targetGO != null)
                    {
                        currentOutline = targetGO.GetComponent<InteractableOutline>();
                        if (currentOutline == null)
                        {
                            currentOutline = targetGO.AddComponent<InteractableOutline>();
                        }
                        currentOutline.SetHighlighted(true);
                    }
                }

                // Determinar el mensaje para el HUD
                string prompt = GetContextualPrompt(interactable);
                OnPromptChanged?.Invoke(prompt);
                return;
            }
        }

        // Si no estamos mirando a nada interactuable
        if (currentInteractable != null)
        {
            ClearTargetOutline();
            OnPromptCleared?.Invoke();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (playerPickup != null && playerPickup.HasObject)
            {
                playerPickup.DropObject();
                ClearTargetOutline();
                OnPromptCleared?.Invoke();
            }
            else if (currentInteractable != null)
            {
                IInteractable target = currentInteractable;
                ClearTargetOutline();
                target.Interact(gameObject);
            }
        }
    }

    private string GetContextualPrompt(IInteractable interactable)
    {
        if (interactable is PickupObject pickupObj)
        {
            return $"Presiona E para recoger {pickupObj.ItemName}";
        }

        if (interactable is InteractableObject io)
        {
            return io.PromptMessage;
        }

        return "Presiona E para interactuar";
    }

    private void ClearTargetOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.SetHighlighted(false);
            currentOutline = null;
        }
        currentInteractable = null;
        currentInteractableGO = null;
    }
}

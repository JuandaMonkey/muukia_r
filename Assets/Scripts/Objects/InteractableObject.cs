using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente genérico para objetos interactuables en el mundo (palancas, puertas, notas, interruptores).
/// Para objetos que pueden ser recogidos, utilícese PickupObject.
/// </summary>
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Configuración General")]
    [SerializeField] private string objectName = "Objeto Interactuable";
    [SerializeField] private string promptMessage = "Presiona E para interactuar";

    [Header("Eventos")]
    public UnityEvent<GameObject> onInteracted = new UnityEvent<GameObject>();

    public string ObjectName => objectName;
    public string PromptMessage => promptMessage;

    public virtual void Interact(GameObject interactor)
    {
        Debug.Log($"[InteractableObject] Interacción realizada con '{objectName}' por '{interactor.name}'.");
        onInteracted?.Invoke(interactor);
    }
}

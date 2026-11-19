using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MUUKIA
{
    /// <summary>
    /// Componente para activar diálogos mediante interacción con el jugador (IInteractable)
    /// o automáticamente al entrar en un área Trigger.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [Header("Tipo de Activación")]
        [SerializeField] private bool triggerOnProximity = false;
        [SerializeField] private bool triggerOnInteraction = true;
        [SerializeField] private bool onlyOnce = true;

        [Header("Líneas de Diálogo")]
        [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();

        [Header("Eventos")]
        [SerializeField] private UnityEvent onDialogueStarted = new UnityEvent();
        [SerializeField] private UnityEvent onDialogueFinished = new UnityEvent();

        private bool hasTriggered = false;

        public void Interact(GameObject interactor)
        {
            if (triggerOnInteraction && (!onlyOnce || !hasTriggered))
            {
                TriggerDialogue();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnProximity && (!onlyOnce || !hasTriggered))
            {
                if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
                {
                    TriggerDialogue();
                }
            }
        }

        public void TriggerDialogue()
        {
            if (DialogueManager.Instance == null) return;
            if (onlyOnce && hasTriggered) return;

            hasTriggered = true;
            onDialogueStarted?.Invoke();

            DialogueManager.Instance.PlayDialogueSequence(dialogueLines, () =>
            {
                onDialogueFinished?.Invoke();
            });
        }
    }
}

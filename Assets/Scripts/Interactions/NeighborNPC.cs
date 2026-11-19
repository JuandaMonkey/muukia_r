using System;
using System.Collections.Generic;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Controlador del Vecino en la calle Fernando Montes de Oca.
    /// Revela a Gustavo los datos clave sobre el Dr. Miguel Maderos, el Castillo y las Criptas.
    /// </summary>
    public class NeighborNPC : MonoBehaviour, IInteractable
    {
        [Header("Referencias")]
        [SerializeField] private PS2CharacterAnimator characterAnimator;

        public bool HasCompletedDialogue { get; private set; }

        public event Action OnConversationFinished;

        public void Interact(GameObject interactor)
        {
            if (!HasCompletedDialogue)
            {
                TriggerNeighborDialogue();
            }
        }

        public void TriggerNeighborDialogue(Action onComplete = null)
        {
            if (DialogueManager.Instance == null) return;
            HasCompletedDialogue = true;

            var lines = new List<DialogueLine>
            {
                new DialogueLine { speakerName = "GUSTAVO", text = "Buenas tardes, señor." },
                new DialogueLine { speakerName = "VECINO", text = "Buenas." },
                new DialogueLine { speakerName = "GUSTAVO", text = "Disculpe, ¿usted lleva mucho tiempo viviendo por aquí?" },
                new DialogueLine { speakerName = "VECINO", text = "Toda la vida, prácticamente. ¿Por qué?" },
                new DialogueLine { speakerName = "GUSTAVO", text = "Por esa casa. ¿Sabe quién la construyó?" },
                new DialogueLine { speakerName = "VECINO", text = "Ah, el castillo. Lo hizo un doctor. Miguel Maderos, creo que se llamaba. Era doctor del Seguro." },
                new DialogueLine { speakerName = "GUSTAVO", text = "¿Y todavía vive por aquí?" },
                new DialogueLine { speakerName = "VECINO", text = "No, hombre. Ya murió hace bastante." },
                new DialogueLine { speakerName = "GUSTAVO", text = "¿Y sabe si construyó alguna otra cosa?" },
                new DialogueLine { speakerName = "VECINO", text = "Pues... las criptas, creo." },
                new DialogueLine { speakerName = "GUSTAVO", text = "¿Qué criptas?" },
                new DialogueLine { speakerName = "VECINO", text = "Las de Obregón. Las que están por las Granjas Micas. Según yo, también las hizo él." },
                new DialogueLine { speakerName = "VECINO", text = "¿Se siente bien, joven? Se puso bien pálido." },
                new DialogueLine { speakerName = "GUSTAVO", text = "Sí, sí... es el calor." }
            };

            DialogueManager.Instance.PlayDialogueSequence(lines, () =>
            {
                if (GustavoController.Instance != null)
                {
                    GustavoController.Instance.SetAgitatedBreathing(true);
                }
                OnConversationFinished?.Invoke();
                onComplete?.Invoke();
            });
        }
    }
}

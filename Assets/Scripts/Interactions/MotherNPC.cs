using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Controlador del NPC de la Madre de Gustavo.
    /// Maneja los diálogos familiares en Hermosillo y su acompañamiento a la clínica psiquiátrica.
    /// </summary>
    public class MotherNPC : MonoBehaviour, IInteractable
    {
        [Header("Referencias")]
        [SerializeField] private PS2CharacterAnimator characterAnimator;
        [SerializeField] private Transform walkTargetDoor;

        [Header("Estado")]
        [SerializeField] private bool isInClinicScene = false;

        private bool hasSpokenHouseDialogue = false;
        private bool hasSpokenClinicDialogue = false;

        public void Interact(GameObject interactor)
        {
            if (!isInClinicScene && !hasSpokenHouseDialogue)
            {
                TriggerHouseDialogue();
            }
            else if (isInClinicScene && !hasSpokenClinicDialogue)
            {
                TriggerClinicDialogue();
            }
        }

        public void TriggerHouseDialogue(Action onComplete = null)
        {
            if (DialogueManager.Instance == null) return;
            hasSpokenHouseDialogue = true;

            var lines = new List<DialogueLine>
            {
                new DialogueLine { speakerName = "MAMÁ", text = "¿Cuántos días vas a estar fuera?" },
                new DialogueLine { speakerName = "GUSTAVO", text = "No sé, mamá. Depende de cómo salga el trabajo. Unos tres o cuatro días, máximo." },
                new DialogueLine { speakerName = "MAMÁ", text = "No me gusta que te vayas tan lejos." },
                new DialogueLine { speakerName = "GUSTAVO", text = "Ma, está a una hora y media. No me voy a otro país." },
                new DialogueLine { speakerName = "GUSTAVO", text = "Mira. Aquí te dejé lo que necesitas. Las pastillas de la mañana y las de la noche. No te las vayas a saltar." },
                new DialogueLine { speakerName = "MAMÁ", text = "No soy una niña, Gustavo." },
                new DialogueLine { speakerName = "GUSTAVO", text = "Ya sé, pero luego se te olvidan." },
                new DialogueLine { speakerName = "MAMÁ", text = "Eres igualito a tu papá cuando te pones así." }
            };

            DialogueManager.Instance.PlayDialogueSequence(lines, () =>
            {
                onComplete?.Invoke();
            });
        }

        public void TriggerClinicDialogue(Action onComplete = null)
        {
            if (DialogueManager.Instance == null) return;
            hasSpokenClinicDialogue = true;

            var lines = new List<DialogueLine>
            {
                new DialogueLine { speakerName = "GUSTAVO", text = "¿Traes todo?" },
                new DialogueLine { speakerName = "MAMÁ", text = "Sí." },
                new DialogueLine { speakerName = "GUSTAVO", text = "¿La receta?" },
                new DialogueLine { speakerName = "MAMÁ", text = "Sí, Gustavo." },
                new DialogueLine { speakerName = "MAMÁ", text = "No tienes que estar cuidándome todo el tiempo." },
                new DialogueLine { speakerName = "GUSTAVO", text = "Nos vemos al rato, mamá." },
                new DialogueLine { speakerName = "MAMÁ", text = "Cuídate mucho." }
            };

            DialogueManager.Instance.PlayDialogueSequence(lines, () =>
            {
                StartCoroutine(WalkIntoClinicRoutine(onComplete));
            });
        }

        private IEnumerator WalkIntoClinicRoutine(Action onComplete)
        {
            if (characterAnimator != null) characterAnimator.SetState(PS2CharacterAnimator.AnimationState.Walk);

            if (walkTargetDoor != null)
            {
                float timer = 0f;
                Vector3 startPos = transform.position;
                Vector3 endPos = walkTargetDoor.position;

                while (timer < 3.0f)
                {
                    timer += Time.deltaTime;
                    transform.position = Vector3.Lerp(startPos, endPos, timer / 3.0f);
                    transform.LookAt(new Vector3(endPos.x, transform.position.y, endPos.z));
                    yield return null;
                }
            }

            gameObject.SetActive(false); // Entra a la clínica
            onComplete?.Invoke();
        }
    }
}

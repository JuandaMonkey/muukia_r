using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MUUKIA
{
    /// <summary>
    /// Administrador de subtítulos y diálogos cinematográficos de MUUKIA.
    /// Soporta efecto máquina de escribir, avance manual con tecla o temporizador automático,
    /// sonidos de máquina de escribir y sincronización con cinemáticas.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI Referencias")]
        [SerializeField] private CanvasGroup dialogueCanvasGroup;
        [SerializeField] private Text speakerTextLegacy;
        [SerializeField] private TMP_Text speakerTextTMP;
        [SerializeField] private Text contentTextLegacy;
        [SerializeField] private TMP_Text contentTextTMP;
        [SerializeField] private GameObject promptAdvanceIndicator;

        [Header("Audio")]
        [SerializeField] private AudioSource dialogueAudioSource;
        [SerializeField] private AudioSource voiceAudioSource;
        [SerializeField] private AudioClip defaultTypewriterBlip;

        public bool IsDialogueActive { get; private set; }

        private Queue<DialogueLine> currentLineQueue = new Queue<DialogueLine>();
        private DialogueLine currentLine;
        private Coroutine typewriterCoroutine;
        private Action onSequenceCompletedCallback;
        private bool isTyping = false;
        private string fullTextTarget = "";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            HideDialogueBox();
        }

        private void Update()
        {
            if (!IsDialogueActive) return;

            // Avanzar con E, Espacio, o Clic Izquierdo
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    // Saltar de inmediato al texto completo
                    CompleteTypewriterInstantly();
                }
                else
                {
                    // Avanzar a la siguiente línea
                    DisplayNextLine();
                }
            }
        }

        public void PlayDialogueSequence(List<DialogueLine> lines, Action onComplete = null)
        {
            if (lines == null || lines.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            onSequenceCompletedCallback = onComplete;
            currentLineQueue.Clear();

            foreach (var line in lines)
            {
                currentLineQueue.Enqueue(line);
            }

            IsDialogueActive = true;
            ShowDialogueBox();
            DisplayNextLine();
        }

        public void PlaySingleLine(DialogueLine line, Action onComplete = null)
        {
            PlayDialogueSequence(new List<DialogueLine> { line }, onComplete);
        }

        private void DisplayNextLine()
        {
            if (currentLineQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            currentLine = currentLineQueue.Dequeue();
            fullTextTarget = currentLine.text;

            // Set speaker name
            string speakerDisplay = currentLine.isOffscreen ? $"{currentLine.speakerName} [FUERA DE PANTALLA]" : currentLine.speakerName;
            if (speakerTextTMP != null) speakerTextTMP.text = speakerDisplay;
            if (speakerTextLegacy != null) speakerTextLegacy.text = speakerDisplay;

            // Play voice or cue if present
            if (currentLine.voiceAudioClip != null && voiceAudioSource != null)
            {
                voiceAudioSource.clip = currentLine.voiceAudioClip;
                voiceAudioSource.Play();
            }

            if (promptAdvanceIndicator != null)
            {
                promptAdvanceIndicator.SetActive(false);
            }

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterRoutine(currentLine));
        }

        private IEnumerator TypewriterRoutine(DialogueLine line)
        {
            isTyping = true;
            SetContentText("");

            float speed = Mathf.Max(0.01f, line.typewriterSpeed);
            for (int i = 0; i <= line.text.Length; i++)
            {
                SetContentText(line.text.Substring(0, i));

                // Reproducir blip leve en caracteres alfabéticos
                if (i > 0 && i % 2 == 0 && dialogueAudioSource != null && defaultTypewriterBlip != null)
                {
                    dialogueAudioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                    dialogueAudioSource.PlayOneShot(defaultTypewriterBlip, 0.25f);
                }

                yield return new WaitForSeconds(speed);
            }

            isTyping = false;

            if (promptAdvanceIndicator != null && !line.autoAdvance)
            {
                promptAdvanceIndicator.SetActive(true);
            }

            if (line.autoAdvance)
            {
                yield return new WaitForSeconds(line.autoAdvanceDelay);
                DisplayNextLine();
            }
        }

        private void CompleteTypewriterInstantly()
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            isTyping = false;
            SetContentText(fullTextTarget);

            if (promptAdvanceIndicator != null && (currentLine == null || !currentLine.autoAdvance))
            {
                promptAdvanceIndicator.SetActive(true);
            }
        }

        private void SetContentText(string text)
        {
            if (contentTextTMP != null) contentTextTMP.text = text;
            if (contentTextLegacy != null) contentTextLegacy.text = text;
        }

        private void EndDialogue()
        {
            IsDialogueActive = false;
            HideDialogueBox();

            Action callback = onSequenceCompletedCallback;
            onSequenceCompletedCallback = null;
            callback?.Invoke();
        }

        private void ShowDialogueBox()
        {
            if (dialogueCanvasGroup != null)
            {
                dialogueCanvasGroup.alpha = 1f;
                dialogueCanvasGroup.blocksRaycasts = true;
                dialogueCanvasGroup.interactable = true;
            }
        }

        private void HideDialogueBox()
        {
            if (dialogueCanvasGroup != null)
            {
                dialogueCanvasGroup.alpha = 0f;
                dialogueCanvasGroup.blocksRaycasts = false;
                dialogueCanvasGroup.interactable = false;
            }
            if (promptAdvanceIndicator != null)
            {
                promptAdvanceIndicator.SetActive(false);
            }
        }
    }
}

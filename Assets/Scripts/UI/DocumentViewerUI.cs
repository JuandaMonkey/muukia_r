using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MUUKIA
{
    /// <summary>
    /// Visor de documentos en pantalla con estetica PS2 / Outlast.
    /// Permite leer informes, recortes de prensa, cartas y ver fotografias e inspeccionar evidencias.
    /// Congela los controles del jugador mientras se lee y los restaura al cerrar con E o Escape.
    /// </summary>
    public class DocumentViewerUI : MonoBehaviour
    {
        public static DocumentViewerUI Instance { get; private set; }

        [Header("Contenedores")]
        [SerializeField] private GameObject viewerRootPanel;
        [SerializeField] private Image documentImageDisplay;
        [SerializeField] private Text titleTextLegacy;
        [SerializeField] private TMP_Text titleTextTMP;
        [SerializeField] private Text dateTextLegacy;
        [SerializeField] private TMP_Text dateTextTMP;
        [SerializeField] private Text contentTextLegacy;
        [SerializeField] private TMP_Text contentTextTMP;
        [SerializeField] private Button closeButton;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip paperRustleClip;

        public bool IsReading { get; private set; }

        private PlayerMovement playerMovement;
        private PlayerLook playerLook;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (viewerRootPanel != null)
            {
                viewerRootPanel.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseDocument);
            }
        }

        private void Update()
        {
            if (!IsReading) return;

            // Cerrar con E o Escape
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseDocument();
            }
        }

        public void OpenDocument(DocumentItem doc)
        {
            if (doc == null) return;

            IsReading = true;
            if (viewerRootPanel != null) viewerRootPanel.SetActive(true);

            // Buscar componentes de jugador si no fueron asignados
            if (playerMovement == null) playerMovement = FindAnyObjectByType<PlayerMovement>();
            if (playerLook == null) playerLook = FindAnyObjectByType<PlayerLook>();

            if (playerMovement != null) playerMovement.enabled = false;
            if (playerLook != null) playerLook.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Asignar textos
            SetText(titleTextLegacy, titleTextTMP, doc.DocumentTitle);
            SetText(dateTextLegacy, dateTextTMP, doc.DateOrLocation);
            SetText(contentTextLegacy, contentTextTMP, doc.DocumentContent);

            if (documentImageDisplay != null)
            {
                if (doc.DocumentSprite != null)
                {
                    documentImageDisplay.gameObject.SetActive(true);
                    documentImageDisplay.sprite = doc.DocumentSprite;
                }
                else
                {
                    documentImageDisplay.gameObject.SetActive(false);
                }
            }

            PlaySound(paperRustleClip);

            // Si tiene audio de voz o cinta, reproducirlo
            if (doc.VoiceTapeClip != null && audioSource != null)
            {
                audioSource.clip = doc.VoiceTapeClip;
                audioSource.Play();
            }
        }

        public void CloseDocument()
        {
            if (!IsReading) return;

            IsReading = false;
            if (viewerRootPanel != null) viewerRootPanel.SetActive(false);

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            PlaySound(paperRustleClip);

            if (playerMovement != null) playerMovement.enabled = true;
            if (playerLook != null)
            {
                playerLook.enabled = true;
                playerLook.SetCursorState(true);
            }
        }

        private void SetText(Text legacy, TMP_Text tmp, string value)
        {
            if (legacy != null) legacy.text = value;
            if (tmp != null) tmp.text = value;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, 0.7f);
            }
        }
    }
}

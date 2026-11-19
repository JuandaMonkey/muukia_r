using System;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Objeto interactuable en el escenario que representa una evidencia narrativa
    /// (carta, informe policial, recorte de periodico, fotografia o cinta magnetica).
    /// </summary>
    public class DocumentItem : MonoBehaviour, IInteractable
    {
        [Header("Información Narrativa")]
        [SerializeField] private string documentTitle = "Documento";
        [SerializeField] private string dateOrLocation = "Sonora, 1989";
        [TextArea(5, 12)]
        [SerializeField] private string documentContent = "";

        [Header("Elemento Visual / Audio")]
        [SerializeField] private Sprite documentSprite;
        [SerializeField] private AudioClip voiceTapeClip;

        [Header("Progreso Narrativo")]
        [SerializeField] private bool triggersStoryProgression = false;
        [SerializeField] private string storyFlag = "";

        private InteractableOutline outline;

        public string DocumentTitle => documentTitle;
        public string DateOrLocation => dateOrLocation;
        public string DocumentContent => documentContent;
        public Sprite DocumentSprite => documentSprite;
        public AudioClip VoiceTapeClip => voiceTapeClip;

        private void Awake()
        {
            outline = GetComponent<InteractableOutline>();
        }

        public void Interact(GameObject interactor)
        {
            if (outline != null) outline.SetHighlighted(false);

            if (DocumentViewerUI.Instance != null)
            {
                DocumentViewerUI.Instance.OpenDocument(this);
            }

            if (triggersStoryProgression && !string.IsNullOrEmpty(storyFlag))
            {
                PlayerPrefs.SetInt("StoryFlag_" + storyFlag, 1);
                PlayerPrefs.Save();
            }
        }
    }
}

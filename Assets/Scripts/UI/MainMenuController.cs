using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace MUUKIA
{
    /// <summary>
    /// Controlador principal del Menu de Inicio de MUUKIA.
    /// Gestiona las transiciones a la primera escena jugable, panel de opciones,
    /// estado de partidas guardadas y audio atmosferico con estetica retro PS2.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Configuracion de Escena Jugable")]
        [SerializeField] private string firstPlayableSceneName = "CH01_HermosilloCasa";

        [Header("Botones Principales")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        [Header("Paneles de Interfaz")]
        [SerializeField] private GameObject mainButtonsContainer;
        [SerializeField] private OptionsMenuController optionsMenu;

        [Header("Textos Informativos")]
        [SerializeField] private TMP_Text statusMessageTMP;
        [SerializeField] private Text statusMessageLegacy;

        [Header("Audio Ambiental")]
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private AudioClip menuMusicClip;
        [SerializeField] private AudioClip buttonClickClip;

        private const string SAVE_STAGE_KEY = "MUUKIA_SavedStage";

        private void Start()
        {
            // Asegurar que el tiempo este normal y el cursor visible para el menu
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            InitializeButtons();
            CheckSaveGame();
            PlayMenuAmbience();
        }

        private void InitializeButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(OnOptionsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            // Seleccionar por defecto el boton JUGAR para navegacion por teclado / gamepad
            if (playButton != null)
            {
                playButton.Select();
            }
        }

        private void CheckSaveGame()
        {
            bool hasSave = PlayerPrefs.HasKey(SAVE_STAGE_KEY);

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;

                // Si no hay partida guardada, atenuar ligeramente el texto
                Text legacyText = continueButton.GetComponentInChildren<Text>();
                if (legacyText != null && !hasSave)
                {
                    Color c = legacyText.color;
                    c.a = 0.35f;
                    legacyText.color = c;
                }

                TMP_Text tmpText = continueButton.GetComponentInChildren<TMP_Text>();
                if (tmpText != null && !hasSave)
                {
                    Color c = tmpText.color;
                    c.a = 0.35f;
                    tmpText.color = c;
                }
            }
        }

        private void PlayMenuAmbience()
        {
            if (ambientAudioSource != null && menuMusicClip != null)
            {
                ambientAudioSource.clip = menuMusicClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0.5f;
                ambientAudioSource.Play();
            }
        }

        public void PlayButtonSound()
        {
            if (ambientAudioSource != null && buttonClickClip != null)
            {
                ambientAudioSource.PlayOneShot(buttonClickClip, 0.7f);
            }
        }

        public void OnPlayClicked()
        {
            PlayButtonSound();
            ShowStatusMessage("Cargando Capítulo 1: El Regreso...");
            StartCoroutine(LoadSceneRoutine(firstPlayableSceneName));
        }

        public void OnContinueClicked()
        {
            PlayButtonSound();
            if (PlayerPrefs.HasKey(SAVE_STAGE_KEY))
            {
                string targetScene = PlayerPrefs.GetString("MUUKIA_SavedScene", firstPlayableSceneName);
                ShowStatusMessage("Reanudando investigacion...");
                StartCoroutine(LoadSceneRoutine(targetScene));
            }
            else
            {
                ShowStatusMessage("No existe partida guardada.");
            }
        }

        public void OnOptionsClicked()
        {
            PlayButtonSound();
            if (optionsMenu != null)
            {
                optionsMenu.OpenOptions(mainButtonsContainer);
            }
            else
            {
                Debug.LogWarning("[MUUKIA MainMenu] OptionsMenuController no asignado.");
            }
        }

        public void OnQuitClicked()
        {
            PlayButtonSound();
            Debug.Log("[MUUKIA] Salir del juego presionado.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            // Deshabilitar botones para evitar dobles clics
            if (playButton != null) playButton.interactable = false;
            if (continueButton != null) continueButton.interactable = false;
            if (optionsButton != null) optionsButton.interactable = false;
            if (quitButton != null) quitButton.interactable = false;

            yield return new WaitForSeconds(0.4f);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private void ShowStatusMessage(string message)
        {
            if (statusMessageTMP != null)
            {
                statusMessageTMP.text = message;
                statusMessageTMP.gameObject.SetActive(true);
            }

            if (statusMessageLegacy != null)
            {
                statusMessageLegacy.text = message;
                statusMessageLegacy.gameObject.SetActive(true);
            }
        }
    }
}

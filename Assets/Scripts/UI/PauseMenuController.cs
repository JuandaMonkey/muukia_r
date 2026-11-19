using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MUUKIA
{
    /// <summary>
    /// Controlador del Menu de Pausa durante el gameplay.
    /// Interrumpe el flujo temporal con Time.timeScale = 0, libera el cursor para interaccion UI
    /// y ofrece opciones de Reanudar, Ajustes y Regreso al Menu Principal.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }
        public static bool IsPaused { get; private set; }

        [Header("Contenedores de Interfaz")]
        [SerializeField] private GameObject pauseRootPanel;
        [SerializeField] private GameObject pauseButtonsContainer;
        [SerializeField] private OptionsMenuController optionsMenu;

        [Header("Botones")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Escena del Menú Principal")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickClip;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (pauseRootPanel != null)
            {
                pauseRootPanel.SetActive(false);
            }

            IsPaused = false;
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(ResumeGame);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(OpenOptionsMenu);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Si el subpanel de opciones esta abierto, cerrarlo primero
                if (optionsMenu != null && optionsMenu.gameObject.activeSelf)
                {
                    optionsMenu.CloseOptions();
                    return;
                }

                if (IsPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (pauseRootPanel != null) pauseRootPanel.SetActive(true);
            if (pauseButtonsContainer != null) pauseButtonsContainer.SetActive(true);
            if (optionsMenu != null) optionsMenu.gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (resumeButton != null)
            {
                resumeButton.Select();
            }
        }

        public void ResumeGame()
        {
            PlayButtonSound();
            IsPaused = false;
            Time.timeScale = 1f;

            if (pauseRootPanel != null) pauseRootPanel.SetActive(false);
            if (optionsMenu != null) optionsMenu.gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OpenOptionsMenu()
        {
            PlayButtonSound();
            if (optionsMenu != null)
            {
                optionsMenu.OpenOptions(pauseButtonsContainer);
            }
        }

        public void ReturnToMainMenu()
        {
            PlayButtonSound();
            IsPaused = false;
            Time.timeScale = 1f;

            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void PlayButtonSound()
        {
            if (audioSource != null && buttonClickClip != null)
            {
                audioSource.PlayOneShot(buttonClickClip, 0.7f);
            }
        }

        private void OnDestroy()
        {
            // Asegurar que timeScale no quede congelado al destruir la escena
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
            IsPaused = false;
        }
    }
}

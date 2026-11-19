using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MUUKIA
{
    /// <summary>
    /// Controlador del menu de opciones para audio, resolucion y pantalla completa.
    /// Funciona de forma persistente mediante PlayerPrefs y afecta directamente los sistemas de audio y video.
    /// Compatible con UI estandar y TextMeshPro.
    /// </summary>
    public class OptionsMenuController : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Video Settings")]
        [SerializeField] private TMP_Dropdown resolutionDropdownTMP;
        [SerializeField] private Dropdown resolutionDropdownLegacy;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Botones")]
        [SerializeField] private Button backButton;

        [Header("Eventos de Cierre")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject previousPanel;

        public event Action OnBackRequested;

        private Resolution[] availableResolutions;
        private List<string> resolutionOptions = new List<string>();

        private const string PREF_MASTER_VOL = "MUUKIA_MasterVolume";
        private const string PREF_MUSIC_VOL = "MUUKIA_MusicVolume";
        private const string PREF_SFX_VOL = "MUUKIA_SFXVolume";
        private const string PREF_FULLSCREEN = "MUUKIA_Fullscreen";
        private const string PREF_RESOLUTION_INDEX = "MUUKIA_ResolutionIndex";

        private void Awake()
        {
            if (optionsPanel == null) optionsPanel = gameObject;

            InitializeResolutions();
            LoadSavedSettings();
            RegisterUIListeners();
        }

        private void InitializeResolutions()
        {
            availableResolutions = Screen.resolutions;
            resolutionOptions.Clear();

            int currentResolutionIndex = 0;

            if (availableResolutions != null && availableResolutions.Length > 0)
            {
                for (int i = 0; i < availableResolutions.Length; i++)
                {
                    string option = $"{availableResolutions[i].width}x{availableResolutions[i].height} @ {Mathf.RoundToInt((float)availableResolutions[i].refreshRateRatio.value)}Hz";
                    resolutionOptions.Add(option);

                    if (availableResolutions[i].width == Screen.currentResolution.width &&
                        availableResolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                    }
                }
            }
            else
            {
                // Opciones retro / estandar por defecto si Screen.resolutions esta vacio
                resolutionOptions.Add("1920x1080 (FHD)");
                resolutionOptions.Add("1280x720 (HD)");
                resolutionOptions.Add("854x480 (SD)");
                resolutionOptions.Add("640x480 (PS2 Retro)");
            }

            if (resolutionDropdownTMP != null)
            {
                resolutionDropdownTMP.ClearOptions();
                resolutionDropdownTMP.AddOptions(resolutionOptions);
            }

            if (resolutionDropdownLegacy != null)
            {
                resolutionDropdownLegacy.ClearOptions();
                resolutionDropdownLegacy.AddOptions(resolutionOptions);
            }
        }

        private void LoadSavedSettings()
        {
            float masterVol = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 1f);
            float musicVol = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 0.8f);
            float sfxVol = PlayerPrefs.GetFloat(PREF_SFX_VOL, 1f);
            bool isFullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
            int savedResIndex = PlayerPrefs.GetInt(PREF_RESOLUTION_INDEX, -1);

            if (masterVolumeSlider != null) masterVolumeSlider.value = masterVol;
            if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol;
            if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;

            AudioListener.volume = masterVol;
            Screen.fullScreen = isFullscreen;

            if (savedResIndex >= 0 && savedResIndex < resolutionOptions.Count)
            {
                SetResolutionIndexUI(savedResIndex);
            }
        }

        private void SetResolutionIndexUI(int index)
        {
            if (resolutionDropdownTMP != null) resolutionDropdownTMP.value = index;
            if (resolutionDropdownLegacy != null) resolutionDropdownLegacy.value = index;
        }

        private void RegisterUIListeners()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            if (resolutionDropdownTMP != null)
            {
                resolutionDropdownTMP.onValueChanged.AddListener(OnResolutionChanged);
            }

            if (resolutionDropdownLegacy != null)
            {
                resolutionDropdownLegacy.onValueChanged.AddListener(OnResolutionChanged);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }
        }

        public void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(PREF_MASTER_VOL, value);
            PlayerPrefs.Save();
        }

        public void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(PREF_MUSIC_VOL, value);
            PlayerPrefs.Save();
        }

        public void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(PREF_SFX_VOL, value);
            PlayerPrefs.Save();
        }

        public void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt(PREF_FULLSCREEN, isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void OnResolutionChanged(int resolutionIndex)
        {
            if (availableResolutions != null && resolutionIndex >= 0 && resolutionIndex < availableResolutions.Length)
            {
                Resolution resolution = availableResolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
                PlayerPrefs.SetInt(PREF_RESOLUTION_INDEX, resolutionIndex);
                PlayerPrefs.Save();
            }
            else
            {
                // Modo fallback
                switch (resolutionIndex)
                {
                    case 0: Screen.SetResolution(1920, 1080, Screen.fullScreen); break;
                    case 1: Screen.SetResolution(1280, 720, Screen.fullScreen); break;
                    case 2: Screen.SetResolution(854, 480, Screen.fullScreen); break;
                    case 3: Screen.SetResolution(640, 480, Screen.fullScreen); break;
                }
                PlayerPrefs.SetInt(PREF_RESOLUTION_INDEX, resolutionIndex);
                PlayerPrefs.Save();
            }
        }

        public void OnBackButtonClicked()
        {
            CloseOptions();
            OnBackRequested?.Invoke();
        }

        public void OpenOptions(GameObject callerPanel = null)
        {
            if (callerPanel != null) previousPanel = callerPanel;
            if (previousPanel != null) previousPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        public void CloseOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (previousPanel != null) previousPanel.SetActive(true);
        }
    }
}

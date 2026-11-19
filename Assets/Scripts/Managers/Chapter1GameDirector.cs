using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MUUKIA
{
    /// <summary>
    /// Director general de eventos y cinemáticas del Capítulo 1: El Regreso.
    /// Orquesta de forma impecable las 7 escenas requeridas, las transiciones visuales,
    /// diálogos, efectos analógicos VHS y el clímax final.
    /// </summary>
    public class Chapter1GameDirector : MonoBehaviour
    {
        public static Chapter1GameDirector Instance { get; private set; }

        public enum ChapterStage
        {
            Scene1_House_Hermosillo,
            Scene2_PsychiatricClinic,
            Scene3_Obregon_MontesDeOca,
            Scene4_NeighborConversation,
            Scene5_HighwayDriving,
            Scene6_MuukiaDesertChase,
            Scene7_Crypts_Finale,
            ChapterEnd
        }

        [Header("Etapa del Capítulo")]
        [SerializeField] private ChapterStage currentStage = ChapterStage.Scene1_House_Hermosillo;

        [Header("Transiciones Cinemáticas")]
        [SerializeField] private CanvasGroup screenFadeCanvasGroup;
        [SerializeField] private float defaultFadeDuration = 1.2f;
        [SerializeField] private CanvasGroup titleCardCanvasGroup;
        [SerializeField] private Text titleCardTextLegacy;
        [SerializeField] private TMP_Text titleCardTextTMP;

        [Header("Puntos de Teletransporte del Jugador")]
        [SerializeField] private Transform houseSpawnPoint;
        [SerializeField] private Transform clinicSpawnPoint;
        [SerializeField] private Transform obregonStreetSpawnPoint;
        [SerializeField] private Transform desertAccidentSpawnPoint;
        [SerializeField] private Transform cryptsSpawnPoint;

        [Header("Referencias de Personajes y Objetos")]
        [SerializeField] private MotherNPC motherNPC;
        [SerializeField] private NeighborNPC neighborNPC;
        [SerializeField] private MuukiaEntity muukiaEntity;
        [SerializeField] private CarController playerCar;
        [SerializeField] private Transform danzanteStatueTransform;
        [SerializeField] private SupernaturalTarget castleWindowTarget;
        [SerializeField] private SupernaturalTarget cryptOpeningTarget;
        [SerializeField] private GameObject castleSilhouetteObject;

        [Header("Audio Ambiental")]
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private AudioClip houseAmbienceClip;
        [SerializeField] private AudioClip urbanAmbienceClip;
        [SerializeField] private AudioClip desertWindClip;
        [SerializeField] private AudioClip cryptsDroneClip;
        [SerializeField] private AudioClip ramirezVoiceNoteClip;
        [SerializeField] private AudioClip sofiaWhisperClip;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            StartCoroutine(InitializeChapterRoutine());
        }

        private IEnumerator InitializeChapterRoutine()
        {
            SetScreenFade(1f);
            yield return new WaitForSeconds(0.3f);

            // Detectar automáticamente la etapa según el nombre de la escena activa
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene == SceneTransitionManager.SCENE_CH01_CASA)
            {
                StartScene1_House();
            }
            else if (activeScene == SceneTransitionManager.SCENE_CH01_CLINICA)
            {
                StartScene2_Clinic();
            }
            else if (activeScene == SceneTransitionManager.SCENE_CH01_OBREGON)
            {
                StartScene3_Obregon();
            }
            else if (activeScene == SceneTransitionManager.SCENE_CH01_CARRETERA)
            {
                StartScene5_HighwayDriving();
            }
            else if (activeScene == SceneTransitionManager.SCENE_CH01_CRIPTAS)
            {
                StartScene7_Crypts();
            }
            else
            {
                // Fallback por defecto según currentStage
                switch (currentStage)
                {
                    case ChapterStage.Scene1_House_Hermosillo: StartScene1_House(); break;
                    case ChapterStage.Scene2_PsychiatricClinic: StartScene2_Clinic(); break;
                    case ChapterStage.Scene3_Obregon_MontesDeOca: StartScene3_Obregon(); break;
                    case ChapterStage.Scene4_NeighborConversation: StartScene4_Neighbor(); break;
                    case ChapterStage.Scene5_HighwayDriving: StartScene5_HighwayDriving(); break;
                    case ChapterStage.Scene6_MuukiaDesertChase: StartScene6_DesertChase(); break;
                    case ChapterStage.Scene7_Crypts_Finale: StartScene7_Crypts(); break;
                }
            }

            yield return FadeScreen(0f, 1.2f);
        }

        #region ESCENA 1: CASA DE GUSTAVO EN HERMOSILLO

        public void StartScene1_House()
        {
            currentStage = ChapterStage.Scene1_House_Hermosillo;
            RetroFogController.Instance?.TransitionToHouse();

            if (ambientAudioSource != null && houseAmbienceClip != null)
            {
                ambientAudioSource.clip = houseAmbienceClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0.4f;
                ambientAudioSource.Play();
            }

            if (houseSpawnPoint != null && GustavoController.Instance != null)
            {
                SpawnPoint sp = houseSpawnPoint.GetComponent<SpawnPoint>();
                if (sp != null)
                {
                    sp.Spawn(GustavoController.Instance.transform);
                }
                else
                {
                    GustavoController.Instance.TeleportTo(houseSpawnPoint.position + Vector3.up * 0.08f, houseSpawnPoint.rotation);
                }
            }
        }

        /// <summary>
        /// Llamado cuando Gustavo interactúa con las pastillas o la puerta para iniciar la conversación con su madre.
        /// </summary>
        public void OnHousePillsOrDoorInteracted()
        {
            if (currentStage != ChapterStage.Scene1_House_Hermosillo) return;

            if (motherNPC != null)
            {
                motherNPC.TriggerHouseDialogue(() =>
                {
                    // Transición a la Escena 2 (Clínica)
                    StartCoroutine(TransitionToScene2Routine());
                });
            }
        }

        private IEnumerator TransitionToScene2Routine()
        {
            yield return new WaitForSeconds(1.0f);

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneTransitionManager.SCENE_CH01_CASA)
            {
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.TransitionToScene(SceneTransitionManager.SCENE_CH01_CLINICA);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneTransitionManager.SCENE_CH01_CLINICA);
                }
                yield break;
            }

            yield return FadeScreen(1f, defaultFadeDuration);

            StartScene2_Clinic();
            yield return FadeScreen(0f, defaultFadeDuration);
        }

        #endregion

        #region ESCENA 2: CLÍNICA PSIQUIÁTRICA

        public void StartScene2_Clinic()
        {
            currentStage = ChapterStage.Scene2_PsychiatricClinic;
            RetroFogController.Instance?.TransitionToClinic();

            if (clinicSpawnPoint != null && GustavoController.Instance != null)
            {
                SpawnPoint sp = clinicSpawnPoint.GetComponent<SpawnPoint>();
                if (sp != null)
                {
                    sp.Spawn(GustavoController.Instance.transform);
                }
                else
                {
                    GustavoController.Instance.TeleportTo(clinicSpawnPoint.position + Vector3.up * 0.08f, clinicSpawnPoint.rotation);
                }
            }

            // Iniciar diálogo frente a la clínica
            if (motherNPC != null)
            {
                motherNPC.gameObject.SetActive(true);
                motherNPC.TriggerClinicDialogue(() =>
                {
                    // Reproducir nota de voz del jefe Ramírez
                    StartCoroutine(PlayBossVoiceNoteRoutine());
                });
            }
        }

        private IEnumerator PlayBossVoiceNoteRoutine()
        {
            yield return new WaitForSeconds(1.0f);

            var lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    speakerName = "RAMÍREZ (NOTA DE VOZ)",
                    text = "Gustavo, llegando a Obregón te vas a ir directamente a Fernando Montes de Oca. Nos reportaron un asesinato esta mañana. Ya sabes cómo está el asunto, necesito que llegues antes que los demás medios. Te paso la ubicación por mensaje.",
                    voiceAudioClip = ramirezVoiceNoteClip,
                    isOffscreen = true,
                    typewriterSpeed = 0.03f
                },
                new DialogueLine
                {
                    speakerName = "GUSTAVO (NOTA DE VOZ)",
                    text = "Va, jefe. Ya voy entrando.",
                    isOffscreen = true,
                    typewriterSpeed = 0.03f
                }
            };

            DialogueManager.Instance?.PlayDialogueSequence(lines, () =>
            {
                StartCoroutine(TransitionToScene3Routine());
            });
        }

        private IEnumerator TransitionToScene3Routine()
        {
            yield return new WaitForSeconds(1.0f);

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneTransitionManager.SCENE_CH01_CLINICA)
            {
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.TransitionToScene(SceneTransitionManager.SCENE_CH01_OBREGON);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneTransitionManager.SCENE_CH01_OBREGON);
                }
                yield break;
            }

            yield return FadeScreen(1f, 1.8f);

            StartScene3_Obregon();
            yield return FadeScreen(0f, 1.5f);
        }

        #endregion

        #region ESCENA 3: CIUDAD OBREGÓN — FERNANDO MONTES DE OCA

        public void StartScene3_Obregon()
        {
            currentStage = ChapterStage.Scene3_Obregon_MontesDeOca;
            RetroFogController.Instance?.TransitionToObregon();

            if (ambientAudioSource != null && urbanAmbienceClip != null)
            {
                ambientAudioSource.clip = urbanAmbienceClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0.5f;
                ambientAudioSource.Play();
            }

            if (obregonStreetSpawnPoint != null && GustavoController.Instance != null)
            {
                SpawnPoint sp = obregonStreetSpawnPoint.GetComponent<SpawnPoint>();
                if (sp != null)
                {
                    sp.Spawn(GustavoController.Instance.transform);
                }
                else
                {
                    GustavoController.Instance.TeleportTo(obregonStreetSpawnPoint.position + Vector3.up * 0.08f, obregonStreetSpawnPoint.rotation);
                }
            }

            // Auto-equipar videocámara para el reportaje
            VideoCameraRig.Instance?.SetCameraEquipped(true);

            // Diálogo de reportaje a cámara
            var reportLines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    speakerName = "GUSTAVO (A CÁMARA)",
                    text = "Durante las primeras horas de este día, autoridades municipales acudieron a un domicilio ubicado sobre la calle Fernando Montes de Oca, donde se reportó el asesinato de una persona. Hasta el momento, las autoridades no han revelado detalles sobre la identidad de la víctima ni sobre las circunstancias en las que ocurrió el hecho.",
                    typewriterSpeed = 0.035f
                }
            };

            DialogueManager.Instance?.PlayDialogueSequence(reportLines, () =>
            {
                StartCoroutine(CastleEncounterRoutine());
            });
        }

        private IEnumerator CastleEncounterRoutine()
        {
            yield return new WaitForSeconds(0.6f);

            // Comienza a escucharse el sonido característico de los tenábaris de MUUKIA
            if (muukiaEntity != null)
            {
                muukiaEntity.PlayTenabarisSound(true, 0.6f);
            }

            // Silueta visible en la ventana del castillo
            if (castleSilhouetteObject != null)
            {
                castleSilhouetteObject.SetActive(true);
            }

            if (castleWindowTarget != null)
            {
                castleWindowTarget.SetActiveState(true);
            }

            // Notificación al jugador
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlaySingleLine(new DialogueLine
                {
                    speakerName = "GUSTAVO",
                    text = "¿Qué es ese sonido...? Viene de esa casa...",
                    typewriterSpeed = 0.03f,
                    autoAdvance = true,
                    autoAdvanceDelay = 2.0f
                });
            }
        }

        /// <summary>
        /// Llamado cuando Gustavo enfoca con su cámara hacia la ventana del castillo.
        /// </summary>
        public void OnCastleWindowObserved()
        {
            if (currentStage != ChapterStage.Scene3_Obregon_MontesDeOca) return;

            StartCoroutine(CastleWindowGlitchRoutine());
        }

        private IEnumerator CastleWindowGlitchRoutine()
        {
            // Glitch analógico fuerte
            if (VHSCameraEffect.Instance != null)
            {
                VHSCameraEffect.Instance.TriggerGlitchSpike(0.9f, 1.2f);
            }

            yield return new WaitForSeconds(0.8f);

            // La silueta desaparece cuando vuelve a mirar directamente
            if (castleSilhouetteObject != null)
            {
                castleSilhouetteObject.SetActive(false);
            }

            if (muukiaEntity != null)
            {
                muukiaEntity.StopTenabarisSound();
            }

            var lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    speakerName = "GUSTAVO",
                    text = "¿Qué chingados...?",
                    typewriterSpeed = 0.03f
                }
            };

            DialogueManager.Instance?.PlayDialogueSequence(lines, () =>
            {
                // El lugar se despeja y se da paso a la conversación con el vecino
                StartScene4_Neighbor();
            });
        }

        #endregion

        #region ESCENA 4: CONVERSACIÓN CON EL VECINO

        public void StartScene4_Neighbor()
        {
            currentStage = ChapterStage.Scene4_NeighborConversation;

            if (neighborNPC != null)
            {
                neighborNPC.gameObject.SetActive(true);
                neighborNPC.OnConversationFinished += OnNeighborConversationDone;
            }
        }

        private void OnNeighborConversationDone()
        {
            if (neighborNPC != null) neighborNPC.OnConversationFinished -= OnNeighborConversationDone;

            // Transición a la secuencia de conducción en la carretera
            StartCoroutine(TransitionToScene5Routine());
        }

        private IEnumerator TransitionToScene5Routine()
        {
            yield return new WaitForSeconds(1.5f);

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneTransitionManager.SCENE_CH01_OBREGON)
            {
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.TransitionToScene(SceneTransitionManager.SCENE_CH01_CARRETERA);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneTransitionManager.SCENE_CH01_CARRETERA);
                }
                yield break;
            }

            yield return FadeScreen(1f, defaultFadeDuration);

            StartScene5_HighwayDriving();
            yield return FadeScreen(0f, defaultFadeDuration);
        }

        #endregion

        #region ESCENA 5: CARRETERA DESÉRTICA Y ACCIDENTE

        public void StartScene5_HighwayDriving()
        {
            currentStage = ChapterStage.Scene5_HighwayDriving;
            RetroFogController.Instance?.TransitionToHighway();

            if (ambientAudioSource != null && desertWindClip != null)
            {
                ambientAudioSource.clip = desertWindClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0.6f;
                ambientAudioSource.Play();
            }

            // Ocultar cámara de video para conducir
            VideoCameraRig.Instance?.SetCameraEquipped(false);

            if (playerCar != null)
            {
                playerCar.gameObject.SetActive(true);
                playerCar.StartDrivingSequence(() =>
                {
                    // Cuando sale del auto tras el accidente
                    StartScene6_DesertChase();
                });
            }

            // Programar el colapso del monumento Danzante Yaqui tras unos segundos de conducción
            StartCoroutine(ScheduleDanzanteAccidentRoutine());
        }

        private IEnumerator ScheduleDanzanteAccidentRoutine()
        {
            yield return new WaitForSeconds(4.5f);

            // Distorsión de la realidad: el sonido ambiental desaparece
            if (ambientAudioSource != null)
            {
                ambientAudioSource.volume = 0.05f;
            }

            if (playerCar != null && danzanteStatueTransform != null)
            {
                playerCar.TriggerDanzanteAccident(danzanteStatueTransform, () =>
                {
                    // Espera a que el jugador salga del auto
                });
            }
        }

        #endregion

        #region ESCENA 6: PRIMER ENCUENTRO CON MUUKIA EN EL DESIERTO

        public void StartScene6_DesertChase()
        {
            currentStage = ChapterStage.Scene6_MuukiaDesertChase;

            if (ambientAudioSource != null && desertWindClip != null)
            {
                ambientAudioSource.volume = 0.5f;
                ambientAudioSource.Play();
            }

            var lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    speakerName = "GUSTAVO",
                    text = "¿Quién...?",
                    typewriterSpeed = 0.03f
                }
            };

            DialogueManager.Instance?.PlayDialogueSequence(lines, () =>
            {
                if (muukiaEntity != null)
                {
                    muukiaEntity.StartDesertSequence();
                }
            });
        }

        public void NotifyPlayerArrivedAtCrypts()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneTransitionManager.SCENE_CH01_CARRETERA)
            {
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.TransitionToScene(SceneTransitionManager.SCENE_CH01_CRIPTAS);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneTransitionManager.SCENE_CH01_CRIPTAS);
                }
                return;
            }

            StartScene7_Crypts();
        }

        #endregion

        #region ESCENA 7: LAS CRIPTAS — FINAL DEL CAPÍTULO

        public void StartScene7_Crypts()
        {
            currentStage = ChapterStage.Scene7_Crypts_Finale;
            RetroFogController.Instance?.TransitionToCrypts();

            if (ambientAudioSource != null && cryptsDroneClip != null)
            {
                ambientAudioSource.clip = cryptsDroneClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0.65f;
                ambientAudioSource.Play();
            }

            if (cryptOpeningTarget != null)
            {
                cryptOpeningTarget.SetActiveState(true);
            }
        }

        /// <summary>
        /// Llamado cuando Gustavo se acerca o enfoca con la cámara a la abertura derrumbada de la cripta.
        /// </summary>
        public void OnCryptOpeningReached()
        {
            if (currentStage != ChapterStage.Scene7_Crypts_Finale) return;

            StartCoroutine(CryptsFinaleClimaxRoutine());
        }

        private IEnumerator CryptsFinaleClimaxRoutine()
        {
            // Bloquear jugador para la cinemática final
            if (GustavoController.Instance != null)
            {
                GustavoController.Instance.LockPlayerForCutscene(true, false);
            }

            var lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    speakerName = "GUSTAVO",
                    text = "¿Qué chingados es esto...?",
                    typewriterSpeed = 0.035f
                },
                new DialogueLine
                {
                    speakerName = "SOFI (VOZ)",
                    text = "Gus...",
                    voiceAudioClip = sofiaWhisperClip,
                    isOffscreen = true,
                    typewriterSpeed = 0.04f
                },
                new DialogueLine
                {
                    speakerName = "GUSTAVO",
                    text = "¿Sofi?",
                    typewriterSpeed = 0.03f
                },
                new DialogueLine
                {
                    speakerName = "SOFI (VOZ)",
                    text = "¿Me vas a dejar sola otra vez?",
                    voiceAudioClip = sofiaWhisperClip,
                    isOffscreen = true,
                    typewriterSpeed = 0.035f
                }
            };

            DialogueManager.Instance?.PlayDialogueSequence(lines, () =>
            {
                StartCoroutine(JumpscareAndCutToBlackRoutine());
            });

            yield return null;
        }

        private IEnumerator JumpscareAndCutToBlackRoutine()
        {
            // Interferencia analógica extrema en la videocámara
            if (VHSCameraEffect.Instance != null)
            {
                VHSCameraEffect.Instance.TriggerGlitchSpike(1.0f, 2.0f);
            }

            // Spawn de MUUKIA justo detrás de Gustavo
            if (muukiaEntity != null && GustavoController.Instance != null)
            {
                muukiaEntity.SpawnBehindGustavo(GustavoController.Instance.transform.position, GustavoController.Instance.transform.forward);
            }

            yield return new WaitForSeconds(0.45f);

            // CORTE INMEDIATO A NEGRO
            SetScreenFade(1f);
            if (ambientAudioSource != null) ambientAudioSource.Stop();
            if (muukiaEntity != null) muukiaEntity.StopTenabarisSound();

            yield return new WaitForSeconds(1.0f);

            // MOSTRAR TÍTULO FINAL
            currentStage = ChapterStage.ChapterEnd;
            ShowEndTitleCard("MUUKIA\n\nCAPÍTULO 1: EL REGRESO\n\nFIN DEL CAPÍTULO 1");

            yield return new WaitForSeconds(5.0f);
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(SceneTransitionManager.SCENE_MAIN_MENU);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneTransitionManager.SCENE_MAIN_MENU);
            }
        }

        #endregion

        #region Helpers de Pantalla y Fade

        private void SetScreenFade(float alpha)
        {
            if (screenFadeCanvasGroup != null)
            {
                screenFadeCanvasGroup.alpha = alpha;
                screenFadeCanvasGroup.blocksRaycasts = alpha > 0.01f;
            }
        }

        private IEnumerator FadeScreen(float targetAlpha, float duration)
        {
            if (screenFadeCanvasGroup == null) yield break;

            float startAlpha = screenFadeCanvasGroup.alpha;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                screenFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                yield return null;
            }

            screenFadeCanvasGroup.alpha = targetAlpha;
            screenFadeCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
        }

        private void ShowEndTitleCard(string text)
        {
            if (titleCardTextTMP != null) titleCardTextTMP.text = text;
            if (titleCardTextLegacy != null) titleCardTextLegacy.text = text;

            if (titleCardCanvasGroup != null)
            {
                titleCardCanvasGroup.alpha = 1f;
            }
        }

        #endregion
    }
}

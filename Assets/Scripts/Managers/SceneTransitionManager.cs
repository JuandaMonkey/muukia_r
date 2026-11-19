using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MUUKIA
{
    /// <summary>
    /// Administrador de transiciones entre escenas del flujo narrativo de MUUKIA.
    /// Ejecuta el fundido a negro (fade out), bloquea controles del jugador,
    /// carga la siguiente escena de forma asíncrona, posiciona al jugador en el SpawnPoint,
    /// y realiza el fundido de entrada (fade in).
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Referencias Visuales")]
        [SerializeField] private CanvasGroup screenFadeCanvasGroup;
        [SerializeField] private float defaultFadeDuration = 1.0f;

        [Header("Configuración de Flujo")]
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_CH01_CASA = "CH01_HermosilloCasa";
        public const string SCENE_CH01_CLINICA = "CH01_Clinica";
        public const string SCENE_CH01_OBREGON = "CH01_Obregon";
        public const string SCENE_CH01_CARRETERA = "CH01_Carretera";
        public const string SCENE_CH01_CRIPTAS = "CH01_Criptas";

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsureFadeOverlay();
        }

        private void EnsureFadeOverlay()
        {
            if (screenFadeCanvasGroup == null)
            {
                Canvas canvas = GetComponentInChildren<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasGO = new GameObject("TransitionCanvas");
                    canvasGO.transform.SetParent(transform);
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 999;
                    canvasGO.AddComponent<CanvasScaler>();
                    canvasGO.AddComponent<GraphicRaycaster>();
                }

                GameObject fadeObj = new GameObject("ScreenFadeOverlay");
                fadeObj.transform.SetParent(canvas.transform, false);
                RectTransform rt = fadeObj.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image img = fadeObj.AddComponent<Image>();
                img.color = Color.black;

                screenFadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
                screenFadeCanvasGroup.alpha = 0f;
                screenFadeCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Inicia la transición suave a la escena destino.
        /// </summary>
        public void TransitionToScene(string targetSceneName, float fadeDuration = -1f)
        {
            if (IsTransitioning) return;
            float dur = fadeDuration > 0f ? fadeDuration : defaultFadeDuration;
            StartCoroutine(TransitionRoutine(targetSceneName, dur));
        }

        private IEnumerator TransitionRoutine(string sceneName, float duration)
        {
            IsTransitioning = true;

            // 1. Desactivar controles del jugador durante la transición
            if (GustavoController.Instance != null)
            {
                GustavoController.Instance.LockPlayerForCutscene(true);
            }

            // 2. Fundido a negro (Fade Out)
            yield return Fade(1f, duration);

            // 3. Cargar la nueva escena
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            // 4. Ubicar al jugador en el SpawnPoint de la escena cargada
            SpawnPoint spawn = FindAnyObjectByType<SpawnPoint>();
            if (spawn != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    spawn.Spawn(player.transform);
                }
            }

            // 5. Fundido de entrada (Fade In)
            yield return Fade(0f, duration);

            // 6. Reactivar controles si no es menú principal
            if (sceneName != SCENE_MAIN_MENU && GustavoController.Instance != null)
            {
                GustavoController.Instance.LockPlayerForCutscene(false);
            }

            IsTransitioning = false;
        }

        public IEnumerator Fade(float targetAlpha, float duration)
        {
            EnsureFadeOverlay();
            if (screenFadeCanvasGroup == null) yield break;

            screenFadeCanvasGroup.blocksRaycasts = (targetAlpha > 0.05f);
            float startAlpha = screenFadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                screenFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            screenFadeCanvasGroup.alpha = targetAlpha;
            screenFadeCanvasGroup.blocksRaycasts = (targetAlpha > 0.05f);
        }

        /// <summary>
        /// Determina la escena siguiente en el flujo canónico del Capítulo 1.
        /// </summary>
        public string GetNextSceneName(string currentSceneName)
        {
            switch (currentSceneName)
            {
                case SCENE_MAIN_MENU: return SCENE_CH01_CASA;
                case SCENE_CH01_CASA: return SCENE_CH01_CLINICA;
                case SCENE_CH01_CLINICA: return SCENE_CH01_OBREGON;
                case SCENE_CH01_OBREGON: return SCENE_CH01_CARRETERA;
                case SCENE_CH01_CARRETERA: return SCENE_CH01_CRIPTAS;
                case SCENE_CH01_CRIPTAS: return SCENE_MAIN_MENU;
                default: return SCENE_MAIN_MENU;
            }
        }
    }
}

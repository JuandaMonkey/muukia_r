using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MUUKIA
{
    /// <summary>
    /// Controlador de distorsión analógica VHS, ruido de cinta y estética PS2.
    /// Modula parámetros del shader y activa ráfagas de interferencia durante eventos sobrenaturales.
    /// </summary>
    public class VHSCameraEffect : MonoBehaviour
    {
        public static VHSCameraEffect Instance { get; private set; }

        [Header("Referencias Visuales")]
        [SerializeField] private Material vhsMaterial;
        [SerializeField] private CanvasGroup vhsCanvasGroup;
        [SerializeField] private GameObject cameraHUDContainer;

        [Header("Configuración Base")]
        [SerializeField] [Range(0f, 1f)] private float baseDistortion = 0.05f;
        [SerializeField] [Range(0f, 1f)] private float baseScanlines = 0.35f;
        [SerializeField] [Range(0f, 1f)] private float baseVignette = 0.45f;

        [Header("Audio")]
        [SerializeField] private AudioSource vhsAudioSource;
        [SerializeField] private AudioClip glitchAudioClip;
        [SerializeField] private AudioClip staticHumClip;

        private float currentGlitch = 0f;
        private Coroutine glitchCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            if (vhsMaterial != null)
            {
                vhsMaterial.SetFloat("_DistortionIntensity", baseDistortion);
                vhsMaterial.SetFloat("_ScanlineIntensity", baseScanlines);
                vhsMaterial.SetFloat("_Vignette", baseVignette);
                vhsMaterial.SetFloat("_GlitchSpike", 0f);
            }
        }

        private void Start()
        {
            if (vhsAudioSource != null && staticHumClip != null)
            {
                vhsAudioSource.clip = staticHumClip;
                vhsAudioSource.loop = true;
                vhsAudioSource.volume = 0.15f;
                vhsAudioSource.Play();
            }
        }

        public void SetCameraMode(bool enabled)
        {
            if (cameraHUDContainer != null)
            {
                cameraHUDContainer.SetActive(enabled);
            }

            if (vhsMaterial != null)
            {
                float targetDist = enabled ? 0.12f : baseDistortion;
                float targetScan = enabled ? 0.55f : baseScanlines;
                vhsMaterial.SetFloat("_DistortionIntensity", targetDist);
                vhsMaterial.SetFloat("_ScanlineIntensity", targetScan);
            }

            if (vhsAudioSource != null)
            {
                vhsAudioSource.volume = enabled ? 0.35f : 0.12f;
            }
        }

        public void TriggerGlitchSpike(float intensity = 0.8f, float duration = 1.0f)
        {
            if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
            glitchCoroutine = StartCoroutine(GlitchRoutine(intensity, duration));
        }

        private IEnumerator GlitchRoutine(float intensity, float duration)
        {
            if (vhsAudioSource != null && glitchAudioClip != null)
            {
                vhsAudioSource.PlayOneShot(glitchAudioClip, 0.9f);
            }

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float factor = 1f - (timer / duration);
                currentGlitch = Mathf.Lerp(intensity, 0f, 1f - factor);

                if (vhsMaterial != null)
                {
                    vhsMaterial.SetFloat("_GlitchSpike", currentGlitch);
                    vhsMaterial.SetFloat("_DistortionIntensity", baseDistortion + currentGlitch * 0.4f);
                }

                yield return null;
            }

            currentGlitch = 0f;
            if (vhsMaterial != null)
            {
                vhsMaterial.SetFloat("_GlitchSpike", 0f);
                vhsMaterial.SetFloat("_DistortionIntensity", baseDistortion);
            }
        }

        public void SetDistortionLevel(float distortion)
        {
            if (vhsMaterial != null)
            {
                vhsMaterial.SetFloat("_DistortionIntensity", Mathf.Max(baseDistortion, distortion));
            }
        }
    }
}

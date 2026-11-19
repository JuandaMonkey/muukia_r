using System.Collections;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Gestiona la niebla atmosférica y la paleta de iluminación ambiental
    /// adaptada al estilo retro de terror psicológico de PlayStation 2.
    /// </summary>
    public class RetroFogController : MonoBehaviour
    {
        public static RetroFogController Instance { get; private set; }

        [System.Serializable]
        public struct EnvironmentMood
        {
            public string moodName;
            public Color fogColor;
            public float fogDensity;
            public Color ambientSkyColor;
            public Color ambientEquatorColor;
            public Color ambientGroundColor;
            public float ambientIntensity;
        }

        [Header("Perfiles Atmosféricos")]
        [SerializeField] private EnvironmentMood houseMood = new EnvironmentMood
        {
            moodName = "Casa Hermosillo",
            fogColor = new Color(0.18f, 0.16f, 0.14f, 1f),
            fogDensity = 0.008f,
            ambientSkyColor = new Color(0.35f, 0.32f, 0.28f, 1f),
            ambientEquatorColor = new Color(0.25f, 0.22f, 0.20f, 1f),
            ambientGroundColor = new Color(0.12f, 0.10f, 0.08f, 1f),
            ambientIntensity = 0.85f
        };

        [SerializeField] private EnvironmentMood clinicMood = new EnvironmentMood
        {
            moodName = "Clínica Psiquiátrica",
            fogColor = new Color(0.25f, 0.27f, 0.28f, 1f),
            fogDensity = 0.012f,
            ambientSkyColor = new Color(0.40f, 0.42f, 0.45f, 1f),
            ambientEquatorColor = new Color(0.28f, 0.30f, 0.32f, 1f),
            ambientGroundColor = new Color(0.15f, 0.15f, 0.16f, 1f),
            ambientIntensity = 0.9f
        };

        [SerializeField] private EnvironmentMood obregonStreetMood = new EnvironmentMood
        {
            moodName = "Obregón - Montes de Oca",
            fogColor = new Color(0.30f, 0.27f, 0.22f, 1f), // Tono desértico cálido caluroso
            fogDensity = 0.014f,
            ambientSkyColor = new Color(0.45f, 0.40f, 0.32f, 1f),
            ambientEquatorColor = new Color(0.30f, 0.26f, 0.22f, 1f),
            ambientGroundColor = new Color(0.18f, 0.14f, 0.10f, 1f),
            ambientIntensity = 0.95f
        };

        [SerializeField] private EnvironmentMood highwayMood = new EnvironmentMood
        {
            moodName = "Carretera Desértica",
            fogColor = new Color(0.20f, 0.16f, 0.14f, 1f),
            fogDensity = 0.020f,
            ambientSkyColor = new Color(0.30f, 0.22f, 0.18f, 1f),
            ambientEquatorColor = new Color(0.18f, 0.14f, 0.12f, 1f),
            ambientGroundColor = new Color(0.08f, 0.06f, 0.05f, 1f),
            ambientIntensity = 0.65f
        };

        [SerializeField] private EnvironmentMood cryptsMood = new EnvironmentMood
        {
            moodName = "Las Criptas",
            fogColor = new Color(0.04f, 0.05f, 0.08f, 1f), // Frío azul oscuro ps2
            fogDensity = 0.045f, // Niebla densa Silent Hill / Fatal Frame
            ambientSkyColor = new Color(0.10f, 0.11f, 0.16f, 1f),
            ambientEquatorColor = new Color(0.05f, 0.06f, 0.09f, 1f),
            ambientGroundColor = new Color(0.02f, 0.02f, 0.03f, 1f),
            ambientIntensity = 0.40f
        };

        private Coroutine transitionCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            ApplyMoodInstant(houseMood);
        }

        public void TransitionToHouse() => StartTransition(houseMood);
        public void TransitionToClinic() => StartTransition(clinicMood);
        public void TransitionToObregon() => StartTransition(obregonStreetMood);
        public void TransitionToHighway() => StartTransition(highwayMood);
        public void TransitionToCrypts() => StartTransition(cryptsMood);

        public void StartTransition(EnvironmentMood targetMood, float duration = 2.5f)
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(TransitionRoutine(targetMood, duration));
        }

        private IEnumerator TransitionRoutine(EnvironmentMood target, float duration)
        {
            Color startFogCol = RenderSettings.fogColor;
            float startFogDensity = RenderSettings.fogDensity;
            Color startSky = RenderSettings.ambientSkyColor;
            Color startEquator = RenderSettings.ambientEquatorColor;
            Color startGround = RenderSettings.ambientGroundColor;
            float startIntensity = RenderSettings.ambientIntensity;

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                RenderSettings.fogColor = Color.Lerp(startFogCol, target.fogColor, t);
                RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, target.fogDensity, t);
                RenderSettings.ambientSkyColor = Color.Lerp(startSky, target.ambientSkyColor, t);
                RenderSettings.ambientEquatorColor = Color.Lerp(startEquator, target.ambientEquatorColor, t);
                RenderSettings.ambientGroundColor = Color.Lerp(startGround, target.ambientGroundColor, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(startIntensity, target.ambientIntensity, t);

                yield return null;
            }

            ApplyMoodInstant(target);
        }

        private void ApplyMoodInstant(EnvironmentMood mood)
        {
            RenderSettings.fogColor = mood.fogColor;
            RenderSettings.fogDensity = mood.fogDensity;
            RenderSettings.ambientSkyColor = mood.ambientSkyColor;
            RenderSettings.ambientEquatorColor = mood.ambientEquatorColor;
            RenderSettings.ambientGroundColor = mood.ambientGroundColor;
            RenderSettings.ambientIntensity = mood.ambientIntensity;
        }
    }
}

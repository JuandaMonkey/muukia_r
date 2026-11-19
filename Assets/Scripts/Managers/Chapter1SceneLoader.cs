using UnityEngine;
using UnityEngine.SceneManagement;

namespace MUUKIA
{
    /// <summary>
    /// Cargador de transición para iniciar el Capítulo 1: El Regreso desde cualquier escena.
    /// </summary>
    public class Chapter1SceneLoader : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "CH01_HermosilloCasa";
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private float delayBeforeLoad = 0.2f;

        private void Start()
        {
            if (autoLoadOnStart)
            {
                Invoke(nameof(LoadChapter), delayBeforeLoad);
            }
        }

        public void LoadChapter()
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}

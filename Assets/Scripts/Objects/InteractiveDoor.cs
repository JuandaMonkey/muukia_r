using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MUUKIA
{
    /// <summary>
    /// Puerta interactiva con bisagra y giro angular suave.
    /// Soporta apertura y cierre interactivo, estado de bloqueo con llave,
    /// y transición fluida a otras escenas para puertas de cambio de nivel.
    /// </summary>
    public class InteractiveDoor : MonoBehaviour, IInteractable
    {
        [Header("Comportamiento de Puerta")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string lockedReason = "La puerta está cerrada con llave.";

        [Header("Rotación de Bisagra")]
        [Tooltip("Ángulo relativo al abrirse (usualmente 90 o -90 grados en Y)")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 3.5f;

        [Header("Transición de Escena (Opcional)")]
        [SerializeField] private bool isSceneTransitionDoor = false;
        [SerializeField] private string targetSceneName = "";
        [SerializeField] private string targetSpawnPointName = "";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip lockedClip;

        private Quaternion closedRotation;
        private Quaternion targetRotation;
        private Coroutine rotateCoroutine;
        private InteractableOutline outline;

        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;

        private void Awake()
        {
            closedRotation = transform.localRotation;
            targetRotation = isOpen ? closedRotation * Quaternion.Euler(0f, openAngle, 0f) : closedRotation;
            transform.localRotation = targetRotation;

            outline = GetComponent<InteractableOutline>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        public void Interact(GameObject interactor)
        {
            if (isLocked)
            {
                PlayAudio(lockedClip);
                PlayerInventory.Instance?.GetComponent<PlayerHUD>()?.ShowDirectNotification(lockedReason);
                return;
            }

            if (isSceneTransitionDoor && !string.IsNullOrEmpty(targetSceneName))
            {
                // Transición a la siguiente escena
                StartCoroutine(TransitionRoutine());
                return;
            }

            // Alternar estado de apertura/cierre
            isOpen = !isOpen;
            targetRotation = isOpen ? closedRotation * Quaternion.Euler(0f, openAngle, 0f) : closedRotation;

            PlayAudio(isOpen ? openClip : closeClip);

            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            rotateCoroutine = StartCoroutine(RotateDoorRoutine(targetRotation));
        }

        public void Unlock()
        {
            isLocked = false;
        }

        private IEnumerator RotateDoorRoutine(Quaternion target)
        {
            while (Quaternion.Angle(transform.localRotation, target) > 0.5f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * openSpeed);
                yield return null;
            }
            transform.localRotation = target;
        }

        private IEnumerator TransitionRoutine()
        {
            PlayAudio(openClip);
            if (outline != null) outline.SetHighlighted(false);

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
                yield break;
            }

            yield return new WaitForSeconds(0.4f);

            if (!string.IsNullOrEmpty(targetSpawnPointName))
            {
                PlayerPrefs.SetString("MUUKIA_NextSpawnPoint", targetSpawnPointName);
                PlayerPrefs.Save();
            }

            SceneManager.LoadScene(targetSceneName);
        }

        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, 0.7f);
            }
        }
    }
}

using System.Collections;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Controlador principal de Gustavo Martínez en el Capítulo 1.
    /// Conecta el movimiento, cámara, videocámara, estados cinemáticos
    /// y reacciones de terror psicológico (respiración agitada, temblor).
    /// </summary>
    public class GustavoController : MonoBehaviour
    {
        public static GustavoController Instance { get; private set; }

        [Header("Referencias a Sistemas de Jugador")]
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerLook playerLook;
        [SerializeField] private PlayerInteraction interaction;
        [SerializeField] private PlayerHUD hud;
        [SerializeField] private CharacterController characterController;

        [Header("Audio")]
        [SerializeField] private AudioSource breathingAudioSource;
        [SerializeField] private AudioClip agitatedBreathingClip;

        public bool IsCutsceneLocked { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (playerLook == null) playerLook = GetComponent<PlayerLook>();
            if (interaction == null) interaction = GetComponent<PlayerInteraction>();
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (hud == null) hud = FindAnyObjectByType<PlayerHUD>();
        }

        public void LockPlayerForCutscene(bool locked, bool hideHUD = true)
        {
            IsCutsceneLocked = locked;

            if (movement != null) movement.enabled = !locked;
            if (playerLook != null) playerLook.enabled = !locked;
            if (interaction != null) interaction.enabled = !locked;

            if (characterController != null)
            {
                // Asegurar que no quede velocidad residual
                characterController.SimpleMove(Vector3.zero);
            }

            if (hideHUD && hud != null)
            {
                hud.gameObject.SetActive(!locked);
            }
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            if (characterController != null) characterController.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            Physics.SyncTransforms();
            if (characterController != null) characterController.enabled = true;

            if (movement != null)
            {
                movement.ResetVelocity();
            }
        }

        public void SetAgitatedBreathing(bool agitated)
        {
            if (breathingAudioSource != null && agitatedBreathingClip != null)
            {
                if (agitated)
                {
                    if (!breathingAudioSource.isPlaying)
                    {
                        breathingAudioSource.clip = agitatedBreathingClip;
                        breathingAudioSource.loop = true;
                        breathingAudioSource.volume = 0.6f;
                        breathingAudioSource.Play();
                    }
                }
                else
                {
                    breathingAudioSource.Stop();
                }
            }
        }

        public void TriggerScareReaction()
        {
            SetAgitatedBreathing(true);
            if (VHSCameraEffect.Instance != null)
            {
                VHSCameraEffect.Instance.TriggerGlitchSpike(0.9f, 1.2f);
            }
        }
    }
}

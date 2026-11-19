using System;
using System.Collections;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Controlador del vehículo de Gustavo para la secuencia de conducción en la carretera de Sonora.
    /// Maneja aceleración, frenado, audio del motor, luces y la secuencia del accidente con el monumento Danzante Yaqui.
    /// </summary>
    public class CarController : MonoBehaviour
    {
        [Header("Configuración de Manejo")]
        [SerializeField] private float maxSpeed = 22f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float steeringAngle = 25f;
        [SerializeField] private float brakeForce = 24f;

        [Header("Audio")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioClip engineClip;
        [SerializeField] private AudioClip brakeClip;
        [SerializeField] private AudioClip crashClip;

        [Header("Luces y Cámara")]
        [SerializeField] private Light leftHeadlight;
        [SerializeField] private Light rightHeadlight;
        [SerializeField] private UnityEngine.Camera carCamera;

        [Header("Puntos de Conducción")]
        [SerializeField] private Transform exitCarPoint;

        public bool IsDriving { get; private set; }
        public float CurrentSpeed { get; private set; }

        private Rigidbody carRigidbody;
        private bool hasCrashed = false;
        private Action onExitCarCallback;

        private void Awake()
        {
            carRigidbody = GetComponent<Rigidbody>();
            if (carRigidbody == null) carRigidbody = gameObject.AddComponent<Rigidbody>();
            carRigidbody.mass = 1200f;

            if (carCamera != null) carCamera.gameObject.SetActive(false);
        }

        public void StartDrivingSequence(Action onExitCar)
        {
            IsDriving = true;
            hasCrashed = false;
            onExitCarCallback = onExitCar;

            if (carCamera != null) carCamera.gameObject.SetActive(true);

            if (leftHeadlight != null) leftHeadlight.enabled = true;
            if (rightHeadlight != null) rightHeadlight.enabled = true;

            if (engineAudioSource != null && engineClip != null)
            {
                engineAudioSource.clip = engineClip;
                engineAudioSource.loop = true;
                engineAudioSource.volume = 0.5f;
                engineAudioSource.Play();
            }
        }

        private void Update()
        {
            if (!IsDriving) return;

            if (hasCrashed)
            {
                // Permitir salir del auto con E
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                {
                    ExitCar();
                }
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            // Acelerar / frenar
            if (v > 0)
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, maxSpeed, acceleration * Time.deltaTime);
            }
            else if (v < 0)
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, -maxSpeed * 0.4f, brakeForce * Time.deltaTime);
            }
            else
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, 6f * Time.deltaTime);
            }

            // Giro
            transform.Rotate(Vector3.up, h * steeringAngle * Time.deltaTime * (CurrentSpeed / maxSpeed));

            // Desplazamiento
            transform.Translate(Vector3.forward * CurrentSpeed * Time.deltaTime);

            // Modulación de tono de motor
            if (engineAudioSource != null)
            {
                engineAudioSource.pitch = 0.8f + (Mathf.Abs(CurrentSpeed) / maxSpeed) * 0.8f;
            }
        }

        public void TriggerDanzanteAccident(Transform danzanteStatue, Action onAccidentDone)
        {
            StartCoroutine(AccidentSequenceRoutine(danzanteStatue, onAccidentDone));
        }

        private IEnumerator AccidentSequenceRoutine(Transform danzanteStatue, Action onAccidentDone)
        {
            // Silencio ambiental repentino
            if (engineAudioSource != null) engineAudioSource.volume = 0.2f;

            // Voz de Gustavo asustado
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlaySingleLine(new DialogueLine
                {
                    speakerName = "GUSTAVO",
                    text = "¡No, no, no!",
                    typewriterSpeed = 0.02f,
                    autoAdvance = true,
                    autoAdvanceDelay = 1.0f
                });
            }

            // Chirrido de frenos
            if (sfxAudioSource != null && brakeClip != null)
            {
                sfxAudioSource.PlayOneShot(brakeClip, 0.9f);
            }

            // Desplome de la estatua
            float statueTimer = 0f;
            Quaternion initialStatueRot = danzanteStatue.rotation;
            Quaternion targetStatueRot = initialStatueRot * Quaternion.Euler(65f, 0, 15f);

            while (statueTimer < 1.4f)
            {
                statueTimer += Time.deltaTime;
                float t = statueTimer / 1.4f;
                danzanteStatue.rotation = Quaternion.Lerp(initialStatueRot, targetStatueRot, t * t);
                CurrentSpeed = Mathf.Lerp(CurrentSpeed, 0f, Time.deltaTime * 6f);
                yield return null;
            }

            // Impacto leve
            hasCrashed = true;
            CurrentSpeed = 0f;
            if (engineAudioSource != null) engineAudioSource.Stop();

            if (sfxAudioSource != null && crashClip != null)
            {
                sfxAudioSource.PlayOneShot(crashClip, 1.0f);
            }

            if (VHSCameraEffect.Instance != null)
            {
                VHSCameraEffect.Instance.TriggerGlitchSpike(1.0f, 1.5f);
            }

            yield return new WaitForSeconds(1.0f);

            // Mostrar indicación para salir del auto
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlaySingleLine(new DialogueLine
                {
                    speakerName = "GUSTAVO",
                    text = "Tengo que salir de aquí...",
                    typewriterSpeed = 0.03f,
                    autoAdvance = true,
                    autoAdvanceDelay = 1.8f
                });
            }

            onAccidentDone?.Invoke();
        }

        public void ExitCar()
        {
            if (!IsDriving) return;

            IsDriving = false;
            if (carCamera != null) carCamera.gameObject.SetActive(false);

            Vector3 spawnPos = exitCarPoint != null ? exitCarPoint.position : transform.position + transform.right * 2.0f;
            Quaternion spawnRot = exitCarPoint != null ? exitCarPoint.rotation : transform.rotation;

            if (GustavoController.Instance != null)
            {
                GustavoController.Instance.TeleportTo(spawnPos, spawnRot);
                GustavoController.Instance.LockPlayerForCutscene(false, false);
                GustavoController.Instance.TriggerScareReaction();
            }

            onExitCarCallback?.Invoke();
        }
    }
}

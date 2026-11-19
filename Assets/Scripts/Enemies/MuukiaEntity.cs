using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Controlador de la entidad sobrenatural MUUKIA.
    /// Inspirada en la silueta del danzante del venado yaqui de Sonora.
    /// Maneja los ojos rojos brillantes, el sonido de los tenábaris,
    /// las apariciones a distancia en el desierto y la aparición final en las criptas.
    /// </summary>
    public class MuukiaEntity : MonoBehaviour
    {
        public static MuukiaEntity Instance { get; private set; }

        [Header("Efectos y Ojos")]
        [SerializeField] private Renderer entityRenderer;
        [SerializeField] private Light redEyesLight;
        [SerializeField] private PS2CharacterAnimator characterAnimator;

        [Header("Audio Tenábaris")]
        [SerializeField] private AudioSource tenabarisAudioSource;
        [SerializeField] private AudioClip tenabarisRattleClip;
        [SerializeField] private AudioClip tenabarisLoopClip;

        [Header("Puntos de Aparición en Desierto (Escena 6)")]
        [SerializeField] private List<Transform> desertWaypoints = new List<Transform>();
        [SerializeField] private float triggerProximityDistance = 14f;

        private int currentWaypointIndex = 0;
        private Transform playerTransform;
        private bool isTrackingPlayerProximity = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            if (characterAnimator == null) characterAnimator = GetComponent<PS2CharacterAnimator>();
            if (characterAnimator != null) characterAnimator.SetState(PS2CharacterAnimator.AnimationState.ScaryTwitch);
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerMovement>();
                if (player != null) playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (isTrackingPlayerProximity && playerTransform != null && currentWaypointIndex < desertWaypoints.Count)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                if (dist <= triggerProximityDistance)
                {
                    AdvanceToNextWaypoint();
                }
            }
        }

        public void PlayTenabarisSound(bool loop = true, float volume = 0.8f)
        {
            if (tenabarisAudioSource != null)
            {
                tenabarisAudioSource.clip = loop ? tenabarisLoopClip : tenabarisRattleClip;
                tenabarisAudioSource.loop = loop;
                tenabarisAudioSource.volume = volume;
                tenabarisAudioSource.Play();
            }
        }

        public void StopTenabarisSound()
        {
            if (tenabarisAudioSource != null)
            {
                tenabarisAudioSource.Stop();
            }
        }

        public void StartDesertSequence()
        {
            currentWaypointIndex = 0;
            gameObject.SetActive(true);
            if (desertWaypoints.Count > 0)
            {
                transform.position = desertWaypoints[0].position;
                transform.rotation = desertWaypoints[0].rotation;
            }
            PlayTenabarisSound(true, 0.7f);
            isTrackingPlayerProximity = true;
        }

        private void AdvanceToNextWaypoint()
        {
            isTrackingPlayerProximity = false;
            StartCoroutine(TeleportGlitchRoutine());
        }

        private IEnumerator TeleportGlitchRoutine()
        {
            // Glitch analógico al desvanecerse
            if (VHSCameraEffect.Instance != null)
            {
                VHSCameraEffect.Instance.TriggerGlitchSpike(0.7f, 0.6f);
            }

            // Ocultar un instante
            SetVisibility(false);
            yield return new WaitForSeconds(0.4f);

            currentWaypointIndex++;
            if (currentWaypointIndex < desertWaypoints.Count)
            {
                transform.position = desertWaypoints[currentWaypointIndex].position;
                transform.rotation = desertWaypoints[currentWaypointIndex].rotation;
                SetVisibility(true);
                PlayTenabarisSound(false, 0.85f);
                yield return new WaitForSeconds(0.5f);
                PlayTenabarisSound(true, 0.6f);
                isTrackingPlayerProximity = true;
            }
            else
            {
                // Ha guiado al jugador a las criptas y desaparece temporalmente
                StopTenabarisSound();
                gameObject.SetActive(false);
                Chapter1GameDirector.Instance?.NotifyPlayerArrivedAtCrypts();
            }
        }

        public void SpawnBehindGustavo(Vector3 playerPos, Vector3 playerForward)
        {
            gameObject.SetActive(true);
            SetVisibility(true);

            // Colocar 1.6m detrás de Gustavo mirando hacia su espalda
            Vector3 spawnPos = playerPos - playerForward * 1.6f;
            spawnPos.y = playerPos.y;
            transform.position = spawnPos;
            transform.LookAt(new Vector3(playerPos.x, spawnPos.y, playerPos.z));

            PlayTenabarisSound(false, 1.0f);
            if (redEyesLight != null) redEyesLight.intensity = 4.0f;
        }

        public void SetVisibility(bool visible)
        {
            if (entityRenderer != null) entityRenderer.enabled = visible;
            if (redEyesLight != null) redEyesLight.enabled = visible;
        }
    }
}

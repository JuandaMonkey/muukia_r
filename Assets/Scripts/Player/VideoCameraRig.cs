using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MUUKIA
{
    /// <summary>
    /// Sistema de cámara de video de Gustavo Martínez.
    /// Permite equipar la videocámara analógica, hacer zoom, grabar escenas,
    /// detectar presencias paranormales a través del visor y activar distorsiones VHS.
    /// </summary>
    public class VideoCameraRig : MonoBehaviour
    {
        public static VideoCameraRig Instance { get; private set; }

        [Header("Cámara y Óptica")]
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float maxZoomFOV = 22f;
        [SerializeField] private float zoomSpeed = 40f;

        [Header("Controles")]
        [SerializeField] private KeyCode toggleKey = KeyCode.C;
        [SerializeField] private bool allowFreeToggle = true;

        [Header("HUD del Visor")]
        [SerializeField] private CanvasGroup cameraHUDGroup;
        [SerializeField] private Text timecodeTextLegacy;
        [SerializeField] private TMP_Text timecodeTextTMP;
        [SerializeField] private Image recIndicatorDot;
        [SerializeField] private Text zoomLevelTextLegacy;
        [SerializeField] private TMP_Text zoomLevelTextTMP;
        [SerializeField] private GameObject staticGlitchAlert;

        [Header("Detección Paranormal")]
        [SerializeField] private LayerMask supernaturalLayers = ~0;
        [SerializeField] private float maxDetectionDistance = 60f;

        public bool IsCameraEquipped { get; private set; }
        public bool IsAimingAtSupernatural { get; private set; }

        public event Action<bool> OnCameraToggled;
        public event Action<string> OnSupernaturalDetected;

        private float currentFOV;
        private float targetFOV;
        private float recordingSeconds = 868f; // 00:14:28
        private float blinkTimer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            if (targetCamera == null)
            {
                targetCamera = GetComponentInChildren<UnityEngine.Camera>();
                if (targetCamera == null) targetCamera = UnityEngine.Camera.main;
            }

            currentFOV = normalFOV;
            targetFOV = normalFOV;
            SetCameraEquipped(false);
        }

        private void Update()
        {
            if (allowFreeToggle && (Input.GetKeyDown(toggleKey) || Input.GetMouseButtonDown(1)))
            {
                SetCameraEquipped(!IsCameraEquipped);
            }

            if (IsCameraEquipped)
            {
                HandleZoom();
                UpdateHUD();
                CheckSupernaturalRaycast();
            }
            else
            {
                // Lerp back to normal FOV
                if (targetCamera != null && Mathf.Abs(targetCamera.fieldOfView - normalFOV) > 0.05f)
                {
                    targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, normalFOV, Time.deltaTime * 8f);
                }
            }
        }

        public void SetCameraEquipped(bool equipped)
        {
            IsCameraEquipped = equipped;
            targetFOV = equipped ? 45f : normalFOV;

            if (cameraHUDGroup != null)
            {
                cameraHUDGroup.alpha = equipped ? 1f : 0f;
                cameraHUDGroup.blocksRaycasts = false;
                cameraHUDGroup.interactable = false;
            }

            if (VHSCameraEffect.Instance != null)
            {
                VHSCameraEffect.Instance.SetCameraMode(equipped);
            }

            OnCameraToggled?.Invoke(equipped);
        }

        public void LockToggle(bool allow)
        {
            allowFreeToggle = allow;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                targetFOV -= scroll * zoomSpeed;
                targetFOV = Mathf.Clamp(targetFOV, maxZoomFOV, normalFOV);
            }

            if (targetCamera != null)
            {
                targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * 10f);
            }
        }

        private void UpdateHUD()
        {
            recordingSeconds += Time.deltaTime;
            int hrs = (int)(recordingSeconds / 3600);
            int mins = (int)((recordingSeconds % 3600) / 60);
            int secs = (int)(recordingSeconds % 60);
            string timeStr = string.Format("SP {0:00}:{1:00}:{2:00}", hrs, mins, secs);

            if (timecodeTextTMP != null) timecodeTextTMP.text = timeStr;
            if (timecodeTextLegacy != null) timecodeTextLegacy.text = timeStr;

            // Zoom level display (e.g. 1.0x to 3.5x)
            float zoomRatio = normalFOV / Mathf.Max(targetCamera != null ? targetCamera.fieldOfView : normalFOV, 1f);
            string zoomStr = string.Format("{0:0.0}X", zoomRatio);
            if (zoomLevelTextTMP != null) zoomLevelTextTMP.text = zoomStr;
            if (zoomLevelTextLegacy != null) zoomLevelTextLegacy.text = zoomStr;

            // Blinking REC dot
            blinkTimer += Time.deltaTime;
            if (recIndicatorDot != null)
            {
                recIndicatorDot.enabled = (blinkTimer % 1.0f) < 0.65f;
            }
        }

        private void CheckSupernaturalRaycast()
        {
            if (targetCamera == null) return;

            Ray ray = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            bool detected = false;
            if (Physics.Raycast(ray, out hit, maxDetectionDistance, supernaturalLayers, QueryTriggerInteraction.Collide))
            {
                SupernaturalTarget target = hit.collider.GetComponent<SupernaturalTarget>();
                if (target == null) target = hit.collider.GetComponentInParent<SupernaturalTarget>();

                if (target != null && target.IsActiveTarget)
                {
                    detected = true;
                    if (!IsAimingAtSupernatural)
                    {
                        IsAimingAtSupernatural = true;
                        if (VHSCameraEffect.Instance != null)
                        {
                            VHSCameraEffect.Instance.TriggerGlitchSpike(0.75f, 0.8f);
                        }
                        if (staticGlitchAlert != null) staticGlitchAlert.SetActive(true);
                        OnSupernaturalDetected?.Invoke(target.TargetId);
                        target.NotifyObserved();
                    }
                }
            }

            if (!detected && IsAimingAtSupernatural)
            {
                IsAimingAtSupernatural = false;
                if (staticGlitchAlert != null) staticGlitchAlert.SetActive(false);
            }
        }
    }
}


using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private Transform playerCamera;

    [Header("Alturas")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1.2f;

    [Header("Velocidad de transicion")]
    [SerializeField] private float transitionSpeed = 8f;

    private CharacterController controller;
    private float standingCameraHeight;
    private float crouchingCameraHeight;

    public bool IsCrouching { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (controller != null && controller.height > 0.5f)
        {
            standingHeight = controller.height;
            crouchingHeight = standingHeight * 0.6f;
        }

        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
        }

        standingCameraHeight = playerCamera != null ? playerCamera.localPosition.y : 1.6f;
        crouchingCameraHeight = standingCameraHeight -
                                (standingHeight - crouchingHeight);

        controller.height = standingHeight;
        controller.center = new Vector3(0f, standingHeight / 2f, 0f);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (IsCrouching)
            {
                TryStandUp();
            }
            else
            {
                IsCrouching = true;
            }
        }

        UpdateCrouch();
    }

    private void TryStandUp()
    {
        float radius = controller.radius * 0.95f;

        Vector3 origin = transform.position +
            Vector3.up * (crouchingHeight - radius);

        float distance = standingHeight -
                         crouchingHeight;

        bool blocked = Physics.SphereCast(
            origin,
            radius,
            Vector3.up,
            out RaycastHit hit,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        if (!blocked)
            IsCrouching = false;
    }

    private void UpdateCrouch()
    {
        float targetHeight = IsCrouching
            ? crouchingHeight
            : standingHeight;

        float targetCameraHeight = IsCrouching
            ? crouchingCameraHeight
            : standingCameraHeight;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            transitionSpeed * Time.deltaTime
        );

        controller.center = new Vector3(
            0f,
            controller.height / 2f,
            0f
        );

        Vector3 cameraPosition = playerCamera.localPosition;

        cameraPosition.y = Mathf.Lerp(
            cameraPosition.y,
            targetCameraHeight,
            transitionSpeed * Time.deltaTime
        );

        playerCamera.localPosition = cameraPosition;
    }
}

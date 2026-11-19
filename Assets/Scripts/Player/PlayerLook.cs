
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float sensitivity = 150f;

    [Header("Limites")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    private float xRotation = 0f;

    void Awake()
    {
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
        }
    }

    void Start()
    {
        SetCursorState(true);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        HandleCursor();

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = Input.GetAxis("Mouse X") *
                       sensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") *
                       sensitivity * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(
            xRotation,
            minVerticalAngle,
            maxVerticalAngle
        );

        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
            else return;
        }

        playerCamera.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    public void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandleCursor()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetMouseButtonDown(0))
        {
            SetCursorState(true);
        }
    }
}

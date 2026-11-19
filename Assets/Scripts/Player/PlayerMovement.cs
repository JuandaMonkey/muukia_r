
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 8.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Salto")]
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Referencias")]
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private PlayerCrouch crouch;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (stamina == null)
            stamina = GetComponent<PlayerStamina>();

        if (crouch == null)
            crouch = GetComponent<PlayerCrouch>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        Move();
    }

    /// <summary>
    /// Reinicia la velocidad acumulada del jugador (util al reaparecer o teletransportarse).
    /// </summary>
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    private void Move()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
            if (controller == null) return;
        }

        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 direction =
            transform.right * x +
            transform.forward * z;

        direction = Vector3.ClampMagnitude(direction, 1f);

        // Correr
        bool wantsToRun =
            Input.GetKey(KeyCode.LeftShift) &&
            z > 0.1f &&
            direction.sqrMagnitude > 0.01f &&
            (crouch == null || !crouch.IsCrouching);

        bool running = wantsToRun &&
            (stamina == null || stamina.TryUse(Time.deltaTime));

        if (!running && stamina != null)
            stamina.Recover(Time.deltaTime);

        float currentSpeed = running ? runSpeed : walkSpeed;

        if (crouch != null && crouch.IsCrouching)
            currentSpeed *= 0.5f;

        // Movimiento horizontal
        controller.Move(
            direction * currentSpeed * Time.deltaTime
        );

        // Salto (consume estamina y cansa al jugador)
        bool canJump = (crouch == null || !crouch.IsCrouching) && (stamina == null || stamina.CanJump);

        if (grounded && Input.GetButtonDown("Jump") && canJump)
        {
            float jumpMultiplier = 1f;
            if (stamina != null)
            {
                jumpMultiplier = stamina.TryUseJump();
            }

            if (jumpMultiplier > 0.1f)
            {
                velocity.y = Mathf.Sqrt(
                    jumpHeight * jumpMultiplier * -2f * gravity
                );
            }
        }

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        if (velocity.y < -40f) velocity.y = -40f;

        controller.Move(velocity * Time.deltaTime);
    }
}

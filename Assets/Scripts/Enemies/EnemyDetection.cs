using UnityEngine;

/// <summary>
/// Gestiona la detección visual del jugador por parte del enemigo mediante ángulo de visión y raycast.
/// </summary>
public class EnemyDetection : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private Transform player = null;

    public bool CanSeePlayer { get; private set; }

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        CanSeePlayer = false;

        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > detectionDistance) return;

        float angle = Vector3.Angle(transform.forward, direction);

        if (angle > fieldOfView / 2f) return;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            direction.normalized,
            out RaycastHit hit,
            detectionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.CompareTag("Player"))
            {
                CanSeePlayer = true;
            }
        }
    }
}

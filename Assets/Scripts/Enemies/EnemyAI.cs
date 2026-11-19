
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Return
    }

    [Header("Referencias")]
    [SerializeField] private Transform player = null;
    [SerializeField] private Transform[] patrolPoints = new Transform[0];

    [Header("Deteccion")]
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField] private float loseDistance = 15f;

    [Header("Velocidades")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4.5f;

    [Header("Patrullaje")]
    [SerializeField] private float waypointTolerance = 1f;
    [SerializeField] private float waitAtWaypoint = 2f;

    private NavMeshAgent agent;
    private EnemyState currentState;

    private int currentWaypoint;
    private float waitTimer;

    private Vector3 initialPosition;

    public EnemyState CurrentState => currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;
    }

    void Start()
    {
        currentState = EnemyState.Patrol;

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (patrolPoints.Length > 0)
            MoveToNextWaypoint();
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
            return;

        DetectPlayer();

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Return:
                ReturnToPatrol();
                break;
        }
    }

    private void DetectPlayer()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (currentState != EnemyState.Chase &&
            distance <= detectionDistance)
        {
            currentState = EnemyState.Chase;
            agent.speed = chaseSpeed;

            Debug.Log("El enemigo ha detectado al jugador.");
        }
        else if (currentState == EnemyState.Chase &&
                 distance > loseDistance)
        {
            currentState = EnemyState.Return;

            Debug.Log("El enemigo ha perdido al jugador.");
        }
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= waypointTolerance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitAtWaypoint)
            {
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        if (patrolPoints.Length == 0)
            return;

        agent.SetDestination(
            patrolPoints[currentWaypoint].position
        );

        currentWaypoint =
            (currentWaypoint + 1) % patrolPoints.Length;
    }

    private void ChasePlayer()
    {
        if (player == null)
            return;

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void ReturnToPatrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints != null &&
            patrolPoints.Length > 0)
        {
            agent.SetDestination(
                patrolPoints[currentWaypoint].position
            );
        }
        else
        {
            agent.SetDestination(initialPosition);
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= waypointTolerance)
        {
            currentState = EnemyState.Patrol;
            waitTimer = 0f;
        }
    }
}

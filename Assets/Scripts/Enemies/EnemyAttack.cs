using UnityEngine;

/// <summary>
/// Gestiona la lógica de ataque del enemigo hacia el jugador cuando se encuentra en rango.
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int damage = 25;

    [Header("Referencias")]
    [SerializeField] private Transform player = null;

    private float nextAttackTime;

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
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackDistance && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log("[EnemyAttack] El enemigo atacó al jugador infligiendo " + damage + " de daño.");
        }
    }
}

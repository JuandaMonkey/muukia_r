
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Eventos")]
    public UnityEvent OnDeath = new UnityEvent();
    public UnityEvent OnHealthChanged = new UnityEvent();

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public bool IsDead => currentHealth <= 0f;

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage
        );

        OnHealthChanged?.Invoke();

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );

        OnHealthChanged?.Invoke();
    }

    private void Die()
    {
        Debug.Log("El jugador ha muerto.");

        OnDeath?.Invoke();
    }
}

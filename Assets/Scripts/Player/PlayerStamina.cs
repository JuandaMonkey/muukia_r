using System;
using UnityEngine;

/// <summary>
/// Gestiona la resistencia (stamina) del jugador para correr y saltar.
/// Proporciona valores fluidos y eventos para el HUD sin sobrecoste de rendimiento.
/// </summary>
public class PlayerStamina : MonoBehaviour
{
    [Header("Resistencia")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;

    [Header("Consumo y Recuperación")]
    [SerializeField] private float runConsumptionRate = 22f; // Costo por segundo al correr
    [SerializeField] private float jumpStaminaCost = 18f;    // Costo por cada salto
    [SerializeField] private float recoveryRate = 16f;       // Recuperación por segundo
    [SerializeField] private float recoveryDelay = 0.7f;     // Retraso tras consumir antes de regenerar

    private float delayTimer;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => Mathf.Clamp01(currentStamina / maxStamina);
    public bool CanJump => currentStamina >= 5f;
    public bool IsExhausted => currentStamina <= 0.1f;

    public event Action<float, float> OnStaminaChanged; // current, max
    public event Action OnStaminaExhausted;

    private void Awake()
    {
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    /// <summary>
    /// Consume estamina continuamente mientras el jugador corre.
    /// </summary>
    public bool TryUse(float deltaTime)
    {
        if (currentStamina <= 0f)
            return false;

        currentStamina = Mathf.Max(0f, currentStamina - runConsumptionRate * deltaTime);
        delayTimer = recoveryDelay;

        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        if (currentStamina <= 0f)
        {
            OnStaminaExhausted?.Invoke();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Consume estamina de forma instantánea al realizar un salto.
    /// Devuelve un multiplicador de efectividad (1.0 = salto completo, menor = salto cansado).
    /// </summary>
    public float TryUseJump()
    {
        if (currentStamina < 5f)
        {
            OnStaminaExhausted?.Invoke();
            return 0f;
        }

        float jumpPower = currentStamina >= jumpStaminaCost ? 1f : Mathf.Clamp01(currentStamina / jumpStaminaCost);
        currentStamina = Mathf.Max(0f, currentStamina - jumpStaminaCost);
        delayTimer = recoveryDelay;

        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        if (currentStamina <= 0f)
        {
            OnStaminaExhausted?.Invoke();
        }

        return Mathf.Max(0.6f, jumpPower);
    }

    /// <summary>
    /// Regenera la estamina gradualmente tras el tiempo de retraso de recuperación.
    /// </summary>
    public void Recover(float deltaTime)
    {
        if (currentStamina >= maxStamina)
            return;

        if (delayTimer > 0f)
        {
            delayTimer -= deltaTime;
            return;
        }

        currentStamina = Mathf.Min(maxStamina, currentStamina + recoveryRate * deltaTime);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    /// <summary>
    /// Restaura una cantidad de estamina de forma inmediata (ej. al consumir una bebida energética).
    /// </summary>
    public void RestoreInstant(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        delayTimer = 0f;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}

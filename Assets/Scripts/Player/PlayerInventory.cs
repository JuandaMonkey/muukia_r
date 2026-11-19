using System;
using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Gestiona el inventario de supervivencia de Gustavo Martínez inspirado en Outlast.
    /// Capacidad estricta: Máximo 3 Vendas (recupera salud) y 2 Bebidas Energéticas (recupera estamina).
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        public const int MAX_BANDAGES = 3;
        public const int MAX_ENERGY_DRINKS = 2;

        [Header("Inventario Actual")]
        [SerializeField] private int bandages = 0;
        [SerializeField] private int energyDrinks = 0;

        [Header("Valores de Curación y Energía")]
        [SerializeField] private float healAmount = 35f;
        [SerializeField] private float staminaRestoreAmount = 60f;

        [Header("Referencias")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerStamina playerStamina;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip itemPickupClip;
        [SerializeField] private AudioClip itemUseClip;

        public int Bandages => bandages;
        public int EnergyDrinks => energyDrinks;

        public event Action<int, int> OnInventoryUpdated; // bandages, energyDrinks
        public event Action<string> OnNotification;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(this);
                return;
            }

            if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
            if (playerStamina == null) playerStamina = GetComponent<PlayerStamina>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            bandages = Mathf.Clamp(bandages, 0, MAX_BANDAGES);
            energyDrinks = Mathf.Clamp(energyDrinks, 0, MAX_ENERGY_DRINKS);
        }

        private void Start()
        {
            OnInventoryUpdated?.Invoke(bandages, energyDrinks);
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            // Tecla 1: Usar Venda
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseBandage();
            }

            // Tecla 2: Usar Bebida Energética
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UseEnergyDrink();
            }
        }

        public bool TryAddBandage()
        {
            if (bandages >= MAX_BANDAGES)
            {
                OnNotification?.Invoke("Inventario lleno: Máximo 3 Vendas");
                return false;
            }

            bandages++;
            PlaySound(itemPickupClip);
            OnNotification?.Invoke($"+1 VENDA ({bandages}/{MAX_BANDAGES})");
            OnInventoryUpdated?.Invoke(bandages, energyDrinks);
            return true;
        }

        public bool TryAddEnergyDrink()
        {
            if (energyDrinks >= MAX_ENERGY_DRINKS)
            {
                OnNotification?.Invoke("Inventario lleno: Máximo 2 Bebidas");
                return false;
            }

            energyDrinks++;
            PlaySound(itemPickupClip);
            OnNotification?.Invoke($"+1 BEBIDA ENERGÉTICA ({energyDrinks}/{MAX_ENERGY_DRINKS})");
            OnInventoryUpdated?.Invoke(bandages, energyDrinks);
            return true;
        }

        public void UseBandage()
        {
            if (bandages <= 0)
            {
                OnNotification?.Invoke("No tienes vendas en el inventario");
                return;
            }

            if (playerHealth != null && playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                OnNotification?.Invoke("Salud al máximo");
                return;
            }

            bandages--;
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
            }

            PlaySound(itemUseClip);
            OnNotification?.Invoke($"Venda aplicada (+{healAmount:0} Salud)");
            OnInventoryUpdated?.Invoke(bandages, energyDrinks);
        }

        public void UseEnergyDrink()
        {
            if (energyDrinks <= 0)
            {
                OnNotification?.Invoke("No tienes bebidas energéticas");
                return;
            }

            if (playerStamina != null && playerStamina.CurrentStamina >= playerStamina.MaxStamina)
            {
                OnNotification?.Invoke("Estamina al máximo");
                return;
            }

            energyDrinks--;
            if (playerStamina != null)
            {
                playerStamina.RestoreInstant(staminaRestoreAmount);
            }

            PlaySound(itemUseClip);
            OnNotification?.Invoke($"Bebida consumida (+{staminaRestoreAmount:0} Estamina)");
            OnInventoryUpdated?.Invoke(bandages, energyDrinks);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, 0.75f);
            }
        }
    }
}

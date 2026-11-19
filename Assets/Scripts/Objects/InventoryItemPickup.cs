using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Objeto interactuable en el escenario que se puede recoger y almacenar en el inventario.
    /// Soporta Vendas (recupera salud) y Bebidas Energéticas (recupera estamina).
    /// </summary>
    public class InventoryItemPickup : MonoBehaviour, IInteractable
    {
        public enum ItemType
        {
            Bandage,
            EnergyDrink
        }

        [Header("Tipo de Objeto")]
        [SerializeField] private ItemType type = ItemType.Bandage;
        [SerializeField] private string customPrompt = "";

        private InteractableOutline outline;

        private void Awake()
        {
            outline = GetComponent<InteractableOutline>();
        }

        public void Interact(GameObject interactor)
        {
            PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                inventory = PlayerInventory.Instance;
            }

            if (inventory == null) return;

            bool added = false;
            if (type == ItemType.Bandage)
            {
                added = inventory.TryAddBandage();
            }
            else if (type == ItemType.EnergyDrink)
            {
                added = inventory.TryAddEnergyDrink();
            }

            if (added)
            {
                if (outline != null) outline.SetHighlighted(false);
                Destroy(gameObject);
            }
        }

        public string GetPromptMessage()
        {
            if (!string.IsNullOrEmpty(customPrompt)) return customPrompt;

            return type switch
            {
                ItemType.Bandage => "Presiona [E] para recoger Venda",
                ItemType.EnergyDrink => "Presiona [E] para recoger Bebida Energética",
                _ => "Presiona [E] para recoger"
            };
        }
    }
}

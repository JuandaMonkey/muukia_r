using UnityEngine;
using UnityEngine.Events;

namespace MUUKIA
{
    /// <summary>
    /// Zona de activación narrativa por colisión con el jugador.
    /// </summary>
    public class TriggerZoneEvent : MonoBehaviour
    {
        [SerializeField] private bool onlyOnce = true;
        [SerializeField] private UnityEvent onTriggered;

        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (onlyOnce && hasTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null || other.GetComponentInParent<PlayerMovement>() != null)
            {
                hasTriggered = true;
                onTriggered?.Invoke();
            }
        }
    }
}

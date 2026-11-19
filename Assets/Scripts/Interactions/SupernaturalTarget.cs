using System;
using UnityEngine;
using UnityEngine.Events;

namespace MUUKIA
{
    /// <summary>
    /// Marca un objeto o punto narrativo como detectable por la cámara de video de Gustavo.
    /// Desencadena interferencias VHS y eventos de historia cuando el jugador enfoca hacia él.
    /// </summary>
    public class SupernaturalTarget : MonoBehaviour
    {
        [SerializeField] private string targetId = "MaderosCastleWindow";
        [SerializeField] private bool isActiveTarget = true;
        [SerializeField] private bool singleTrigger = true;

        [Header("Eventos")]
        [SerializeField] private UnityEvent onObserved;

        public string TargetId => targetId;
        public bool IsActiveTarget
        {
            get => isActiveTarget;
            set => isActiveTarget = value;
        }

        private bool hasBeenObserved = false;

        public void NotifyObserved()
        {
            if (singleTrigger && hasBeenObserved) return;

            hasBeenObserved = true;
            onObserved?.Invoke();
        }

        public void SetActiveState(bool active)
        {
            isActiveTarget = active;
        }
    }
}

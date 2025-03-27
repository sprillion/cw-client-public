using System;
using environment;
using UnityEngine;

namespace character
{
    public class InteractDetector : MonoBehaviour
    {
        public event Action<IInteractable> OnInteractEnter; 
        public event Action<IInteractable> OnInteractExit; 
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Interact")) return;
            if (!other.TryGetComponent(out IInteractable interactable)) return;
            
            OnInteractEnter?.Invoke(interactable);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Interact")) return;
            if (!other.TryGetComponent(out IInteractable interactable)) return;
            
            OnInteractExit?.Invoke(interactable);
        }
    }
}
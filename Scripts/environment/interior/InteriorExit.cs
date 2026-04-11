using System;
using environment;
using infrastructure.services.interior;
using UnityEngine;

namespace environment.interior
{
    public class InteriorExit : MonoBehaviour, IInteractable
    {
        public event Action<IInteractable> OnDestroyed;

        private IInteriorService _interiorService;

        public void Initialize(IInteriorService interiorService)
        {
            _interiorService = interiorService;
        }

        public void Interact()
        {
            _interiorService.Exit();
        }
    }
}

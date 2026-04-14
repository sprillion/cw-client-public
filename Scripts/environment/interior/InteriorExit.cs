using System;
using environment;
using infrastructure.services.interior;
using UnityEngine;
using Zenject;

namespace environment.interior
{
    public class InteriorExit : MonoBehaviour, IInteractable
    {
        private IInteriorService _interiorService;
        
        [field: SerializeField] public bool DisablePanel { get; private set; }
        [field: SerializeField] public bool DisableButton { get; private set; }
        public event Action<IInteractable> OnDestroyed;

        [Inject]
        public void Construct(IInteriorService interiorService)
        {
            _interiorService = interiorService;
        }

        public void Interact()
        {
            _interiorService.Exit();
        }
    }
}

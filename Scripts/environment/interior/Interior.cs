using infrastructure.services.interior;
using UnityEngine;
using Zenject;

namespace environment.interior
{
    public class Interior : MonoBehaviour
    {
        [SerializeField] private InteriorType _type;
        [SerializeField] private Vector3 _worldPosition;
        [SerializeField] private bool _disablePlayer;
        [SerializeField] private bool _disableAllCanvases;
        [SerializeField] private bool _hideOtherCharactersAndMobs;
        [SerializeField] private bool _hasOwnCamera;

        private IInteriorService _interiorService;
        
        public InteriorType Type => _type;
        public bool DisablePlayer => _disablePlayer;
        public bool DisableAllCanvases => _disableAllCanvases;
        public bool HideOtherCharactersAndMobs => _hideOtherCharactersAndMobs;
        public bool HasOwnCamera => _hasOwnCamera;
        

        [Inject]
        public void Construct(IInteriorService interiorService)
        {
            _interiorService = interiorService;
        }

        public void Start()
        {
            transform.position = _worldPosition;
        }
    }
}

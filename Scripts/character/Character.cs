using factories.characters;
using UnityEngine;
using Zenject;

namespace character
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private CharacterSkin _characterSkin;
        [SerializeField] private InteractDetector _interactDetector;
        [SerializeField] private HandItemsController _handItemsController;
        
        public CharacterStats CharacterStats { get; private set; }
        public InteractDetector InteractDetector => _interactDetector;

        public HandItemsController HandItemsController => _handItemsController;
        

        [Inject]
        public void Construct(ICharacterFactory characterFactory)
        {
            CharacterStats = characterFactory.CreateCharacterStats();
        }

        public void SetPosition(Vector3 position)
        {
            _characterMovement.SetPosition(position);
        }

    }
}
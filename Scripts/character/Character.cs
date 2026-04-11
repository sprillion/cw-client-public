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
        
        public int Id { get; set; }
        public bool IsDead { get; set; }
        public CharacterStats CharacterStats { get; private set; }

        public CharacterMovement CharacterMovement => _characterMovement;
        public CharacterSkin CharacterSkin => _characterSkin;
        public InteractDetector InteractDetector => _interactDetector;

        public HandItemsController HandItemsController => _handItemsController;
        

        [Inject]
        public void Construct(ICharacterFactory characterFactory)
        {
            CharacterStats = characterFactory.CreateCharacterStats();
            _handItemsController.Initialize();
        }
    }
}
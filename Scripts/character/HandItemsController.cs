using factories.characters;
using infrastructure.services.inventory.items;
using UnityEngine;
using Zenject;

namespace character
{
    public class HandItemsController : MonoBehaviour
    {
        [SerializeField] private CharacterAnimator _characterAnimator;
        [SerializeField] private PotionThrowController _potionThrowController;
        
        [SerializeField] private Transform _swordParent;

        private PooledObject _currentObject;

        public Item CurrentItem { get; private set; }

        
        private ICharacterFactory _characterFactory;
        
        [Inject]
        public void Construct(ICharacterFactory characterFactory)
        {
            _characterFactory = characterFactory;
            
            _potionThrowController.gameObject.SetActive(false);
        }

        public void ChangeHandItem(Item item)
        {
            if (CurrentItem == item) return;
            CurrentItem = item;
            
            ClearCurrentObject();
            
            if (CurrentItem == null) return;
            
            switch (CurrentItem.Data.ItemType)
            {
                case ItemType.Weapon:
                    CreateSword();
                    break;
                case ItemType.Food:
                    break;
                case ItemType.Potion:
                    SetPotion();
                    break;
            }
        }

        private void ClearCurrentObject()
        {
            _currentObject?.Release();
            _currentObject = null;
            _potionThrowController.gameObject.SetActive(false);
            _characterAnimator.DisableAttackLayer();
        }

        private void CreateSword()
        {

        }

        private void SetPotion()
        {
            _potionThrowController.SetItem(CurrentItem);
            _potionThrowController.gameObject.SetActive(true);
        }
    }
}
using character.handItems;
using infrastructure.services.inventory.items;
using UnityEngine;

namespace character
{
    public class HandItemsController : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_BaseMap");
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");
        
        [SerializeField] private CharacterAnimator _characterAnimator;
        [SerializeField] private PotionThrowController _potionThrowController;
        [SerializeField] private CharacterCombat _characterCombat;
        [SerializeField] private FoodController _foodController;
        
        [SerializeField] private Renderer _sword;
                
        private MaterialPropertyBlock _swordBlock;
        
        private HandsItemData _handsItemData;
        private PooledObject _currentObject;
        
        public Item CurrentItem { get; private set; }

        public void Initialize()
        {
            _swordBlock = new MaterialPropertyBlock();
            _sword.GetPropertyBlock(_swordBlock, 0);
            _handsItemData = GameResources.Data.Character.hands_item_data<HandsItemData>();
            
            _potionThrowController.gameObject.SetActive(false);
            _sword.gameObject.SetActive(false);
        }

        public void ChangeHandItem(Item item)
        {
            if (CurrentItem == item) return;
            CurrentItem = item;
            
            ClearCurrentObject();
            
            if (CurrentItem == null) return;
            
            switch (CurrentItem.Data.ItemType)
            {
                case ItemType.Sword:
                    SetSword();
                    break;
                case ItemType.Food:
                    SetFood();
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
            _characterCombat.gameObject.SetActive(true);
            _potionThrowController.gameObject.SetActive(false);
            _characterAnimator.DisableAttackLayer();
            _sword.gameObject.SetActive(false);
            _characterAnimator.SetSword(false);
            _foodController.gameObject.SetActive(false);
        }

        private void SetSword()
        {
            _characterCombat.gameObject.SetActive(true);
            var swordTextures = _handsItemData.GetSwordTexture(CurrentItem.Id);
            _swordBlock.SetTexture(MainTex, swordTextures.Main);
            _swordBlock.SetTexture(BumpMap, swordTextures.NormalMap);
            _swordBlock.SetFloat(BumpScale, swordTextures.NormalScale);
            _sword.SetPropertyBlock(_swordBlock, 0);
            _sword.gameObject.SetActive(true);
            _characterAnimator.SetSword(true);
        }

        private void SetPotion()
        {
            _characterCombat.gameObject.SetActive(false);
            _potionThrowController.SetItem(CurrentItem);
            _potionThrowController.gameObject.SetActive(true);
        }

        private void SetFood()
        {
            _characterCombat.gameObject.SetActive(false);
            _foodController.SetItem(CurrentItem);
            _foodController.gameObject.SetActive(true);
        }
    }
}
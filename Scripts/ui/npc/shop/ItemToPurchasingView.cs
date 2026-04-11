using factories.inventory;
using infrastructure.services.npc;
using infrastructure.services.players;
using TMPro;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.npc
{
    public class ItemToPurchasingView : PooledObject
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _buyButton;

        private IInventoryFactory _inventoryFactory;
        private INpcService _npcService;
        private ICharacterService _characterService;
        public ItemToPurchasing CurrentItem { get; private set; }


        [Inject]
        public void Construct(IInventoryFactory inventoryFactory, INpcService npcService, ICharacterService characterService)
        {
            _inventoryFactory = inventoryFactory;
            _npcService = npcService;
            _characterService = characterService;
            
            _buyButton.onClick.AddListener(Buy);
        }
        
        public void SetItem(ItemToPurchasing item)
        {
            CurrentItem = item;
            _iconImage.sprite = _inventoryFactory.GetItemData(CurrentItem.Item.Id).Icon;
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            if (CurrentItem == null) return;

            var iconCurrency = CurrentItem.CurrencyType == CurrencyType.Gold ? IconType.Gold : IconType.Diamond;
            _priceText.text = $"{iconCurrency.ToIcon()} {CurrentItem.Price}";

            _buyButton.interactable = _characterService.CurrentCharacter.CharacterStats.HaveCurrency(CurrentItem.CurrencyType, CurrentItem.Price);
        }

        private void Buy()
        {
            if (CurrentItem == null) return;
            
            _npcService.BuyItem(CurrentItem.Item.Id);
        }
    }
}
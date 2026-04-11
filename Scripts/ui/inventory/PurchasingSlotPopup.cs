using infrastructure.services.inventory;
using infrastructure.services.npc;
using infrastructure.services.players;
using TMPro;
using ui.popup;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.inventory
{
    public class PurchasingSlotPopup : Popup
    {
        [SerializeField] private TMP_Text _price;

        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _cancelButton;

        [SerializeField] private LoadingPanel _loadingPanel;

        private IInventoryService _inventoryService;
        private ICharacterService _characterService;
        
        [Inject]
        public void Construct(IInventoryService inventoryService, ICharacterService characterService)
        {
            _inventoryService = inventoryService;
            _characterService = characterService;
            _inventoryService.OnPurchsingSlotInfo += SetInfo;
        }
        
        public override void Initialize()
        {
            _buyButton.onClick.AddListener(Buy);
            _cancelButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            _inventoryService.GetPurchasingInfo();
            _loadingPanel.Show();
            base.Show();
        }

        private void SetInfo(PurchasingSlot purchasingSlot)
        {
            if (!purchasingSlot.CanBuy)
            {
                Hide();
                _loadingPanel.Hide();
                return;
            }

            var icon = purchasingSlot.CurrencyType switch
            {
                CurrencyType.Gold => IconType.Gold,
                CurrencyType.Diamonds => IconType.Diamond,
                _ => IconType.Gold
            };
            
            _price.text = $"{purchasingSlot.Price}{icon.ToIcon()}";

            _buyButton.interactable =
                _characterService.CurrentCharacter.CharacterStats.HaveCurrency(purchasingSlot.CurrencyType,
                    purchasingSlot.Price);
            _loadingPanel.Hide();
        }

        private void Buy()
        {
            _inventoryService.BuyNewSlot();
            Hide();
        }
    }
}
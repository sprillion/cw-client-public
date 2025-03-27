using System;
using factories.inventory;
using infrastructure.services.npc;
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
        public ItemToPurchasing CurrentItem { get; private set; }

        public event Action<int> OnBuy;

        [Inject]
        public void Construct(IInventoryFactory inventoryFactory)
        {
            _inventoryFactory = inventoryFactory;
            
            _buyButton.onClick.AddListener(Buy);
        }

        public override void Release()
        {
            OnBuy = null;
            base.Release();
        }

        public void SetItem(ItemToPurchasing item)
        {
            CurrentItem = item;

            _iconImage.sprite = _inventoryFactory.GetItemData(item.Item.Id).Icon;
            var iconCurrency = item.CurrencyType == CurrencyType.Gold ? IconType.Gold : IconType.Diamond;
            _priceText.text = $"{iconCurrency.ToIcon()} {item.Price}";
        }

        private void Buy()
        {
            if (CurrentItem == null) return;
            
            OnBuy?.Invoke(CurrentItem.Item.Id);
        }
    }
}
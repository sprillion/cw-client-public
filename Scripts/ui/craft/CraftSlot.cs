using factories.inventory;
using infrastructure.factories;
using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using TMPro;
using ui.inventory;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.craft
{
    public class CraftSlot : MonoBehaviour
    {
        [SerializeField] private Slot _slot;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Image _readyIcon;
        [SerializeField] private Image _notReadyIcon;

        private IInventoryFactory _inventoryFactory;
        private IInventoryService _inventoryService;
        
        public bool IsReady { get; private set; }
        private bool _isResult;
        
        [Inject]
        public void Construct(IInventoryFactory inventoryFactory, IInventoryService inventoryService)
        {
            _inventoryFactory = inventoryFactory;
            _inventoryService = inventoryService;
        }
        
        public void SetItem(infrastructure.services.craft.Item item, bool isResult)
        {
            _isResult = isResult;
            
            var uiItem = Pool.Get<UiItem>();

            uiItem.Initialize(
                new Item()
                {
                    Id = item.Id, 
                    Count = item.Count,
                    // ShowCount = false
                },
                null);
            uiItem.Draggable = false;
            _slot.SetItem(uiItem);
            CheckReady(item.Count);
        }

        public void Clear()
        {
            _slot.ClearItem();
            _countText.text = "";
            _readyIcon.gameObject.SetActive(false);
            _notReadyIcon.gameObject.SetActive(false);
        }

        private void CheckReady(int count)
        {
            if (_isResult)
            {
                _countText.text = count.ToString();
                _readyIcon.gameObject.SetActive(false);
                _notReadyIcon.gameObject.SetActive(false);
            }
            else
            {
                var currentCount = _inventoryService.CountItems(_slot.CurrentUiItem.CurrentItem.Id);
                _countText.text = $"{currentCount}/{count}";
                IsReady = currentCount >= count;
                _readyIcon.gameObject.SetActive(IsReady);
                _notReadyIcon.gameObject.SetActive(!IsReady);
            }
        }
    }
}
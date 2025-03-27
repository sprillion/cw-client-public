using System.Collections.Generic;
using environment.chests;
using factories.inventory;
using infrastructure.services.chests;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.inventory.chest
{
    public class ChestPopup : Popup
    {
        [SerializeField] private Button _getAllButton;
        [SerializeField] private Transform _slotsParent;
        
        private readonly List<Slot> _slots = new List<Slot>();
        
        private IInventoryFactory _inventoryFactory;
        private IChestsService _chestsService;

        private ChestFromMob _currentChest;

        [Inject]
        public void Construct(IInventoryFactory inventoryFactory, IChestsService chestsService)
        {
            _inventoryFactory = inventoryFactory;
            _chestsService = chestsService;
            
            _getAllButton.onClick.AddListener(GetAllItems);

            for (int i = 0; i < 20; i++)
            {
                CreateSlot(i);
            }
        }

        public override void Show(Popup backPopup)
        {
            _getAllButton.gameObject.SetActive(true);
            _getAllButton.interactable = true;
            base.Show(backPopup);
        }

        public override void Hide()
        {
            base.Hide();
            if (_currentChest == null) return;
            
            _currentChest.OnSetItems -= SetItems;
            _currentChest = null;
        }

        public void SetChestFromMob(ChestFromMob chestFromMob)
        {
            _currentChest = chestFromMob;
            _slots.ForEach(s => s.ClearItem());
            _currentChest.OnSetItems += SetItems;
            SetItems();
        }

        private void SetItems()
        {
            _slots.ForEach(s => s.ClearItem());
            
            for (int i = 0; i < _currentChest.Items.Count; i++)
            {
                if (_slots.Count <= i)
                {
                    CreateSlot(i);
                }
                CreateItem(i);
            }
        }
        
        private void CreateItem(int index)
        {
            var item = _currentChest.Items[index];
            var slot = _slots[index];
            
            var uiItem = _inventoryFactory.GetItem();
            
            uiItem.Initialize(item, transform.parent);
            uiItem.Draggable = false;
            uiItem.OnClickItem += GetItem;
            
            slot.SetItem(uiItem);
        }

        private void CreateSlot(int index)
        {
            var slot = _inventoryFactory.GetSlot();
            slot.CanDropHere = false;
            slot.transform.SetParent(_slotsParent);
            slot.transform.SetSiblingIndex(_slots.Count);
            slot.SetId(index);
            _slots.Add(slot);
        }

        private void GetAllItems()
        {
            _getAllButton.interactable = false;
            _chestsService.GetAllItems(_currentChest.Id);
            _slots.ForEach(slot => slot.ClearItem());
        }

        private void GetItem(UiItem uiItem)
        {
            _chestsService.GetItem(_currentChest.Id, uiItem.CurrentItem.Id);
            
            _currentChest.Items.Remove(uiItem.CurrentItem);
            _slots[uiItem.CurrentItem.Slot].ClearItem();
        }
    }
}
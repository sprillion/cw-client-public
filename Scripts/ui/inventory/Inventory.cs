using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using factories.inventory;
using infrastructure.services.chests;
using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using ui.popup;
using ui.quickPanel;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.inventory
{
    public class Inventory : Popup
    {
        [SerializeField] private List<Popup> _popupsToClose;
        [SerializeField] private GameObject _background;
        
        [SerializeField] private Transform _movingItemsParent;
        [SerializeField] private Transform _slotParent;
        [SerializeField] private QuickPanel _quickPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _closeBGButton;
        
        private IInventoryService _inventoryService;
        private IInventoryFactory _inventoryFactory;
        private IChestsService _chestsService;
        
        private readonly List<Slot> _slots = new List<Slot>();
        private readonly List<GameObject> _lockedSlots = new List<GameObject>();
        private readonly List<UiItem> _items = new List<UiItem>();

        private Button _addSlotButton;


        [Inject]
        public void Construct(IInventoryService inventoryService, IInventoryFactory inventoryFactory, IChestsService chestsService)
        {
            _inventoryService = inventoryService;
            _inventoryFactory = inventoryFactory;
            _chestsService = chestsService;
        }

        public override void Initialize()
        {
            _closeButton.onClick.AddListener(Back);
            _closeBGButton.onClick.AddListener(Back);

            _chestsService.OnGetAllItems += LaunchHide;
            _inventoryService.OnSlotUpdated += UpdateSlot;
            
            CreateSlots(_inventoryService.CountSlots, _inventoryService.MaxCountSlots);
            CreateAllItems();
        }

        public override void Show()
        {
            _quickPanel.EnableEditor();
            _background.SetActive(true);
            base.Show();
        }

        public override void Hide()
        {
            _quickPanel.DisableEditor();
            _popupsToClose.ForEach(popup => popup.Hide());
            _background.SetActive(false);
            base.Hide();
        }
        
        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(Hide);
            _closeBGButton.onClick.RemoveListener(Hide);
        }

        private void CreateSlots(int countSlots, int maxCountSlots)
        {
            for (int i = 0; i < countSlots; i++)
            {
                AddSlot(i);
            }
            
            AddAddSlotButton();
            
            for (int i = 0; i < maxCountSlots - countSlots - 1; i++)
            {
                AddLockedSlot();
            }
        }

        private void AddSlot(int id)
        {
            var slot = _inventoryFactory.GetSlot();
            slot.CanDropHere = true;
            _slots.Add(slot);
            slot.SetId(id);
            slot.transform.SetParent(_slotParent);
            slot.transform.SetSiblingIndex(id);
        }
        
        private void RemoveLockedSlot()
        {
            if (_lockedSlots.Count == 0) return;
            var slot = _lockedSlots[0];
            _lockedSlots.RemoveAt(0);
            Destroy(slot);
        }

        private void AddAddSlotButton()
        {
            _addSlotButton = _inventoryFactory.GetAddSlot();
            _addSlotButton.transform.SetParent(_slotParent);
            _addSlotButton.transform.SetAsLastSibling();
        }

        private void AddLockedSlot()
        {
            var slot = _inventoryFactory.GetLockedSlot();
            _lockedSlots.Add(slot);
            slot.transform.SetParent(_slotParent);
            slot.transform.transform.SetAsLastSibling();
        }

        private void CreateAllItems()
        {
            RemoveAllItems();
            _inventoryService.Items.ForEach(CreateItem);
        }

        private void CreateItem(Item item)
        {
            var uiItem = _inventoryFactory.GetItem();
            uiItem.Initialize(item, _movingItemsParent);
            uiItem.Draggable = true;
            _slots[item.Slot].SetItem(uiItem);
            _items.Add(uiItem);
        }

        private void RemoveAllItems()
        {
            _slots.ForEach(s => s.ClearItem());
        }

        private void UpdateSlot(Item item)
        {
            var uiItem = _items.FirstOrDefault(i => i.CurrentItem == item);
            if (uiItem == null)
            {
                CreateItem(item);
            }
        }

        private void LaunchHide()
        {
            HideWithDelay().Forget();
        }
        
        private async UniTaskVoid HideWithDelay()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            Hide();
        }
    }
}
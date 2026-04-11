using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using factories.inventory;
using infrastructure.factories;
using infrastructure.services.chests;
using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using Sirenix.Utilities;
using ui.inventory.character;
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
        [SerializeField] private CharacterPopup _characterPopup;
        [SerializeField] private PurchasingSlotPopup _purchasingSlotPopup;

        [SerializeField] private ItemInfo _itemInfo;
        
        [SerializeField] private Transform _movingItemsParent;
        [SerializeField] private Transform _slotParent;
        [SerializeField] private QuickPanel _quickPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _closeBGButton;
        
        private IInventoryService _inventoryService;
        private IInventoryFactory _inventoryFactory;
        private IChestsService _chestsService;

        private readonly Dictionary<int, Slot> _slots = new Dictionary<int, Slot>();
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

            _chestsService.OnGetAllItems += LaunchHide;
            _inventoryService.OnSlotUpdated += UpdateSlot;
            _inventoryService.OnSlotBuyed += OnSlotBuyed;
            
            CreateSlots(_inventoryService.CountSlots, _inventoryService.MaxCountSlots);
            CreateAllItems();
            
            _itemInfo.Initialize();
            _purchasingSlotPopup.Initialize();
        }

        public override void Show()
        {
            _quickPanel.EnableEditor();
            base.Show();
        }

        public override void Hide()
        {
            _quickPanel.DisableEditor();
            _popupsToClose.ForEach(popup => popup.Back());
            base.Hide();
        }
        
        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(Hide);
        }

        private void OnEnable()
        {
            _closeBGButton.gameObject.SetActive(true);
            _closeBGButton.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            _closeBGButton.gameObject.SetActive(false);
            _closeBGButton.onClick.RemoveListener(Back);
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
            
            AddQuickSlots();
            AddEquipSlots();
        }

        private void AddSlot(int id)
        {
            var slot = _inventoryFactory.GetSlot();
            slot.CanDropHere = true;
            _slots.Add(id, slot);
            slot.SetId(id);
            slot.transform.SetParent(_slotParent);
            slot.transform.SetSiblingIndex(id);
            slot.transform.localScale = Vector3.one;
        }

        private void AddQuickSlots()
        {
            foreach (var quickPanelSlot in _quickPanel.Slots)
            {
                _slots.Add(quickPanelSlot.Id, quickPanelSlot);
            }
        }
        
        private void AddEquipSlots()
        {
            foreach (var equipSlot in _characterPopup.EquipSlots)
            {
                _slots.Add(equipSlot.Id, equipSlot);
            }
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
            _addSlotButton.transform.localScale = Vector3.one;
            _addSlotButton.transform.SetAsLastSibling();
            _addSlotButton.onClick.RemoveAllListeners();
            _addSlotButton.onClick.AddListener(_purchasingSlotPopup.Show);
        }

        private void AddLockedSlot()
        {
            var slot = _inventoryFactory.GetLockedSlot();
            _lockedSlots.Add(slot);
            slot.transform.SetParent(_slotParent);
            slot.transform.transform.SetAsLastSibling();
            slot.transform.localScale = Vector3.one;
        }

        private void CreateAllItems()
        {
            RemoveAllItems();
            _inventoryService.Items.ForEach(CreateItem);
        }

        private void CreateItem(Item item)
        {
            var uiItem = Pool.Get<UiItem>();
            uiItem.Initialize(item, _movingItemsParent);
            uiItem.Draggable = true;
            _items.Add(uiItem);
            if (item.Slot >= 0)
            {
                _slots[item.Slot].SetItem(uiItem);
            }
        }

        private void RemoveItem(Item item)
        {
            _slots[item.Slot]?.ClearItem();
        }

        private void RemoveAllItems()
        {
            _slots.Values.ForEach(s => s.ClearItem());
        }

        private void UpdateSlot(Item item)
        {
            var uiItem = _items.FirstOrDefault(i => i.CurrentItem == item);
            if (uiItem == null)
            {
                CreateItem(item);
            }
            else if (item.Count == 0)
            {
                RemoveItem(item);
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

        private void OnSlotBuyed()
        {
            RemoveLockedSlot();
            AddSlot(_inventoryService.CountSlots - 1);
        }
    }
}
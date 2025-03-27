using System.Collections.Generic;
using System.Linq;
using factories.inventory;
using infrastructure.services.npc;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.npc
{
    public class ShopPopup : Popup
    {
        [SerializeField] private Button _allTab;
        [SerializeField] private Button[] _levelTabs;

        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _unselectedColor;

        [SerializeField] private Transform _itemsParent;
        
        private readonly List<ItemToPurchasingView> _items = new List<ItemToPurchasingView>();
        
        private INpcService _npcService;
        private IInventoryFactory _inventoryFactory;
        
        [Inject]
        public void Construct(INpcService npcService, IInventoryFactory inventoryFactory)
        {
            _npcService = npcService;
            _inventoryFactory = inventoryFactory;
            
            _allTab.onClick.AddListener(() => SetItems(0));
            for (var i = 0; i < _levelTabs.Length; i++)
            {
                var level = i + 1;
                _levelTabs[i].onClick.AddListener(() => SetItems(level));
            }
        }

        public override void Show()
        {
            SetTabs();
            SetItems(0);
            base.Show();
        }

        private void SetTabs()
        {
            for (var i = 0; i < _levelTabs.Length; i++)
            {
                _levelTabs[i].interactable = _npcService.CurrentAttitudeLevel >= i + 1;
            }
        }

        private void SetItems(int level)
        {
            ChangeColorButtons(level);
            
            ClearItems();
            
            var items = _npcService.GetShopItems();
            
            if (level == 0)
            {
                items = items.Where(item => item.NpcLevel <= _npcService.CurrentAttitudeLevel).ToList();
            }
            else
            {
                items = items.Where(item => item.NpcLevel == _npcService.CurrentAttitudeLevel).ToList();
            }
            
            items.ForEach(CreateItemView);
        }

        private void ChangeColorButtons(int level)
        {
            _allTab.targetGraphic.color = _unselectedColor;
            foreach (var levelTab in _levelTabs)
            {
                levelTab.targetGraphic.color = _unselectedColor;
            }

            if (level == 0)
            {
                _allTab.targetGraphic.color = _selectedColor;
            }
            else
            {
                _levelTabs[level - 1].targetGraphic.color = _selectedColor;
            }
        }

        private void CreateItemView(ItemToPurchasing item)
        {
            var view = _inventoryFactory.GetItemToPurchasing();
            view.SetItem(item);
            view.transform.SetParent(_itemsParent);
            _items.Add(view);
        }

        private void ClearItems()
        {
            _items.ForEach(item => item.Release());
            _items.Clear();
        }
    }
}
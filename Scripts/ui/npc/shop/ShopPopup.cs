using System.Collections.Generic;
using System.Linq;
using infrastructure.factories;
using infrastructure.services.npc;
using infrastructure.services.players;
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

        [SerializeField] private Image _backgroundImage;
        
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _unselectedColor;

        [SerializeField] private Transform _itemsParent;
        
        private readonly List<ItemToPurchasingView> _items = new List<ItemToPurchasingView>();
        
        private INpcService _npcService;
        private ICharacterService _characterService;
        
        [Inject]
        public void Construct(INpcService npcService, ICharacterService characterService)
        {
            _npcService = npcService;
            _characterService = characterService;
            
            _allTab.onClick.AddListener(() => SetItems(0));
            for (var i = 0; i < _levelTabs.Length; i++)
            {
                var level = i + 1;
                _levelTabs[i].onClick.AddListener(() => SetItems(level));
            }
        }

        public override void Show()
        {
            _backgroundImage.color = _npcService.CurrentNpcData.BackgroundColor;
            SetTabs();
            SetItems(0);
            _characterService.CurrentCharacter.CharacterStats.OnGoldChanged += UpdateItems;
            base.Show();
        }

        public override void Hide()
        {
            _characterService.CurrentCharacter.CharacterStats.OnGoldChanged -= UpdateItems;
            base.Hide();
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
            var view = Pool.Get<ItemToPurchasingView>();
            view.SetItem(item);
            view.SetParentPreserveScale(_itemsParent);
            _items.Add(view);
        }

        private void ClearItems()
        {
            _items.ForEach(item => item.Release());
            _items.Clear();
        }

        private void UpdateItems(int _)
        {
            foreach (var itemToPurchasingView in _items)
            {
                itemToPurchasingView.UpdateInfo();
            }
        }
    }
}
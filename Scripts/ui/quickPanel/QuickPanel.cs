using infrastructure.services.inventory;
using infrastructure.services.players;
using Sirenix.Utilities;
using ui.inventory.quickSlots;
using UnityEngine;
using Zenject;

namespace ui.quickPanel
{
    public class QuickPanel : MonoBehaviour
    {
        [SerializeField] private QuickSlot[] _slots;
        [SerializeField] private GameObject _indent;

        private IInventoryService _inventoryService;
        private ICharacterService _characterService;
        
        private QuickSlot _currentSelectedSlot;

        public QuickSlot[] Slots => _slots;
        
        [Inject]
        public void Construct(IInventoryService inventoryService, ICharacterService characterService)
        {
            _inventoryService = inventoryService;
            _characterService = characterService;
        }

        public void Initialize()
        {
            CreateSlots();
        }
        
        public void EnableEditor()
        {
            _slots.ForEach(slot => slot.DisableSelection());
        }

        public void DisableEditor()
        {
            _slots.ForEach(slot => slot.EnableSelection());
        }
        
        private void CreateSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].gameObject.SetActive(i < _inventoryService.CountQuickSlots);
                _slots[i].OnSelected += OnSlotSelected;
            }
            _indent.SetActive(_inventoryService.CountQuickSlots > 4);
            _slots[0].Select();
        }

        private void OnSlotSelected(QuickSlot quickSlot)
        {
            if (_currentSelectedSlot == quickSlot) return;
            _currentSelectedSlot?.Unselect();
            if (_currentSelectedSlot)
            {
                _currentSelectedSlot.OnItemChanged -= UpdateHandItem;
            }
            _currentSelectedSlot = quickSlot;
            _currentSelectedSlot.OnItemChanged += UpdateHandItem;
            UpdateHandItem();
        }

        private void UpdateHandItem()
        {
            _characterService.CurrentCharacter.HandItemsController.ChangeHandItem(_currentSelectedSlot.CurrentUiItem?.CurrentItem);
            _characterService.UpdateHandItem(_currentSelectedSlot.CurrentUiItem?.CurrentItem.Id ?? -1);
        }
    }
}
using infrastructure.services.chests;
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
        
        [Inject]
        public void Construct(IInventoryService inventoryService, ICharacterService characterService)
        {
            _inventoryService = inventoryService;
            _characterService = characterService;
        }

        public void Initialize()
        {
            CreateSlots();
            CreateItems();
        }

        public void EnableEditor()
        {
            _slots.ForEach(slot => slot.ShowDeleteButton());
        }

        public void DisableEditor()
        {
            _slots.ForEach(slot => slot.HideDeleteButton());
        }
        
        private void CreateSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].SetId(i);
                _slots[i].gameObject.SetActive(i < _inventoryService.CountQuickSlots);
                _slots[i].OnSelected += OnSlotSelected;
            }
            _indent.SetActive(_inventoryService.CountQuickSlots > 4);
            _slots[0].Select();
        }
        
        private void CreateItems()
        {
            _inventoryService.Items.ForEach(item =>
            {
                if (item.QuickSlot == null) return;
                _slots[(int)item.QuickSlot].SetItem(item, true);
            });
        }

        private void OnSlotSelected(int id)
        {
            _currentSelectedSlot?.Unselect();
            _currentSelectedSlot = _slots[id];
            _characterService.CurrentCharacter.HandItemsController.ChangeHandItem(_currentSelectedSlot.CurrentItem);
        }
    }
}
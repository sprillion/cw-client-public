using System;
using UnityEngine;
using UnityEngine.UI;

namespace ui.inventory.quickSlots
{
    public class QuickSlot : Slot
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private Image _selectImage;
        [SerializeField] private GameObject _selected;

        public event Action<QuickSlot> OnSelected; 

        private void OnEnable()
        {
            _selectButton.onClick.AddListener(Select);
        }

        private void OnDisable()
        {
            _selectButton.onClick.RemoveListener(Select);
        }
        
        public override bool DropCondition(UiItem uiItem)
        {
            return uiItem.CurrentItem.IsUsableItem;
        }

        public void EnableSelection()
        {
            _selectImage.raycastTarget = true;
            _selectButton.interactable = true;
        }
        
        public void DisableSelection()
        {
            _selectImage.raycastTarget = false;
            _selectButton.interactable = false;
        }
        
        public void Unselect()
        {
            _selected.SetActive(false);
        }

        public void Select()
        {
            OnSelected?.Invoke(this);
            _selected.SetActive(true);
        }
    }
}
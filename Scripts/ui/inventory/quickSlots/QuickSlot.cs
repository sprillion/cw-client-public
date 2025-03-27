using System;
using infrastructure.services.inventory.items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ui.inventory.quickSlots
{
    public class QuickSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private GameObject _selected;
        
        [SerializeField] private CooldownSlot _cooldownSlot;
        
        public Item CurrentItem { get; private set; }
        
        public int Id { get; private set; }

        public event Action<int> OnSelected; 

        private void OnEnable()
        {
            _selectButton.onClick.AddListener(Select);
            _deleteButton.onClick.AddListener(DeleteItem);
        }

        private void OnDisable()
        {
            _selectButton.onClick.RemoveListener(Select);
            _deleteButton.onClick.RemoveListener(DeleteItem);
        }

        public void OnDrop(PointerEventData eventData)
        {
            UiItem uiItem = eventData.pointerDrag.GetComponent<UiItem>();
            
            if (uiItem == null) return;
            if (CurrentItem != null)
            {
                DeleteItem();
            }
            
            SetItem(uiItem.CurrentItem);
        }

        public void SetId(int id)
        {
            Id = id;
        }

        public void SetItem(Item item, bool isLoad = false)
        {
            CurrentItem = item;
            CurrentItem.QuickSlot = Id;
            _itemIcon.sprite = CurrentItem.Data.Icon;
            _itemIcon.gameObject.SetActive(true);
            CurrentItem.OnUse += OnItemUsed;
            
            if (!isLoad)
            {
                ShowDeleteButton(); 
            }
        }

        public void Unselect()
        {
            _selected.SetActive(false);
        }

        public void Select()
        {
            OnSelected?.Invoke(Id);
            _selected.SetActive(true);
        }

        public void ShowDeleteButton()
        {
            if (CurrentItem == null) return;
            _deleteButton.gameObject.SetActive(true);
        }
        
        public void HideDeleteButton()
        {
            _deleteButton.gameObject.SetActive(false);
        }

        private void DeleteItem()
        {
            CurrentItem.OnUse -= OnItemUsed;
            CurrentItem.QuickSlot = null;
            CurrentItem = null;
            _itemIcon.sprite = null;
            _itemIcon.gameObject.SetActive(false);
            HideDeleteButton();
            _cooldownSlot.StopCooldown();
        }

        private void OnItemUsed(Item item)
        {
            _cooldownSlot.StartCooldown(item.Cooldown);
        }
    }
}
using System;
using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace ui.inventory
{
    public class Slot : MonoBehaviour, IDropHandler
    {
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public bool CanDropHere { get; set; } = true;
        [field: SerializeField] public Transform ParentToItems { get; set; }

        [SerializeField] protected CooldownSlot _cooldownSlot;
        
        private IInventoryService _inventoryService;
        public UiItem CurrentUiItem { get; private set; }

        public event Action OnItemChanged; 

        [Inject]
        public void Construct(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!CanDropHere) return;

            GameObject dropped = eventData.pointerDrag;
            UiItem uiItem = dropped.GetComponent<UiItem>();

            if (uiItem == null || !uiItem.Draggable || CurrentUiItem == uiItem || !DropCondition(uiItem)) return;
            
            SetItem(uiItem);
            OnItemChanged?.Invoke();
        }

        public virtual bool DropCondition(UiItem uiItem)
        {
            return true;
        }

        public void SetItem(UiItem uiItem)
        {
            if (CurrentUiItem == null)
            {
                CurrentUiItem = uiItem;
                CurrentUiItem.SetParent(ParentToItems);
                CurrentUiItem.transform.localScale = Vector3.one;
                CurrentUiItem.CurrentItem.Slot = Id;
                CurrentUiItem.CurrentItem.OnUse += OnItemUsed;
                CurrentUiItem.CurrentItem.OnCountChange += CheckCountItems;
                CurrentUiItem.CurrentItem.OnSlotChange += OnSlotChanged;
                CurrentUiItem.OnRemove += ResetCurrentItem;
            }
            else if (CurrentUiItem.CurrentItem.Id == uiItem.CurrentItem.Id)
            {
                MergeItems(uiItem);
            }
        }

        public void SetVisualItem(UiItem uiItem)
        {
            if (CurrentUiItem == null)
            {
                CurrentUiItem = uiItem;
                CurrentUiItem.SetParent(ParentToItems);
                CurrentUiItem.transform.localScale = Vector3.one;
            }
        }

        public void SetId(int id)
        {
            Id = id;
        }

        public void ClearItem()
        {
            CurrentUiItem?.Release();
            ResetCurrentItem();
            OnItemChanged?.Invoke();
        }

        private void ResetCurrentItem()
        {
            if (CurrentUiItem != null)
            {
                CurrentUiItem.CurrentItem.OnUse -= OnItemUsed;
                CurrentUiItem.CurrentItem.OnCountChange -= CheckCountItems;
                CurrentUiItem.CurrentItem.OnSlotChange -= OnSlotChanged;
                CurrentUiItem.OnRemove -= ResetCurrentItem;
            }

            CurrentUiItem = null;

            _cooldownSlot.StopCooldown();
        }

        private void MergeItems(UiItem uiItem)
        {
            _inventoryService.UpdateItemSlot(uiItem.CurrentItem.Slot, Id);
            if (CurrentUiItem.CurrentItem.Count + uiItem.CurrentItem.Count > CurrentUiItem.CurrentItem.StackSize)
            {
                var offset = CurrentUiItem.CurrentItem.StackSize - CurrentUiItem.CurrentItem.Count;
                CurrentUiItem.CurrentItem.Count = CurrentUiItem.CurrentItem.StackSize;
                uiItem.CurrentItem.Count -= offset;
            }
            else
            {
                CurrentUiItem.CurrentItem.Count += uiItem.CurrentItem.Count;
                uiItem.CurrentItem.Count = 0;
            }
        }

        private void OnItemUsed(Item item)
        {
            _cooldownSlot.StartCooldown(item.Cooldown);
        }

        private void CheckCountItems()
        {
            if (CurrentUiItem.CurrentItem.Count <= 0)
            {
                ClearItem();
            }
        }

        private void OnSlotChanged(int oldSlot, int newSlot)
        {
            ResetCurrentItem();
            OnItemChanged?.Invoke();
        }
    }
}
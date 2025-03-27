using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace ui.inventory
{
    public class Slot : MonoBehaviour, IDropHandler
    {
        [field: SerializeField] public bool CanDropHere { get; set; } = true;
        [field: SerializeField] public Transform ParentToItems { get; set; }

        [SerializeField] private CooldownSlot _cooldownSlot;
        public int Id { get; private set; }

        public UiItem CurrentUiItem { get; set; }

        private IInventoryService _inventoryService;

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

            if (uiItem == null || !uiItem.Draggable || CurrentUiItem == uiItem) return;
            
            SetItem(uiItem);
        }

        public void SetItem(UiItem uiItem)
        {
            if (CurrentUiItem == null)
            {
                CurrentUiItem = uiItem;
                CurrentUiItem.SetParent(ParentToItems);
                CurrentUiItem.CurrentItem.Slot = Id;
                CurrentUiItem.CurrentItem.OnUse += OnItemUsed;
                CurrentUiItem.CurrentItem.OnCountChange += CheckCountItems;
                CurrentUiItem.CurrentItem.OnSlotChange += OnSlotChanged;
            }
            else if (CurrentUiItem.CurrentItem.Id == uiItem.CurrentItem.Id)
            {
                MergeItems(uiItem);
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
        }

        private void ResetCurrentItem()
        {
            if (CurrentUiItem != null)
            {
                CurrentUiItem.CurrentItem.OnUse -= OnItemUsed;
                CurrentUiItem.CurrentItem.OnCountChange -= CheckCountItems;
                CurrentUiItem.CurrentItem.OnSlotChange -= OnSlotChanged;
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
        }
    }
}
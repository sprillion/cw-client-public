using I2.Loc;
using infrastructure.services.inventory;
using infrastructure.services.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace ui.inventory
{
    public class RemoveItemObject : MonoBehaviour, IDropHandler
    {
        private IInventoryService _inventoryService;
        private IUiService _uiService;

        [Inject]
        public void Construct(IInventoryService inventoryService, IUiService uiService)
        {
            _inventoryService = inventoryService;
            _uiService = uiService;
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject dropped = eventData.pointerDrag;
            UiItem uiItem = dropped.GetComponent<UiItem>();
            
            if (uiItem == null || !uiItem.Draggable) return;

            RemoveItem(uiItem);
        }

        private void RemoveItem(UiItem uiItem)
        {
            _uiService.ConfirmView.SetConfirmInfo(LocalizationManager.GetTranslation("Confirm/Description/RemoveItem"),
                () =>
                {
                    _inventoryService.RemoveItem(uiItem.CurrentItem.Slot);
                    uiItem.Release();
                });
            _uiService.ConfirmView.Show();
        }
    }
}
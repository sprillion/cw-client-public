using infrastructure.services.inventory.items;
using ui.inventory;
using ui.npc;
using UnityEngine;
using UnityEngine.UI;

namespace factories.inventory
{
    public interface IInventoryFactory
    {
        Slot GetSlot();
        Button GetAddSlot();
        GameObject GetLockedSlot();
        ItemData GetItemData(int id);
        UiItem GetItem();
        ItemToPurchasingView GetItemToPurchasing();
    }
}
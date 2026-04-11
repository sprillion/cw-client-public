using infrastructure.services.inventory.items;
using ui.inventory;
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
    }
}
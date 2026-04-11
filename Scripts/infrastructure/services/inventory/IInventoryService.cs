using System;
using System.Collections.Generic;
using infrastructure.services.inventory.items;
using network;

namespace infrastructure.services.inventory
{
    public interface IInventoryService : IReceiver
    { 
        List<Item> Items { get; }

        int CountSlots { get; }
        int MaxCountSlots { get; }
        int CountQuickSlots { get; }
        
        event Action OnInventoryLoaded;
        event Action<Item> OnSlotUpdated;
        event Action OnSlotBuyed;
        event Action<PurchasingSlot> OnPurchsingSlotInfo;

        void UpdateItemSlot(int oldSlot, int newSlot);
        void RemoveItem(int slot);
        bool HaveItems(int itemId, int count);
        int CountItems(int itemId);
        void UseItem(Item item);
        void GetPurchasingInfo();
        void BuyNewSlot();
    }
}
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

        void UpdateItemSlot(int oldSlot, int newSlot);
    }
}
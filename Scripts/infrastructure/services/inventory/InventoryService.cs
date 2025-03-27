using System;
using System.Collections.Generic;
using System.Linq;
using factories.inventory;
using infrastructure.services.inventory.items;
using network;
using UnityEngine;

namespace infrastructure.services.inventory
{
    public class InventoryService : IInventoryService
    {
        public enum FromServerMessage : byte
        {
            AllInventoryInfo,
            UpdateSlot,
        }
        
        public enum FromClientMessages : byte
        {
            RemoveItem,
            UpdateSlot,
            UpdateQuickSlot,
            UseItem,
        }

        private readonly IInventoryFactory _inventoryFactory;
        private readonly INetworkManager _networkManager;

        public List<Item> Items { get; } = new List<Item>();

        public int CountSlots { get; private set; }
        public int MaxCountSlots { get; private set; }
        public int CountQuickSlots { get; private set; }
        
        public event Action OnInventoryLoaded;
        public event Action<Item> OnSlotUpdated;
        
        public InventoryService(IInventoryFactory inventoryFactory, INetworkManager networkManager)
        {
            _inventoryFactory = inventoryFactory;
            _networkManager = networkManager;
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            switch (type)
            {
                case FromServerMessage.AllInventoryInfo:
                    AllInventoryInfo(message);
                    break;
                case FromServerMessage.UpdateSlot:
                    UpdateSlotFromServer(message);
                    break;
            }
        }

        private void AllInventoryInfo(Message message)
        {
            LoadAllSlots(message);
            LoadAllItems(message);
            OnInventoryLoaded?.Invoke();
        }

        private void LoadAllSlots(Message message)
        {
            CountSlots = message.GetInt();
            MaxCountSlots = message.GetInt();
            CountQuickSlots = message.GetInt();
        }
        
        private void LoadAllItems(Message message)
        {
            Items.Clear();
            var countItems = message.GetInt();
            for (int i = 0; i < countItems; i++)
            {
                CreateItem(message);
            }
        }
        
        private int? GetQuickSlot(Message message)
        {
            var quickSlot = message.GetInt();
            return quickSlot < 0 ? null : quickSlot;
        }

        private void CreateItem(Message message)
        {
            var id = message.GetInt();
            var item = new Item()
            {
                Id = id,
                Count = message.GetInt(),
                StackSize = message.GetInt(),
                Cooldown = message.GetByte(),
                Slot = message.GetInt(),
                QuickSlot = GetQuickSlot(message),
                Data = _inventoryFactory.GetItemData(id)
            };
            Items.Add(item);

            item.OnSlotChange += UpdateItemSlot;
            item.OnQuickSlotChange += UpdateQuickSlot;
        }

        public void UpdateItemSlot(int oldSlot, int newSlot)
        {
            var message = new Message(ClientToServerId.Inventory);
            message.AddByte((byte)FromClientMessages.UpdateSlot);
            message.AddInt(oldSlot);
            message.AddInt(newSlot);
            _networkManager.SendMessage(message);
        }
        
        private void UpdateQuickSlot(int quickSlot, int itemSlot)
        {
            var message = new Message(ClientToServerId.Inventory);
            message.AddByte((byte)FromClientMessages.UpdateQuickSlot);
            message.AddInt(quickSlot);
            if (quickSlot == null)
            {
                message.AddInt(-1);
            }
            else
            {
                message.AddInt(itemSlot);
            }
            _networkManager.SendMessage(message);
        }

        private void UpdateSlotFromServer(Message message)
        {
            var slot = message.GetInt();
            var itemId = message.GetInt();
            var itemCount = message.GetInt();
            var stackSize = message.GetInt();

            var item = Items.FirstOrDefault(i => i.Slot == slot);

            if (item == null)
            {
                item = new Item()
                {
                    Id = itemId,
                    Count = itemCount,
                    StackSize = stackSize,
                    Slot = slot,
                    QuickSlot = null,
                    Data = _inventoryFactory.GetItemData(itemId)
                };
                Items.Add(item);
            }
            else
            {
                item.Id = itemId;
                item.Count = itemCount;
                item.Data = _inventoryFactory.GetItemData(itemId);
            }
            
            OnSlotUpdated?.Invoke(item);
        }
    }
}
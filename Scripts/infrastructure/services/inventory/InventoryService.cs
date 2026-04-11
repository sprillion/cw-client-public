using System;
using System.Collections.Generic;
using System.Linq;
using factories.inventory;
using infrastructure.services.npc;
using network;
using tools;
using ui.inventory.equipSlot;
using Item = infrastructure.services.inventory.items.Item;

namespace infrastructure.services.inventory
{
    public class InventoryService : IInventoryService
    {
        public enum FromClientMessages : byte
        {
            RemoveItem,
            UpdateSlot,
            UseItem,
            PurchasingSlotInfo,
            BuySlot,
        }
        public enum FromServerMessage : byte
        {
            AllInventoryInfo,
            UpdateSlot,
            PurchasingSlotInfo,
            BuySlotResult,
        }

        private readonly IInventoryFactory _inventoryFactory;
        private readonly INetworkManager _networkManager;

        public List<Item> Items { get; } = new List<Item>();

        public int CountSlots { get; private set; }
        public int MaxCountSlots { get; private set; }
        public int CountQuickSlots { get; private set; }
        
        public event Action OnInventoryLoaded;
        public event Action<Item> OnSlotUpdated;
        public event Action OnSlotBuyed;
        public event Action<PurchasingSlot> OnPurchsingSlotInfo;
        
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
                case FromServerMessage.PurchasingSlotInfo:
                    SetPurchasingSlotInfo(message);
                    break;
                case FromServerMessage.BuySlotResult:
                    SetNewBuySlot(message);
                    break;
            }
        }
        
        public void UpdateItemSlot(int oldSlot, int newSlot)
        {
            if (oldSlot == newSlot) return;
            var message = new Message(MessageType.Inventory);
            message.AddByte((byte)FromClientMessages.UpdateSlot);
            message.AddInt(oldSlot);
            message.AddInt(newSlot);
            _networkManager.SendMessage(message);
        }

        public void RemoveItem(int slot)
        {
            var message = new Message(MessageType.Inventory)
                .AddByte(FromClientMessages.RemoveItem.ToByte())
                .AddInt(slot);
            
            _networkManager.SendMessage(message);
        }

        public bool HaveItems(int itemId, int count)
        {
            var currentCount = 0;
            
            foreach (var item in Items)
            {
                if (item.Id != itemId) continue;

                currentCount += item.Count;
                
                if (currentCount >= count) return true;
            }

            return false;
        }

        public int CountItems(int itemId)
        {
            var currentCount = 0;
            
            foreach (var item in Items)
            {
                if (item.Id != itemId) continue;

                currentCount += item.Count;
            }

            return currentCount;
        }

        public void UseItem(Item item)
        {
            var message = new Message(MessageType.Inventory)
                .AddByte(FromClientMessages.UseItem.ToByte())
                .AddInt(item.Id)
                .AddInt(item.Slot);

            _networkManager.SendMessage(message);
        }

        public void GetPurchasingInfo()
        {
            var message = new Message(MessageType.Inventory)
                .AddByte(FromClientMessages.PurchasingSlotInfo.ToByte());
            
            _networkManager.SendMessage(message);
        }

        public void BuyNewSlot()
        {
            var message = new Message(MessageType.Inventory)
                .AddByte(FromClientMessages.BuySlot.ToByte());
            
            _networkManager.SendMessage(message);
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

        private void CreateItem(Message message)
        {
            var id = message.GetInt();
            var item = new Item()
            {
                Id = id,
                Count = message.GetInt(),
                StackSize = message.GetInt(),
                Slot = message.GetInt(),
                IsEquipeItem = message.GetBool(),
                IsUsableItem = message.GetBool(),
            };
            if (item.IsUsableItem)
            {
                item.Cooldown = message.GetInt();
            }
            if (item.IsEquipeItem)
            {
                item.EquipSlotType = message.GetByteEnum<EquipSlotType>();
            }
            
            Items.Add(item);

            item.OnSlotChange += UpdateItemSlot;
        }

        private void UpdateSlotFromServer(Message message)
        {
            var itemId = message.GetInt();
            var itemCount = message.GetInt();
            var stackSize = message.GetInt();
            var slot = message.GetInt();
            var isEquipedItem = message.GetBool();
            var isUsableItem = message.GetBool();
            float cooldown = 0;
            if (isUsableItem)
            {
                cooldown = message.GetInt();
            }
            EquipSlotType equipSlotType = EquipSlotType.None;
            if (isEquipedItem)
            {
                equipSlotType = message.GetByteEnum<EquipSlotType>();
            }

            var item = Items.FirstOrDefault(i => i.Slot == slot);

            if (item == null)
            {
                item = new Item()
                {
                    Id = itemId,
                    Count = itemCount,
                    StackSize = stackSize,
                    Slot = slot,
                    IsEquipeItem = isEquipedItem,
                    IsUsableItem = isUsableItem,
                    Cooldown = cooldown,
                    EquipSlotType = equipSlotType,
                };
                Items.Add(item);
                item.OnSlotChange += UpdateItemSlot;
            }
            else
            {
                item.Id = itemId;
                item.Count = itemCount;
                item.IsEquipeItem = isEquipedItem;
                item.IsUsableItem = isUsableItem;
                item.Cooldown = cooldown;
            }

            if (item.Count == 0)
            {
                Items.Remove(item);
            }
            
            OnSlotUpdated?.Invoke(item);
        }

        private void SetPurchasingSlotInfo(Message message)
        {
            var purchasingSlot = new PurchasingSlot
            {
                CanBuy = message.GetBool()
            };

            if (purchasingSlot.CanBuy)
            {
                purchasingSlot.Price = message.GetInt();
                purchasingSlot.CurrencyType = message.GetByteEnum<CurrencyType>();
            }
            
            OnPurchsingSlotInfo?.Invoke(purchasingSlot);
        }

        private void SetNewBuySlot(Message message)
        {
            CountSlots = message.GetInt();
            OnSlotBuyed?.Invoke();
        }
    }
}
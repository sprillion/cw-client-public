using System;
using System.Collections.Generic;
using System.Linq;
using environment.chests;
using factories.inventory;
using infrastructure.factories;
using infrastructure.factories.environment;
using infrastructure.services.inventory.items;
using network;
using UnityEngine;

namespace infrastructure.services.chests
{
    public class ChestsService : IChestsService
    {
        private readonly List<ChestFromMob> _chestsFromMobs = new List<ChestFromMob>();

        public enum FromServerMessage : byte
        {
            Create,
            Remove,
            Items,
            GetAllItemsStatus
        }

        private enum FromClientMessages : byte
        {
            Open,
            GetAllItems,
            GetItem,
        }

        public enum ChestType : byte
        {
            DroppedItems,
            Ads,
        }

        private readonly INetworkManager _networkManager;
        private readonly IEnvironmentFactory _environmentFactory;
        private readonly IInventoryFactory _inventoryFactory;
        
        public event Action OnGetAllItems;
        
        
        public ChestsService(INetworkManager networkManager, IEnvironmentFactory environmentFactory,
            IInventoryFactory inventoryFactory)
        {
            _networkManager = networkManager;
            _environmentFactory = environmentFactory;
            _inventoryFactory = inventoryFactory;
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.Create:
                    CreateChest(message);
                    break;
                case FromServerMessage.Remove:
                    RemoveChest(message);
                    break;
                case FromServerMessage.Items:
                    SetItems(message);
                    break;
                case FromServerMessage.GetAllItemsStatus:
                    AllItemsGeted();
                    break;
            }
        }

        public void OpenChest(ushort chestId)
        {
            var message = new Message(MessageType.Chest);
            message.AddByte((byte)FromClientMessages.Open)
                .AddUShort(chestId);

            _networkManager.SendMessage(message);
        }

        public void GetAllItems(ushort chestId)
        {
            var message = new Message(MessageType.Chest);
            message.AddByte((byte)FromClientMessages.GetAllItems)
                .AddUShort(chestId);

            _networkManager.SendMessage(message);
        }

        public void GetItem(ushort chestId, int itemId)
        {
            var message = new Message(MessageType.Chest);
            message.AddByte((byte)FromClientMessages.GetItem)
                .AddUShort(chestId)
                .AddInt(itemId);

            _networkManager.SendMessage(message);
        }
        
        private void CreateChest(Message message)
        {
            var chestType = (ChestType)message.GetByte();

            switch (chestType)
            {
                case ChestType.DroppedItems:
                    CreateChestFromMob(message);
                    break;
                case ChestType.Ads:
                    break;
            }
        }

        private void CreateChestFromMob(Message message)
        {
            var chest = Pool.Get<ChestFromMob>();
            chest.Id = message.GetUShort();
            var position = message.GetVector3();
            chest.transform.position = position;
            _chestsFromMobs.Add(chest);
        }

        private void RemoveChest(Message message)
        {
            var chestId = message.GetUShort();

            var chest = _chestsFromMobs.FirstOrDefault(c => c.Id == chestId);
            chest?.Release();
            _chestsFromMobs.Remove(chest);
        }

        private void SetItems(Message message)
        {
            var chestId = message.GetUShort();
            var chest = _chestsFromMobs.FirstOrDefault(c => c.Id == chestId);
            if (chest == null) return;

            var itemsCount = message.GetInt();
            var items = new List<Item>(); ;
            for (int i = 0; i < itemsCount; i++)
            {
                var item = CreateItem(message);
                items.Add(item);
            }
            
            chest.SetItems(items);
        }

        private Item CreateItem(Message message)
        {
            var id = message.GetInt();
            var item = new Item()
            {
                Id = id,
                Count = message.GetInt(),
            };
            return item;
        }

        private void AllItemsGeted()
        {
            OnGetAllItems?.Invoke();
        }
    }
}
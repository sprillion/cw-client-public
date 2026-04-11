using System.Collections.Generic;
using System.Linq;
using infrastructure.services.inventory.items;
using ui.inventory;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace factories.inventory
{
    public class InventoryFactory : IInventoryFactory
    {
        private const string ItemsDataPath = "Data/Inventory/";

        private const string SlotPath = "Prefabs/Ui/Inventory/Slot";
        private const string AddSlotPath = "Prefabs/Ui/Inventory/AddSlot";
        private const string LockedSlotPath = "Prefabs/Ui/Inventory/LockedSlot";

        private const string ItemPath = "Prefabs/Ui/Inventory/Item";

        private readonly DiContainer _container;

        private readonly Slot _slotPrefab;
        private readonly Button _addSlotPrefab;
        private readonly GameObject _lockedSlotPrefab;

        private readonly Dictionary<int, ItemData> _itemsData;

        public InventoryFactory(DiContainer container)
        {
            _container = container;

            _slotPrefab = Resources.Load<Slot>(SlotPath);
            _addSlotPrefab = Resources.Load<Button>(AddSlotPath);
            _lockedSlotPrefab = Resources.Load<GameObject>(LockedSlotPath);
            _itemsData = Resources.LoadAll<ItemData>(ItemsDataPath).ToDictionary(data => data.Id, data => data);
        }

        public Slot GetSlot()
        {
            return _container.InstantiatePrefabForComponent<Slot>(_slotPrefab);
        }

        public Button GetAddSlot()
        {
            return _container.InstantiatePrefabForComponent<Button>(_addSlotPrefab);
        }

        public GameObject GetLockedSlot()
        {
            return _container.InstantiatePrefab(_lockedSlotPrefab);
        }

        public ItemData GetItemData(int id)
        {
            return _itemsData.GetValueOrDefault(id);
        }
    }
}
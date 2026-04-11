using System;
using System.Collections.Generic;
using System.Linq;
using ui.inventory.equipSlot;
using UnityEngine;

namespace infrastructure.services.inventory.items
{
    public class Item
    {
        private static Dictionary<int, ItemData> _itemsData;
        
        private int _count;
        private int _slot;

        private int _id;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                Data = _itemsData.GetValueOrDefault(value);
            }
        }

        public int Count
        {
            get => _count;
            set
            {
                if (_count == value) return;
                _count = value;
                OnCountChange?.Invoke();
            }
        }

        public int StackSize { get; set; }

        public int Slot
        {
            get => _slot;
            set
            {
                if (_slot == value) return;
                var oldValue = _slot;
                _slot = value;
                OnSlotChange?.Invoke(oldValue, _slot);
            }
        }
        
        public bool IsUsableItem { get; set; }
        public bool IsEquipeItem { get; set; }
        // public bool ShowCount { get; set; }
        
        public EquipSlotType EquipSlotType { get; set; }
        public float Cooldown { get; set; }
        public float CurrentCooldown { get; private set; }

        public bool IsCooldown => CurrentCooldown > Time.time;

        public ItemData Data { get; private set; }

        public event Action OnCountChange;
        public event Action<int, int> OnSlotChange;
        public event Action<Item> OnUse;

        public Item()
        {
            if (_itemsData != null) return;
            _itemsData = Resources.LoadAll<ItemData>("Data/Inventory").ToDictionary(data => data.Id, data => data);
        }

        public void Use()
        {
            CurrentCooldown = Time.time + Cooldown;
            OnUse?.Invoke(this);
        }
    }
}
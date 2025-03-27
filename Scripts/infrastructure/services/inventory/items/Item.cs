using System;
using UnityEngine;

namespace infrastructure.services.inventory.items
{
    public class Item
    {
        private int _count;
        private int _slot;
        private int? _quickSlot = null;
        
        public int Id { get; set; }

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

        public int? QuickSlot
        {
            get => _quickSlot;
            set
            {
                if (_quickSlot == value) return;
                int oldValue = -1;
                if (value == null)
                {
                    oldValue = (int)_quickSlot;
                }
                _quickSlot = value;
                OnQuickSlotChange?.Invoke(_quickSlot ?? oldValue, _quickSlot == null ? -1 : Slot);
            }
        }
        
        public float Cooldown { get; set; }
        public float CurrentCooldown { get; private set; }

        public bool IsCooldown => CurrentCooldown > Time.time;

        public ItemData Data { get; set; }

        public event Action OnCountChange;
        public event Action<int, int> OnSlotChange;
        public event Action<int, int> OnQuickSlotChange;
        public event Action<Item> OnUse;

        public void Use()
        {
            CurrentCooldown = Time.time + Cooldown;
            OnUse?.Invoke(this);
        }
    }
}
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace infrastructure.services.inventory.items
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Data/Inventory/ItemData")]
    public class ItemData : ScriptableObject
    {
        [field:SerializeField, OnValueChanged(nameof(AddItemToDictionary))] public int Id { get; private set; }
        [field:SerializeField] public ItemType ItemType { get; private set; }

        [field:PreviewField(100, Alignment = ObjectFieldAlignment.Left)]
        [field:SerializeField, OnValueChanged(nameof(AddItemToDictionary))] public Sprite Icon{ get; private set; }

#if UNITY_EDITOR
                
        public static event Action<ItemData> OnItemChanged;
        
        private void AddItemToDictionary()
        {
            OnItemChanged?.Invoke(this);
        }
#endif

    }
}
using System;
using ArcaneOnyx.ScriptableObjectDatabase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace infrastructure.services.inventory.items
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Data/Inventory/ItemData")]
    public class ItemData : ScriptableObject, IScriptableItem
    {
        [field: SerializeField]
#if UNITY_EDITOR
        [field: OnValueChanged(nameof(AddItemToDictionary))]
#endif
        public int Id { get; private set; }

        [field: SerializeField] public ItemType ItemType { get; private set; }

        [field: PreviewField(100, Alignment = ObjectFieldAlignment.Left)]
        [field: SerializeField]
#if UNITY_EDITOR
        [field: OnValueChanged(nameof(AddItemToDictionary))]
#endif
        public Sprite Icon { get; private set; }
        
        public string Name
        {
            get { return $"{Id} {$"Items/{Id}".Loc()} ({ItemType})"; }
            set { name = Name; }
        }

        public string CustomId { get; set; }
        public int SortId => Id;
        public void OnSave()
        {
            name = Name;
        }

#if UNITY_EDITOR

        public static event Action<ItemData> OnItemChanged;

        private void AddItemToDictionary()
        {
            OnItemChanged?.Invoke(this);
        }
#endif
    }
}
using System.Collections.Generic;
using infrastructure.services.inventory.items;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace tools
{
    [CreateAssetMenu(fileName = "AutoIconDictionary", menuName = "Data/AutoIconDictionary")]
    public class AutoIconDictionary : SerializedScriptableObject
    {
        [DictionaryDrawerSettings(KeyLabel = "Id", ValueLabel = "Icon")]
        [OdinSerialize] public Dictionary<int, Sprite> Items = new Dictionary<int, Sprite>();

#if UNITY_EDITOR
        private void OnEnable()
        {
            ItemData.OnItemChanged += AddIcon;
        }

        private void OnDisable()
        {
            ItemData.OnItemChanged -= AddIcon;
        }

        private void OnDestroy()
        {
            ItemData.OnItemChanged -= AddIcon;
        }

        private void AddIcon(ItemData itemData)
        {
            Items[itemData.Id] = itemData.Icon;
            EditorUtility.SetDirty(this);
        }

        [Button]
        private void UpdateItems()
        {
            Items.Clear();
            var datas = Resources.LoadAll<ItemData>("Data/Inventory");
            
            foreach (var itemData in datas)
            {
                Items.Add(itemData.Id, itemData.Icon);
            }

            Resources.UnloadUnusedAssets();
            EditorUtility.SetDirty(this);
        }
#endif

    }
    
    
}
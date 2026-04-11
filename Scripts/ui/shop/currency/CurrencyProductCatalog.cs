using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ui.shop.currency
{
    [CreateAssetMenu(fileName = "CurrencyProductCatalog", menuName = "Data/Catalog/CurrencyProductCatalog")]
    public class CurrencyProductCatalog : SerializedScriptableObject
    {
        [OdinSerialize] public Dictionary<string, Sprite> Sprites = new();
        public Sprite DefaultSprite;

        public Sprite GetSprite(string productId)
        {
            return Sprites.GetValueOrDefault(productId, DefaultSprite);
        }
    }
}

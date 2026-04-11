using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ui.tools
{
    [CreateAssetMenu(fileName = "SpriteCatalog", menuName = "Data/Catalog/IdToSpriteCatalog")]
    public class IdToSpriteCatalog: SerializedScriptableObject
    {
        public Dictionary<int, Sprite> Sprites = new Dictionary<int, Sprite>();

        public Sprite DefaultSprite;

        public Sprite GetSprite(int id)
        {
            return Sprites.GetValueOrDefault(id, DefaultSprite);
        }
    }
}
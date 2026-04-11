using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ui.tools
{
    [CreateAssetMenu(fileName = "SpriteCatalog", menuName = "Data/Catalog/SpriteCatalog")]
    public class SpriteCatalog : SerializedScriptableObject
    {
        public Dictionary<Enum, Sprite> Sprites = new Dictionary<Enum, Sprite>();

        public Sprite DefaultSprite;


        public Sprite GetSprite(Enum type)
        {
            return Sprites.GetValueOrDefault(type, DefaultSprite);
        }
    }
}
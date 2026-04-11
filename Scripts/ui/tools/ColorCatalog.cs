using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ui.tools
{
    [CreateAssetMenu(fileName = "ColorCatalog", menuName = "Data/Catalog/ColorCatalog")]
    public class ColorCatalog : SerializedScriptableObject
    {
        public Dictionary<Enum, Color> Colors = new Dictionary<Enum, Color>();

        public Color DefaultColor = Color.white;


        public Color GetColor(Enum type)
        {
            return Colors.GetValueOrDefault(type, DefaultColor);
        }
    }
}
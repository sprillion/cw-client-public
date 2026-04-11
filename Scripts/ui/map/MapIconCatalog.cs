using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ui.map
{
    [CreateAssetMenu(fileName = "MapIconCatalog", menuName = "Data/Map/MapIconCatalog")]
    public class MapIconCatalog : SerializedScriptableObject
    {
        public Dictionary<MapIconType, MapIconData> Icons = new Dictionary<MapIconType, MapIconData>();

        public MapIconData Get(MapIconType type)
        {
            return Icons.GetValueOrDefault(type);
        }
    }
}

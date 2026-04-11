using UnityEngine;

namespace ui.map
{
    [CreateAssetMenu(fileName = "MapIconData", menuName = "Data/Map/MapIconData")]
    public class MapIconData : ScriptableObject
    {
        public Sprite Icon;
        public Color Color = Color.white;
        public Vector2 Size = new Vector2(32f, 32f);
    }
}

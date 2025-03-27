using UnityEngine;

namespace infrastructure.services.npc
{
    [CreateAssetMenu(fileName = "NpcData", menuName = "Data/Npc/Npc")]
    public class NpcData : ScriptableObject
    {
        public NpcType NpcType;
        
        public Material Material;

        public Sprite AvatarIcon;
        public Color BackgroundColor;
        
    }
}
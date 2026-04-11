using UnityEngine;

namespace character
{
    [CreateAssetMenu(fileName = "SkinData", menuName = "Data/Character/SkinData")]
    public class SkinData : ScriptableObject
    {
        public int Id;
        public Sprite AvatarIcon;
        public Texture2D Texture;
    }
}
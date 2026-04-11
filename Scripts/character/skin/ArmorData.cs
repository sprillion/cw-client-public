using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace character
{
    [CreateAssetMenu(fileName = "ArmorData", menuName = "Data/Character/Armor")]
    public class ArmorData : SerializedScriptableObject
    {
        [DictionaryDrawerSettings(KeyLabel = "Armor Type", ValueLabel = "Armor Materials")]
        [OdinSerialize] public Dictionary<ArmorType, ArmorTextures> ArmorTexturesMap = new Dictionary<ArmorType, ArmorTextures>();

        public Texture2D GetTexture(ArmorType armorType, ArmorPlaceType armorPlaceType)
        {
            return armorPlaceType switch
            {
                ArmorPlaceType.Head => ArmorTexturesMap[armorType].Head,
                ArmorPlaceType.Body => ArmorTexturesMap[armorType].Body,
                ArmorPlaceType.Legs => ArmorTexturesMap[armorType].Legs,
                ArmorPlaceType.Foot => ArmorTexturesMap[armorType].Foot,
                _ => null
            };
        }
    }

    [Serializable]
    public class ArmorTextures
    {
        public Texture2D Head;
        public Texture2D Body;
        public Texture2D Legs;
        public Texture2D Foot;
        
        public Texture2D NormalMap;
    }
}
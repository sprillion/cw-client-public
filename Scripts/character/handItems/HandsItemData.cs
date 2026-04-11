using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace character.handItems
{
    [CreateAssetMenu(fileName = "HandsItemData", menuName = "Data/Character/HandsItemData")]
    public class HandsItemData : SerializedScriptableObject
    {
        [OdinSerialize] public Dictionary<int, SwordTextures> SwordsTexturesMap = new Dictionary<int, SwordTextures>();

        public SwordTextures GetSwordTexture(int itemId)
        {
            return SwordsTexturesMap[itemId];
        }
    }
    
    [Serializable]
    public class SwordTextures
    {
        public Texture2D Main;
        public Texture2D NormalMap;
        public float NormalScale;
    }
}
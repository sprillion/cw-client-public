using System;
using UnityEngine;

namespace character
{
    [CreateAssetMenu(fileName = "CapeData", menuName = "Data/Character/CapeData")]
    public class CapeData : ScriptableObject
    {
        public int Id;
        public Texture2D CapeTexture;
        public Color CapeColor = Color.white;
        public Texture2D EmblemTexture;
        public Color EmblemColor = Color.white;
        public Texture2D EmblemMaskTexture;

        [NonSerialized] private Sprite _capeSprite;
        [NonSerialized] private Sprite _emblemSprite;

        public Sprite CapeSprite => _capeSprite ??= CapeTexture != null
            ? Sprite.Create(CapeTexture, new Rect(Vector2.zero, new Vector2(256, 256)), new Vector2(0.5f, 0.5f))
            : null;

        public Sprite EmblemSprite => _emblemSprite ??= EmblemTexture != null
            ? Sprite.Create(EmblemTexture, new Rect(Vector2.zero, new Vector2(256, 256)), new Vector2(0.5f, 0.5f))
            : null;
    }
}

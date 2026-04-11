using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockTextureData", menuName = "Data/Map/BlockTextureData")]
public class BlockTextureData : ScriptableObject
{
    [SerializeField] private float _textureSize;
    [SerializeField] private float _blockSize;
    [SerializeField] private List<BlockData> _blockTextures;
    public float TextureSize => _textureSize;
    public float BlockSize => _blockSize;
    public int CountBlocksInLine => (int)(_textureSize / _blockSize);
        
    public List<BlockData> BlockDatas => _blockTextures;
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockTextureData", menuName = "Data/Map/BlockTextureData")]
public class BlockTextureData : ScriptableObject
{
    [SerializeField] private float _textureSize;
    [SerializeField] private float _blockSize;
    [SerializeField] private Texture2D _fullTexture;
    [SerializeField] private List<BlockTexture> _blockTextures;
    public float TextureSize => _textureSize;
    public float BlockSize => _blockSize;
    public int CountBlocksInLine => (int)(_textureSize / _blockSize);

    public Texture2D FullTexture => _fullTexture;
        
    public BlockTexture GetBlockTexture(BlockType blockType)
    {
        return _blockTextures.FirstOrDefault(block => block.BlockType == blockType);
    }
}
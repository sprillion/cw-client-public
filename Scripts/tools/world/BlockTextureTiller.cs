using UnityEngine;

namespace tools.world
{
    [ExecuteAlways]
    public class BlockTextureTiller : MonoBehaviour
    {
        private static readonly int TilingX = Shader.PropertyToID("_Tiling_X");
        private static readonly int TilingY = Shader.PropertyToID("_Tiling_Y");
        private static readonly int TilingZ = Shader.PropertyToID("_Tiling_Z");
        private static readonly int OffsetX = Shader.PropertyToID("_Offset_X");
        private static readonly int OffsetY = Shader.PropertyToID("_Offset_Y");
        private static readonly int OffsetZ = Shader.PropertyToID("_Offset_Z");
        
        private float _blocksWidth;
        private float _blocksLenght;
        private float _blocksHeigth;
        
        private Renderer _renderer;
        private MaterialPropertyBlock _materialProperty;


        private void Awake()
        {
            if (!TryGetComponent(out _renderer)) return;

            _blocksWidth = transform.localScale.x;
            _blocksLenght = transform.localScale.z;
            _blocksHeigth = transform.localScale.y;
            
            _materialProperty = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_materialProperty);
            SetTilling();
        }

        private void SetTilling()
        {
            _materialProperty.SetVector(TilingX, new Vector2(_blocksLenght, _blocksHeigth));
            _materialProperty.SetVector(TilingY, new Vector2(_blocksWidth, _blocksLenght));
            _materialProperty.SetVector(TilingZ, new Vector2(_blocksWidth, _blocksHeigth));

            var lenghtOffset = _blocksLenght % 2 == 0 ? 0 : 0.5f; 
            var heigthOffset = _blocksHeigth % 2 == 0 ? 0 : 0.5f; 
            var widthOffset = _blocksWidth % 2 == 0 ? 0 : 0.5f; 
            
            _materialProperty.SetVector(OffsetX, new Vector2(lenghtOffset, heigthOffset));
            _materialProperty.SetVector(OffsetY, new Vector2(widthOffset, lenghtOffset));
            _materialProperty.SetVector(OffsetZ, new Vector2(widthOffset, heigthOffset));
            
            _renderer.SetPropertyBlock(_materialProperty);
        }
    }
}
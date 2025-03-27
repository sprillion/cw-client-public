using UnityEngine;
using UnityEngine.UI;

namespace ui.tools
{
    //[ExecuteInEditMode]
    public class TilingImage : MonoBehaviour
    {
        [SerializeField] private Image _image; 
        [SerializeField] private float _tilingFactor = 1.0f;


        private void Update()
        {
            UpdateTiling();
        }
                
        private void UpdateTiling()
        {
            _image.material.mainTextureScale = new Vector2(_tilingFactor / _image.rectTransform.rect.height, _tilingFactor / _image.rectTransform.rect.width);
        }

    }
}
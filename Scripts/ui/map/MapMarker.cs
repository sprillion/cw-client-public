using UnityEngine;
using UnityEngine.UI;

namespace ui.map
{
    public class MapMarker : PooledObject
    {
        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _rectTransform;

        public void SetData(Sprite sprite, Color color, Vector2 size)
        {
            _icon.sprite = sprite;
            _icon.color = color;
            _rectTransform.sizeDelta = size;
        }

        public void SetRotation(float degrees)
        {
            transform.localEulerAngles = new Vector3(0, 0, -degrees);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}

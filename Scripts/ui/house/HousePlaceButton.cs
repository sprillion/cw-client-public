using System;
using infrastructure.services.house;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;

namespace ui.house
{
    public class HousePlaceButton : PooledObject
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _button;

        private Transform _anchor;
        private Camera _camera;
        private Camera _renderCamera;
        private RectTransform _rectTransform;
        private RectTransform _canvasRect;

        private SpriteCatalog _spriteCatalog;

        public void Setup(HousePlaceType housePlaceType, Transform anchor, Camera camera, Action onClick)
        {
            if (_spriteCatalog == null)
            {
                _spriteCatalog = GameResources.Data.Catalogs.house_icons_catalog<SpriteCatalog>();
            }
            
            _anchor = anchor;
            _camera = camera;
            _rectTransform = (RectTransform)transform;

            var canvas = GetComponentInParent<Canvas>();
            _canvasRect = (RectTransform)canvas.transform;
            _renderCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            _icon.sprite = _spriteCatalog.GetSprite(housePlaceType);
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }

        public override void OnGetted()
        {
            _anchor = null;
            _camera = null;
        }

        private void Update()
        {
            if (_anchor == null || _camera == null || _canvasRect == null) return;

            var screenPos = _camera.WorldToScreenPoint(_anchor.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPos, _renderCamera, out var localPos);

            _rectTransform.localPosition = localPos;
        }
    }
}

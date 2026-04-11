using System;
using UnityEngine;
using UnityEngine.UI;

namespace ui.house
{
    public class HousePlaceButton : PooledObject
    {
        [SerializeField] private Button _button;

        private Transform _anchor;
        private Camera _camera;
        private RectTransform _rectTransform;
        private RectTransform _canvasRect;
        private Camera _canvasCamera;

        public void Setup(Transform anchor, Camera camera, Action onClick)
        {
            _anchor = anchor;
            _camera = camera;
            _rectTransform = (RectTransform)transform;

            var canvas = GetComponentInParent<Canvas>();
            _canvasRect = (RectTransform)canvas.transform;
            _canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

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
            bool behind = screenPos.z < 0f;

            _button.gameObject.SetActive(!behind);

            if (!behind)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect,
                    new Vector2(screenPos.x, screenPos.y),
                    _canvasCamera,
                    out var localPos);

                _rectTransform.localPosition = localPos;
            }
        }
    }
}

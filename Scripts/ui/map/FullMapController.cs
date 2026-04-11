using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace ui.map
{
    public class FullMapController : MonoBehaviour
    {
        [SerializeField] private RectTransform _mapRect;
        [SerializeField] private RectTransform _parentRect;

        [Header("Zoom Settings")] public float zoomSpeed = 0.1f;
        public float minZoom = 0.5f;
        public float maxZoom = 2f;

        [Header("Drag Settings")] public float dragSpeed = 1f;

        public event Action<Vector2> OnTapDetected;

        private Vector2 _dragStart;
        private Vector2 _touchStartPos;
        private bool _touchExceededThreshold;
        private const float TapThreshold = 20f;

        private void Awake()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            HandleZoom();
            HandleDrag();
        }

        private void HandleZoom()
        {
            // ПК: колесо мыши
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll != 0)
                {
                    var mousePos = Mouse.current.position.ReadValue();
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, mousePos, null, out var cursorPos);
                    ZoomAtPoint(scroll * zoomSpeed * 0.1f, cursorPos);
                }
            }

            // Мобильные устройства: pinch
            if (Touch.activeTouches.Count == 2)
            {
                var t0 = Touch.activeTouches[0];
                var t1 = Touch.activeTouches[1];

                Vector2 t0Prev = t0.screenPosition - t0.delta;
                Vector2 t1Prev = t1.screenPosition - t1.delta;

                float prevDistance = (t0Prev - t1Prev).magnitude;
                float currentDistance = (t0.screenPosition - t1.screenPosition).magnitude;
                float delta = (currentDistance - prevDistance) * 0.01f;

                Vector2 midpoint = (t0.screenPosition + t1.screenPosition) / 2f;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, midpoint, null, out var localPoint);

                ZoomAtPoint(delta * zoomSpeed, localPoint);
            }
        }

        private void ZoomAtPoint(float deltaScale, Vector2 zoomCenter)
        {
            Vector3 oldScale = _mapRect.localScale;
            Vector3 newScale = ClampScale(oldScale + Vector3.one * deltaScale);

            float scaleFactor = newScale.x / oldScale.x;

            Vector2 anchoredPos = _mapRect.anchoredPosition;
            anchoredPos = zoomCenter + (anchoredPos - zoomCenter) * scaleFactor;

            _mapRect.localScale = newScale;
            _mapRect.anchoredPosition = anchoredPos;

            ClampPosition();
        }

        private void HandleDrag()
        {
            // ПК: левая кнопка мыши
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _dragStart = Mouse.current.position.ReadValue();
                }

                if (Mouse.current.leftButton.isPressed)
                {
                    Vector2 delta = Mouse.current.position.ReadValue() - _dragStart;
                    _mapRect.anchoredPosition += delta * dragSpeed;
                    _dragStart = Mouse.current.position.ReadValue();
                    ClampPosition();
                }

                // ПК: правая кнопка мыши — установить маркер
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    OnTapDetected?.Invoke(Mouse.current.position.ReadValue());
            }

            // Мобильные устройства: один палец
            if (Touch.activeTouches.Count == 1)
            {
                var touch = Touch.activeTouches[0];
                if (touch.phase == TouchPhase.Began)
                {
                    _dragStart = touch.screenPosition;
                    _touchStartPos = touch.screenPosition;
                    _touchExceededThreshold = false;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    if (!_touchExceededThreshold &&
                        (touch.screenPosition - _touchStartPos).magnitude > TapThreshold)
                        _touchExceededThreshold = true;

                    Vector2 delta = touch.screenPosition - _dragStart;
                    _mapRect.anchoredPosition += delta * dragSpeed;
                    _dragStart = touch.screenPosition;
                    ClampPosition();
                }
                else if (touch.phase == TouchPhase.Ended && !_touchExceededThreshold)
                {
                    OnTapDetected?.Invoke(touch.screenPosition);
                }
            }
        }

        private Vector3 ClampScale(Vector3 scale)
        {
            scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
            scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
            scale.z = 1f;
            return scale;
        }

        private void ClampPosition()
        {
            Vector2 parentSize = _parentRect.rect.size;
            Vector2 mapSize = _mapRect.sizeDelta * _mapRect.localScale;

            Vector2 pos = _mapRect.anchoredPosition;

            if (mapSize.x <= parentSize.x)
                pos.x = 0;
            else
            {
                float limitX = (mapSize.x - parentSize.x) / 2f;
                pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
            }

            if (mapSize.y <= parentSize.y)
                pos.y = 0;
            else
            {
                float limitY = (mapSize.y - parentSize.y) / 2f;
                pos.y = Mathf.Clamp(pos.y, -limitY, limitY);
            }

            _mapRect.anchoredPosition = pos;
        }
    }
}

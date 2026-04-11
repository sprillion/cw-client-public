using infrastructure.services.map;
using infrastructure.services.players;
using infrastructure.services.waypoint;
using TMPro;
using UnityEngine;
using Zenject;

namespace ui.waypoint
{
    public class WaypointIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform _indicatorRoot;
        [SerializeField] private TMP_Text _distanceText;

        [SerializeField] private float _edgePadding = 50f;

        private IWaypointService _waypointService;
        private ICharacterService _characterService;
        private IMapService _mapService;
        private Camera _camera;

        // Кэшированная высота поверхности в точке вейпоинта.
        // null — чанк ещё не загружен, используем Y=0 как fallback.
        private float? _surfaceY;

        [Inject]
        public void Construct(IWaypointService waypointService, ICharacterService characterService,
            IMapService mapService)
        {
            _waypointService = waypointService;
            _characterService = characterService;
            _mapService = mapService;
        }

        private void Start()
        {
            _camera = Camera.main;
            _waypointService.OnWaypointChanged += OnWaypointChanged;
            _mapService.ChunksUpdated += OnChunksUpdated;
        }

        private void OnDestroy()
        {
            _waypointService.OnWaypointChanged -= OnWaypointChanged;
            _mapService.ChunksUpdated -= OnChunksUpdated;
        }

        private void OnWaypointChanged(Vector3? pos)
        {
            _surfaceY = pos.HasValue
                ? _mapService.GetSurfaceHeight(pos.Value.x, pos.Value.z)
                : null;
        }

        private void OnChunksUpdated()
        {
            var wp = _waypointService.WaypointPosition;
            if (wp.HasValue)
                _surfaceY = _mapService.GetSurfaceHeight(wp.Value.x, wp.Value.z);
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            var wp = _waypointService.WaypointPosition;
            if (!wp.HasValue || _characterService.CurrentCharacter == null)
            {
                _indicatorRoot.gameObject.SetActive(false);
                return;
            }

            _indicatorRoot.gameObject.SetActive(true);

            var playerPos = _characterService.CurrentCharacter.transform.position;

            float distance = new Vector2(wp.Value.x - playerPos.x, wp.Value.z - playerPos.z).magnitude;
            _distanceText.text = $"{Mathf.RoundToInt(distance)}m";

            // Используем высоту поверхности из MapService; fallback — Y=0
            float waypointY = _surfaceY ?? wp.Value.y;
            var waypointPos = new Vector3(wp.Value.x, waypointY, wp.Value.z);

            var screenPos3D = _camera.WorldToScreenPoint(waypointPos);
            bool isBehind = screenPos3D.z < 0f;
            if (isBehind)
                screenPos3D = -screenPos3D;

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var screenPos2D = new Vector2(screenPos3D.x, screenPos3D.y);

            bool isOnScreen = !isBehind
                && screenPos3D.x >= _edgePadding && screenPos3D.x <= Screen.width - _edgePadding
                && screenPos3D.y >= _edgePadding && screenPos3D.y <= Screen.height - _edgePadding;

            Vector2 dir = screenPos2D - center;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;
            dir.Normalize();

            Vector2 indicatorPos;
            if (isOnScreen)
            {
                indicatorPos = screenPos2D;
            }
            else
            {
                float halfW = center.x - _edgePadding;
                float halfH = center.y - _edgePadding;
                float tx = dir.x != 0 ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
                float ty = dir.y != 0 ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
                float t = Mathf.Min(tx, ty);
                indicatorPos = center + dir * t;
            }

            _indicatorRoot.position = indicatorPos;
        }
    }
}

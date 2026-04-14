using System.Collections.Generic;
using infrastructure.factories;
using infrastructure.services.mapMarkers;
using infrastructure.services.mobs;
using infrastructure.services.players;
using infrastructure.services.waypoint;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.map
{
    public class MiniMap : MonoBehaviour
    {
        [SerializeField] private FullMap _fullMap;

        [SerializeField] private Button _fullMapButton;
        [SerializeField] private Button _zoomInButton;
        [SerializeField] private Button _zoomOutButton;

        [SerializeField] private RectTransform _mapTransform;
        [SerializeField] private RectTransform _playerIcon;
        [SerializeField] private RectTransform _iconsContainer;

        [SerializeField] private float[] _scales;
        [SerializeField] private float _baseZoom = 4f;

        private ICharacterService _characterService;
        private IMobService _mobService;
        private IMapMarkerService _mapMarkerService;
        private IWaypointService _waypointService;

        private int _currentScale = 1;
        private bool _npcMarkersCreated;
        private bool _mineMarkersCreated;
        private bool _lumberMarkersCreated;
        private bool _houseMarkersCreated;

        private readonly Dictionary<int, MapMarker> _playerMarkers = new Dictionary<int, MapMarker>();
        private readonly Dictionary<ushort, MapMarker> _mobMarkers = new Dictionary<ushort, MapMarker>();
        private readonly List<MapMarker> _npcMarkers = new List<MapMarker>();
        private readonly List<MapMarker> _mineMarkers = new List<MapMarker>();
        private readonly List<MapMarker> _lumberMarkers = new List<MapMarker>();
        private readonly List<MapMarker> _houseMarkers = new List<MapMarker>();

        private MapMarker _waypointMarker;

        [Inject]
        public void Construct(ICharacterService characterService, IMobService mobService,
            IMapMarkerService mapMarkerService, IWaypointService waypointService)
        {
            _characterService = characterService;
            _mobService = mobService;
            _mapMarkerService = mapMarkerService;
            _waypointService = waypointService;
        }

        private void Start()
        {
            _fullMapButton.onClick.AddListener(ShowFullMap);
            _zoomInButton.onClick.AddListener(ZoomIn);
            _zoomOutButton.onClick.AddListener(ZoomOut);

            _mapTransform.localScale = new Vector3(_scales[_currentScale], _scales[_currentScale], 1f);

            _waypointService.OnWaypointChanged += OnWaypointChanged;
            OnWaypointChanged(_waypointService.WaypointPosition);
        }

        private void OnDestroy()
        {
            _waypointService.OnWaypointChanged -= OnWaypointChanged;
        }

        private void OnWaypointChanged(Vector3? pos)
        {
            if (!pos.HasValue)
            {
                if (_waypointMarker != null)
                {
                    _waypointMarker.Release();
                    _waypointMarker = null;
                }
                return;
            }

            if (_waypointMarker == null)
            {
                _waypointMarker = Pool.Get<MapMarker>();
                _waypointMarker.transform.SetParent(_iconsContainer, false);
                var data = _mapMarkerService.GetIconData(MapIconType.Waypoint);
                if (data != null)
                    _waypointMarker.SetData(data.Icon, data.Color, data.Size);
            }
        }

        private void Update()
        {
            if (_characterService.CurrentCharacter == null) return;

            var playerPos = _characterService.CurrentCharacter.transform.position;
            float totalZoom = _baseZoom * _scales[_currentScale];

            _mapTransform.localPosition = new Vector2(-playerPos.x, -playerPos.z) * totalZoom;
            _playerIcon.rotation = Quaternion.Euler(0, 0, -_characterService.CurrentCharacter.transform.eulerAngles.y);

            UpdateOtherPlayerMarkers(playerPos, totalZoom);
            UpdateMobMarkers(playerPos, totalZoom);
            UpdateNpcMarkers(playerPos, totalZoom);
            UpdateMineMarkers(playerPos, totalZoom);
            UpdateLumberMarkers(playerPos, totalZoom);
            UpdateHouseMarkers(playerPos, totalZoom);
            UpdateWaypointMarker(playerPos, totalZoom);
        }

        private void UpdateWaypointMarker(Vector3 playerPos, float totalZoom)
        {
            if (_waypointMarker == null || !_waypointService.WaypointPosition.HasValue)
                return;

            var wp = _waypointService.WaypointPosition.Value;
            var relPos = new Vector2(wp.x - playerPos.x, wp.z - playerPos.z) * totalZoom;
            float halfW = _iconsContainer.rect.width / 2f;
            float halfH = _iconsContainer.rect.height / 2f;
            if (Mathf.Abs(relPos.x) > halfW || Mathf.Abs(relPos.y) > halfH)
            {
                float tx = relPos.x != 0 ? halfW / Mathf.Abs(relPos.x) : float.MaxValue;
                float ty = relPos.y != 0 ? halfH / Mathf.Abs(relPos.y) : float.MaxValue;
                relPos *= Mathf.Min(tx, ty);
            }
            _waypointMarker.transform.localPosition = relPos;
            _waypointMarker.SetVisible(true);
        }

        private void UpdateOtherPlayerMarkers(Vector3 playerPos, float totalZoom)
        {
            bool typeVisible = _mapMarkerService.IsTypeVisible(MapIconType.OtherPlayer);

            RemoveStaleMarkers(_playerMarkers, _characterService.OtherCharacters);

            foreach (var kvp in _characterService.OtherCharacters)
            {
                if (!_playerMarkers.TryGetValue(kvp.Key, out var marker))
                {
                    marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsContainer, false);
                    var data = _mapMarkerService.GetIconData(MapIconType.OtherPlayer);
                    marker.SetData(data.Icon, data.Color, data.Size);
                    _playerMarkers[kvp.Key] = marker;
                }

                var relPos = kvp.Value.transform.position - playerPos;
                marker.transform.localPosition = new Vector2(relPos.x, relPos.z) * totalZoom;
                marker.SetVisible(typeVisible);
            }
        }

        private void UpdateMobMarkers(Vector3 playerPos, float totalZoom)
        {
            bool typeVisible = _mapMarkerService.IsTypeVisible(MapIconType.Mob);

            RemoveStaleMarkers(_mobMarkers, _mobService.Mobs);

            foreach (var kvp in _mobService.Mobs)
            {
                if (!_mobMarkers.TryGetValue(kvp.Key, out var marker))
                {
                    marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsContainer, false);
                    var data = _mapMarkerService.GetIconData(MapIconType.Mob);
                    marker.SetData(_mapMarkerService.GetMobSprite(kvp.Value.MobType), data.Color, data.Size);
                    _mobMarkers[kvp.Key] = marker;
                }

                var relPos = kvp.Value.transform.position - playerPos;
                marker.transform.localPosition = new Vector2(relPos.x, relPos.z) * totalZoom;
                marker.SetVisible(typeVisible);
            }
        }

        private void UpdateNpcMarkers(Vector3 playerPos, float totalZoom)
        {
            var points = _mapMarkerService.NpcConfigPoints;
            if (!_npcMarkersCreated && points.Count > 0)
            {
                _npcMarkersCreated = true;
                var data = _mapMarkerService.GetIconData(MapIconType.Npc);
                foreach (var point in points)
                {
                    var marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsContainer, false);
                    marker.SetData(_mapMarkerService.GetNpcSprite(point.Type), data.Color, data.Size);
                    _npcMarkers.Add(marker);
                }
            }

            bool visible = _mapMarkerService.IsTypeVisible(MapIconType.Npc);
            for (int i = 0; i < _npcMarkers.Count; i++)
            {
                var relPos = points[i].Position - playerPos;
                _npcMarkers[i].transform.localPosition = new Vector2(relPos.x, relPos.z) * totalZoom;
                _npcMarkers[i].SetVisible(visible);
            }
        }

        private void UpdateMineMarkers(Vector3 playerPos, float totalZoom)
        {
            var points = _mapMarkerService.MineConfigPoints;
            if (!_mineMarkersCreated && points.Count > 0)
            {
                _mineMarkersCreated = true;
                var data = _mapMarkerService.GetIconData(MapIconType.Mine);
                foreach (var _ in points)
                {
                    var marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsContainer, false);
                    if (data != null)
                        marker.SetData(data.Icon, data.Color, data.Size);
                    _mineMarkers.Add(marker);
                }
            }

            bool visible = _mapMarkerService.IsTypeVisible(MapIconType.Mine);
            for (int i = 0; i < _mineMarkers.Count; i++)
            {
                var relPos = points[i] - playerPos;
                _mineMarkers[i].transform.localPosition = new Vector2(relPos.x, relPos.z) * totalZoom;
                _mineMarkers[i].SetVisible(visible);
            }
        }

        private void UpdateLumberMarkers(Vector3 playerPos, float totalZoom)
        {
            var points = _mapMarkerService.LumberAreaPoints;
            if (!_lumberMarkersCreated && points.Count > 0)
            {
                _lumberMarkersCreated = true;
                var data = _mapMarkerService.GetIconData(MapIconType.Lumber);
                foreach (var _ in points)
                {
                    var marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsContainer, false);
                    if (data != null)
                        marker.SetData(data.Icon, data.Color, data.Size);
                    _lumberMarkers.Add(marker);
                }
            }

            bool visible = _mapMarkerService.IsTypeVisible(MapIconType.Lumber);
            for (int i = 0; i < _lumberMarkers.Count; i++)
            {
                var relPos = points[i] - playerPos;
                _lumberMarkers[i].transform.localPosition = new Vector2(relPos.x, relPos.z) * totalZoom;
                _lumberMarkers[i].SetVisible(visible);
            }
        }

        private void UpdateHouseMarkers(Vector3 playerPos, float totalZoom)
        {
            var points = _mapMarkerService.HouseConfigPoints;
            if (!_houseMarkersCreated && points.Count > 0)
            {
                _houseMarkersCreated = true;
                var data = _mapMarkerService.GetIconData(MapIconType.House);
                foreach (var _ in points)
                {
                    var marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsContainer, false);
                    if (data != null)
                        marker.SetData(data.Icon, data.Color, data.Size);
                    _houseMarkers.Add(marker);
                }
            }

            bool visible = _mapMarkerService.IsTypeVisible(MapIconType.House);
            for (int i = 0; i < _houseMarkers.Count; i++)
            {
                var relPos = points[i] - playerPos;
                _houseMarkers[i].transform.localPosition = new Vector2(relPos.x, relPos.z) * totalZoom;
                _houseMarkers[i].SetVisible(visible);
            }
        }

        private void RemoveStaleMarkers<TKey, TEntity>(
            Dictionary<TKey, MapMarker> markers,
            IReadOnlyDictionary<TKey, TEntity> activeEntities)
        {
            var toRemove = new List<TKey>();
            foreach (var id in markers.Keys)
            {
                if (!activeEntities.ContainsKey(id))
                    toRemove.Add(id);
            }
            foreach (var id in toRemove)
            {
                markers[id].Release();
                markers.Remove(id);
            }
        }

        private void ShowFullMap()
        {
            _fullMap.Show();
        }

        private void ZoomIn()
        {
            if (_currentScale >= _scales.Length - 1) return;
            _currentScale++;
            _mapTransform.localScale = new Vector3(_scales[_currentScale], _scales[_currentScale], 1f);
        }

        private void ZoomOut()
        {
            if (_currentScale <= 0) return;
            _currentScale--;
            _mapTransform.localScale = new Vector3(_scales[_currentScale], _scales[_currentScale], 1f);
        }
    }
}

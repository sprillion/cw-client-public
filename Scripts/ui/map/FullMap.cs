using System;
using System.Collections.Generic;
using infrastructure.factories;
using infrastructure.services.mapMarkers;
using infrastructure.services.mobs;
using infrastructure.services.navigation;
using infrastructure.services.players;
using infrastructure.services.transport;
using infrastructure.services.waypoint;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.map
{
    public class FullMap : Popup
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _clearWaypointButton;
        [SerializeField] private Button _autopilotButton;

        [SerializeField] private RectTransform _iconsLayer;

        [SerializeField] private RectTransform _localPlayerIcon;

        // Тот же RectTransform, что и _mapRect в FullMapController (спрайт карты).
        // Используется для корректного маппинга мировых координат в локальные,
        // независимо от pivot/anchor карты.
        [SerializeField] private RectTransform _mapImageRect;

        [SerializeField] private Transform _togglesContainer;

        [SerializeField] private FullMapController _controller;

        private ICharacterService _characterService;
        private IMobService _mobService;
        private IMapMarkerService _mapMarkerService;
        private IWaypointService _waypointService;
        private INavigationService _navigationService;
        private ITransportService _transportService;

        private bool _isOpen;
        private bool _togglesInitialized;
        private bool _npcMarkersCreated;
        private bool _mineMarkersCreated;
        private bool _lumberMarkersCreated;

        private readonly Dictionary<int, MapMarker> _playerMarkers = new Dictionary<int, MapMarker>();
        private readonly Dictionary<ushort, MapMarker> _mobMarkers = new Dictionary<ushort, MapMarker>();
        private readonly List<MapMarker> _npcMarkers = new List<MapMarker>();
        private readonly List<MapMarker> _mineMarkers = new List<MapMarker>();
        private readonly List<MapMarker> _lumberMarkers = new List<MapMarker>();

        private MapMarker _waypointMarker;

        [Inject]
        public void Construct(ICharacterService characterService, IMobService mobService,
            IMapMarkerService mapMarkerService, IWaypointService waypointService,
            INavigationService navigationService, ITransportService transportService)
        {
            _characterService = characterService;
            _mobService = mobService;
            _mapMarkerService = mapMarkerService;
            _waypointService = waypointService;
            _navigationService = navigationService;
            _transportService = transportService;
        }

        private void Start()
        {
            Initialize();
        }

        public override void Initialize()
        {
            _closeButton.onClick.AddListener(Back);
            _clearWaypointButton.onClick.AddListener(_waypointService.ClearWaypoint);
            _autopilotButton.onClick.AddListener(OnAutopilotClicked);
            InitializeToggles();
        }

        public override void Show()
        {
            _canvas.sortingOrder = 99;
            _isOpen = true;
            _mapMarkerService.OnVisibilityChanged += OnVisibilityChanged;
            _controller.OnTapDetected += OnMapTapped;
            _waypointService.OnWaypointChanged += OnWaypointChanged;
            _transportService.OnLocalMounted += UpdateAutopilotButton;
            _transportService.OnLocalDismounted += UpdateAutopilotButton;
            _navigationService.OnCooldownRejected += OnCooldownRejected;
            _navigationService.OnNavigationStarted += UpdateAutopilotButton;
            _navigationService.OnNavigationCompleted += UpdateAutopilotButton;
            _navigationService.OnNavigationCancelled += UpdateAutopilotButton;
            OnWaypointChanged(_waypointService.WaypointPosition);
            UpdateAutopilotButton();
            base.Show();
        }

        public override void Hide()
        {
            _canvas.sortingOrder = 1;
            _isOpen = false;
            _mapMarkerService.OnVisibilityChanged -= OnVisibilityChanged;
            _controller.OnTapDetected -= OnMapTapped;
            _waypointService.OnWaypointChanged -= OnWaypointChanged;
            _transportService.OnLocalMounted -= UpdateAutopilotButton;
            _transportService.OnLocalDismounted -= UpdateAutopilotButton;
            _navigationService.OnCooldownRejected -= OnCooldownRejected;
            _navigationService.OnNavigationStarted -= UpdateAutopilotButton;
            _navigationService.OnNavigationCompleted -= UpdateAutopilotButton;
            _navigationService.OnNavigationCancelled -= UpdateAutopilotButton;
            if (_waypointMarker != null)
            {
                _waypointMarker.Release();
                _waypointMarker = null;
            }
            ClearMarkers();
            _npcMarkersCreated = false;
            _mineMarkersCreated = false;
            _lumberMarkersCreated = false;
            base.Hide();
        }

        private void Update()
        {
            if (!_isOpen) return;
            if (_characterService.CurrentCharacter == null) return;

            var playerPos = _characterService.CurrentCharacter.transform.position;

            if (_localPlayerIcon != null)
            {
                _localPlayerIcon.localPosition = WorldToLocal(playerPos);
                _localPlayerIcon.localEulerAngles =
                    new Vector3(0, 0, -_characterService.CurrentCharacter.transform.eulerAngles.y);
            }

            UpdateOtherPlayerMarkers();
            UpdateMobMarkers();
            UpdateNpcMarkers();

            if (_waypointMarker != null && _waypointService.WaypointPosition.HasValue)
                _waypointMarker.transform.localPosition = WorldToLocal(_waypointService.WaypointPosition.Value);
        }

        private Vector2 WorldToLocal(Vector3 worldPos)
        {
            float ratio = _mapImageRect.rect.width / 2048f;
            float s = _mapImageRect.localScale.x;
            return (Vector2)_mapImageRect.localPosition
                   + new Vector2(
                       (worldPos.x * ratio) * s,
                       (worldPos.z * ratio) * s);
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _iconsLayer, screenPos, null, out var localPos);
            float ratio = _mapImageRect.rect.width / 2048f;
            float s = _mapImageRect.localScale.x;
            Vector2 offset = localPos - (Vector2)_mapImageRect.localPosition;
            return new Vector3(offset.x / (ratio * s), 0f, offset.y / (ratio * s));
        }

        private void OnMapTapped(Vector2 screenPos)
        {
            _waypointService.SetWaypoint(ScreenToWorld(screenPos));
        }

        private void OnWaypointChanged(Vector3? pos)
        {
            _clearWaypointButton.gameObject.SetActive(pos.HasValue);
            UpdateAutopilotButton();

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
                _waypointMarker.transform.SetParent(_iconsLayer, false);
                var data = _mapMarkerService.GetIconData(MapIconType.Waypoint);
                if (data != null)
                    _waypointMarker.SetData(data.Icon, data.Color, data.Size);
            }

            _waypointMarker.transform.localPosition = WorldToLocal(pos.Value);
        }

        private void OnAutopilotClicked()
        {
            if (_navigationService.IsActive)
                _navigationService.CancelNavigation();
            else if (_waypointService.WaypointPosition.HasValue)
                _navigationService.RequestPath(_waypointService.WaypointPosition.Value);
        }

        private void UpdateAutopilotButton()
        {
            if (_autopilotButton == null) return;
            bool canStart = _transportService.IsLocalMounted && _waypointService.WaypointPosition.HasValue;
            _autopilotButton.gameObject.SetActive(_navigationService.IsActive || canStart);
        }

        private void OnCooldownRejected(float remainingSeconds)
        {
            // Cooldown is enforced server-side; button stays visible so player can retry
        }

        private void InitializeToggles()
        {
            if (_togglesInitialized || _togglesContainer == null) return;
            _togglesInitialized = true;

            foreach (MapIconType type in Enum.GetValues(typeof(MapIconType)))
            {
                if (type == MapIconType.Waypoint) continue;
                var toggle = Pool.Get<MapIconToggle>();
                toggle.transform.SetParent(_togglesContainer, false);
                toggle.Initialize(type, _mapMarkerService);
            }
        }

        private void ClearMarkers()
        {
            foreach (var marker in _playerMarkers.Values) marker.Release();
            foreach (var marker in _mobMarkers.Values) marker.Release();
            foreach (var marker in _npcMarkers) marker.Release();
            foreach (var marker in _mineMarkers) marker.Release();
            foreach (var marker in _lumberMarkers) marker.Release();

            _playerMarkers.Clear();
            _mobMarkers.Clear();
            _npcMarkers.Clear();
            _mineMarkers.Clear();
            _lumberMarkers.Clear();
        }

        private void UpdateOtherPlayerMarkers()
        {
            RemoveStaleMarkers(_playerMarkers, _characterService.OtherCharacters);
            foreach (var kvp in _characterService.OtherCharacters)
            {
                if (!_playerMarkers.TryGetValue(kvp.Key, out var marker))
                {
                    marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsLayer, false);
                    var data = _mapMarkerService.GetIconData(MapIconType.OtherPlayer);
                    marker.SetData(data.Icon, data.Color, data.Size);
                    marker.SetVisible(_mapMarkerService.IsTypeVisible(MapIconType.OtherPlayer));
                    _playerMarkers[kvp.Key] = marker;
                }

                marker.transform.localPosition = WorldToLocal(kvp.Value.transform.position);
            }
        }

        private void UpdateMobMarkers()
        {
            RemoveStaleMarkers(_mobMarkers, _mobService.Mobs);
            foreach (var kvp in _mobService.Mobs)
            {
                if (!_mobMarkers.TryGetValue(kvp.Key, out var marker))
                {
                    marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsLayer, false);
                    var data = _mapMarkerService.GetIconData(MapIconType.Mob);
                    marker.SetData(_mapMarkerService.GetMobSprite(kvp.Value.MobType), data.Color, data.Size);
                    marker.SetVisible(_mapMarkerService.IsTypeVisible(MapIconType.Mob));
                    _mobMarkers[kvp.Key] = marker;
                }

                marker.transform.localPosition = WorldToLocal(kvp.Value.transform.position);
            }
        }

        private void UpdateNpcMarkers()
        {
            var npcPoints = _mapMarkerService.NpcConfigPoints;
            if (!_npcMarkersCreated && npcPoints.Count > 0)
            {
                _npcMarkersCreated = true;
                var npcData = _mapMarkerService.GetIconData(MapIconType.Npc);
                foreach (var point in npcPoints)
                {
                    var marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsLayer, false);
                    marker.SetData(_mapMarkerService.GetNpcSprite(point.Type), npcData.Color, npcData.Size);
                    marker.SetVisible(_mapMarkerService.IsTypeVisible(MapIconType.Npc));
                    _npcMarkers.Add(marker);
                }
            }

            var minePoints = _mapMarkerService.MineConfigPoints;
            if (!_mineMarkersCreated && minePoints.Count > 0)
            {
                _mineMarkersCreated = true;
                var mineData = _mapMarkerService.GetIconData(MapIconType.Mine);
                foreach (var pos in minePoints)
                {
                    var marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsLayer, false);
                    if (mineData != null)
                        marker.SetData(mineData.Icon, mineData.Color, mineData.Size);
                    marker.SetVisible(_mapMarkerService.IsTypeVisible(MapIconType.Mine));
                    _mineMarkers.Add(marker);
                }
            }

            for (int i = 0; i < _npcMarkers.Count; i++)
                _npcMarkers[i].transform.localPosition = WorldToLocal(npcPoints[i].Position);

            for (int i = 0; i < _mineMarkers.Count; i++)
                _mineMarkers[i].transform.localPosition = WorldToLocal(minePoints[i]);

            var lumberPoints = _mapMarkerService.LumberAreaPoints;
            if (!_lumberMarkersCreated && lumberPoints.Count > 0)
            {
                _lumberMarkersCreated = true;
                var lumberData = _mapMarkerService.GetIconData(MapIconType.Lumber);
                foreach (var pos in lumberPoints)
                {
                    var marker = Pool.Get<MapMarker>();
                    marker.transform.SetParent(_iconsLayer, false);
                    if (lumberData != null)
                        marker.SetData(lumberData.Icon, lumberData.Color, lumberData.Size);
                    marker.SetVisible(_mapMarkerService.IsTypeVisible(MapIconType.Lumber));
                    _lumberMarkers.Add(marker);
                }
            }

            for (int i = 0; i < _lumberMarkers.Count; i++)
                _lumberMarkers[i].transform.localPosition = WorldToLocal(lumberPoints[i]);
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

        private void OnVisibilityChanged(MapIconType type, bool visible)
        {
            switch (type)
            {
                case MapIconType.OtherPlayer:
                    foreach (var m in _playerMarkers.Values) m.SetVisible(visible);
                    break;
                case MapIconType.Mob:
                    foreach (var m in _mobMarkers.Values) m.SetVisible(visible);
                    break;
                case MapIconType.Npc:
                    foreach (var m in _npcMarkers) m.SetVisible(visible);
                    break;
                case MapIconType.Mine:
                    foreach (var m in _mineMarkers) m.SetVisible(visible);
                    break;
                case MapIconType.Lumber:
                    foreach (var m in _lumberMarkers) m.SetVisible(visible);
                    break;
                case MapIconType.LocalPlayer:
                    if (_localPlayerIcon != null) _localPlayerIcon.gameObject.SetActive(visible);
                    break;
            }
        }
    }
}
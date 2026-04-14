using System;
using System.Collections.Generic;
using environment.house;
using infrastructure.factories;
using infrastructure.services.craft;
using infrastructure.services.house;
using ui.craft;
using UnityEngine;
using Zenject;

namespace ui.house
{
    public class HousePlaceButtonsView : MonoBehaviour
    {
        [SerializeField] private HousePlaceObject[] _places;
        [SerializeField] private CraftView _craftView;
        [SerializeField] private UpgradeHouseView _upgradeHouseView;

        private IHouseService _houseService;
        private Camera _camera;
        private readonly Dictionary<HousePlaceType, HousePlaceButton> _activeButtons = new();

        [Inject]
        public void Construct(IHouseService houseService)
        {
            _houseService = houseService;
        }

        public void Start()
        {
            _camera = Camera.main;

            Pool.CreatePool<HousePlaceButton>(6);
            _houseService.OnHouseReceived += Refresh;
        }

        private void OnDestroy()
        {
            if (_houseService != null)
                _houseService.OnHouseReceived -= Refresh;
        }

        private void Refresh()
        {
            int houseLevel = _houseService.GetHousePlaceInfo(HousePlaceType.House)?.Level ?? -1;

            foreach (var place in _places)
            {
                var info = _houseService.GetHousePlaceInfo(place.Type);
                place.Refresh(info?.Level ?? -1);

                if (place.CanShowButton(info, houseLevel))
                    SpawnOrUpdateButton(place);
                else
                    ReturnButton(place.Type);
            }
        }

        private void SpawnOrUpdateButton(HousePlaceObject place)
        {
            if (_activeButtons.ContainsKey(place.Type)) return;

            var btn = Pool.Get<HousePlaceButton>();
            btn.SetParentPreserveScale(transform);
            btn.Setup(place.Type, place.ButtonAnchor, _camera, GetButtonClickAction(place));

            _activeButtons[place.Type] = btn;
        }

        private void ReturnButton(HousePlaceType type)
        {
            if (!_activeButtons.TryGetValue(type, out var btn)) return;
            btn.Release();
            _activeButtons.Remove(type);
        }

        protected virtual Action GetButtonClickAction(HousePlaceObject place)
        {
            var type = place.Type;
            return () =>
            {
                var info = _houseService.GetHousePlaceInfo(type);

                if (info == null || info.Level == 0)
                {
                    _upgradeHouseView.Show(type);
                    return;
                }

                switch (type)
                {
                    case HousePlaceType.House:
                        _upgradeHouseView.Show(HousePlaceType.House);
                        break;
                    case HousePlaceType.Workbench:
                        _craftView.Show(null, CraftPlaceType.Workbench);
                        break;
                    case HousePlaceType.Furnace:
                        _craftView.Show(null, CraftPlaceType.Furnace);
                        break;
                    case HousePlaceType.Anvil:
                        _craftView.Show(null, CraftPlaceType.Anvil);
                        break;
                    case HousePlaceType.Brewing:
                        _craftView.Show(null, CraftPlaceType.Brewing);
                        break;
                    case HousePlaceType.Garden:
                        _craftView.Show(null, CraftPlaceType.Garden);
                        break;
                }
            };
        }
    }
}
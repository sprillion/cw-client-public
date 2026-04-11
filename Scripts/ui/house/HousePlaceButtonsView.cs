using System;
using System.Collections.Generic;
using environment.house;
using infrastructure.factories;
using infrastructure.services.craft;
using infrastructure.services.house;
using ui.craft;
using UnityEngine;

namespace ui.house
{
    public class HousePlaceButtonsView : MonoBehaviour
    {
        [SerializeField] private HousePlaceObject[] _places;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CraftView _craftView;

        private IHouseService _houseService;
        private Camera _camera;
        private readonly Dictionary<HousePlaceType, HousePlaceButton> _activeButtons = new();

        public void Initialize(IHouseService houseService)
        {
            _houseService = houseService;
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
            btn.SetParentPreserveScale(_canvas.transform);
            btn.Setup(place.ButtonAnchor, _camera, GetButtonClickAction(place));
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
            return place.Type switch
            {
                HousePlaceType.Workbench  => () => _craftView.Show(null, CraftPlaceType.Workbench),
                HousePlaceType.Furnace    => () => _craftView.Show(null, CraftPlaceType.Furnace),
                HousePlaceType.Anvil      => () => _craftView.Show(null, CraftPlaceType.Anvil),
                HousePlaceType.PotionRack => () => _craftView.Show(null, CraftPlaceType.PotionMaker),
                _                         => null
            };
        }
    }
}

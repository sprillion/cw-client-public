using System.Collections.Generic;
using infrastructure.services.players;
using infrastructure.services.settings;
using network;
using UnityEngine;

namespace infrastructure.services.proximity
{
    public class ProximityService : IProximityService
    {
        private const float UnitsPerRenderStep = 16f;
        private const float HysteresisMultiplier = 1.15f;
        private const int CheckInterval = 15;

        private readonly ICharacterService _characterService;
        private readonly List<GameObject> _objects = new();
        private float _activateSqr;
        private float _deactivateSqr;
        private int _tick;

        public ProximityService(INetworkManager networkManager, ICharacterService characterService, ISettingsService settingsService)
        {
            _characterService = characterService;
            UpdateRadii(settingsService.Current.renderDistance);
            settingsService.OnRenderDistanceChanged += UpdateRadii;
            networkManager.Update += Tick;
        }

        public void Register(GameObject obj) => _objects.Add(obj);

        public void Unregister(GameObject obj) => _objects.Remove(obj);

        private void UpdateRadii(int renderDistance)
        {
            float activate = renderDistance * UnitsPerRenderStep;
            _activateSqr = activate * activate;
            float deactivate = activate * HysteresisMultiplier;
            _deactivateSqr = deactivate * deactivate;
        }

        private void Tick()
        {
            if (++_tick < CheckInterval) return;
            _tick = 0;

            var character = _characterService.CurrentCharacter;
            if (character == null) return;

            var playerPos = character.transform.position;

            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                var obj = _objects[i];
                if (obj == null) { _objects.RemoveAt(i); continue; }

                float sqrDist = (playerPos - obj.transform.position).sqrMagnitude;
                bool isActive = obj.activeSelf;

                if (!isActive && sqrDist <= _activateSqr)
                    obj.SetActive(true);
                else if (isActive && sqrDist > _deactivateSqr)
                    obj.SetActive(false);
            }
        }
    }
}

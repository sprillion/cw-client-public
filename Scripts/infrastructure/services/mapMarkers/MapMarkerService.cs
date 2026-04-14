using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using environment.house;
using environment.lumber;
using environment.mine;
using infrastructure.factories;
using infrastructure.services.bundles;
using infrastructure.services.proximity;
using infrastructure.services.mobs;
using infrastructure.services.npc;
using infrastructure.services.saveLoad;
using Newtonsoft.Json;
using ui.map;
using ui.tools;
using UnityEngine;

namespace infrastructure.services.mapMarkers
{
    public readonly struct NpcMapPoint
    {
        public readonly NpcType Type;
        public readonly Vector3 Position;

        public NpcMapPoint(NpcType type, Vector3 position)
        {
            Type = type;
            Position = position;
        }
    }

    public class MapMarkerService : IMapMarkerService
    {
        private const string VisibilityKeyPrefix = "MapIconVisible_";

        private readonly MapIconCatalog _mapIconCatalog;
        private readonly SpriteCatalog _mobIconsCatalog;
        private readonly INpcService _npcService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IProximityService _proximityService;

        private readonly Dictionary<MapIconType, bool> _visibility = new Dictionary<MapIconType, bool>();

        private readonly List<NpcMapPoint> _npcConfigPoints = new List<NpcMapPoint>();
        private readonly List<Vector3> _mineConfigPoints = new List<Vector3>();
        private readonly List<Vector3> _lumberAreaPoints = new List<Vector3>();
        private readonly List<Vector3> _houseConfigPoints = new List<Vector3>();
        private List<NpcMapPoint> _configNpcPointsBuffer;
        private bool _npcDataReady;

        public IReadOnlyList<NpcMapPoint> NpcConfigPoints => _npcConfigPoints;
        public IReadOnlyList<Vector3> MineConfigPoints => _mineConfigPoints;
        public IReadOnlyList<Vector3> LumberAreaPoints => _lumberAreaPoints;
        public IReadOnlyList<Vector3> HouseConfigPoints => _houseConfigPoints;

        public event Action<MapIconType, bool> OnVisibilityChanged;

        public MapMarkerService(INpcService npcService, ISaveLoadService saveLoadService, IBundleService bundleService, IProximityService proximityService)
        {
            _npcService = npcService;
            _saveLoadService = saveLoadService;
            _proximityService = proximityService;

            _mapIconCatalog = GameResources.Data.Map.map_icon_catalog<MapIconCatalog>();
            _mobIconsCatalog = GameResources.Data.Catalogs.mob_icons<SpriteCatalog>();

            foreach (MapIconType type in Enum.GetValues(typeof(MapIconType)))
            {
                var key = VisibilityKeyPrefix + type;
                _visibility[type] = !_saveLoadService.HasJson(key) || _saveLoadService.GetJson(key) == "true";
            }

            _npcService.OnNpcLoaded += OnNpcDataLoaded;
            LoadConfigAsync(bundleService).Forget();
        }

        private void OnNpcDataLoaded()
        {
            _npcService.OnNpcLoaded -= OnNpcDataLoaded;
            _npcDataReady = true;
            if (_configNpcPointsBuffer != null)
            {
                _npcConfigPoints.AddRange(_configNpcPointsBuffer);
                _configNpcPointsBuffer = null;
            }
        }

        public MapIconData GetIconData(MapIconType type) => _mapIconCatalog.Get(type);

        public Sprite GetMobSprite(MobType mobType) => _mobIconsCatalog.GetSprite(mobType);

        public Sprite GetNpcSprite(NpcType npcType)
        {
            var icon = _npcService.GetNpcAvatarIcon(npcType);
            return icon != null ? icon : _mapIconCatalog.Get(MapIconType.Npc)?.Icon;
        }

        public bool IsTypeVisible(MapIconType type) => _visibility[type];

        public void SetTypeVisible(MapIconType type, bool visible)
        {
            _visibility[type] = visible;
            _saveLoadService.SetJson(VisibilityKeyPrefix + type, visible ? "true" : "false");
            OnVisibilityChanged?.Invoke(type, visible);
        }

        private async UniTaskVoid LoadConfigAsync(IBundleService bundleService)
        {
            var asset = await bundleService.LoadAssetByName<UnityEngine.TextAsset>("MapPointsConfig");
            if (asset == null) return;

            var config = JsonConvert.DeserializeObject<MapPointsConfigJson>(asset.text);
            if (config == null) return;

            if (config.Npcs != null)
            {
                var points = new List<NpcMapPoint>(config.Npcs.Count);
                foreach (var n in config.Npcs)
                    points.Add(new NpcMapPoint(n.Type, new Vector3(n.X, n.Y, n.Z)));

                if (_npcDataReady)
                    _npcConfigPoints.AddRange(points);
                else
                    _configNpcPointsBuffer = points;
            }

            if (config.Mines != null)
                foreach (var m in config.Mines)
                {
                    var pos = new Vector3(m.X, m.Y, m.Z);
                    _mineConfigPoints.Add(pos);
                    var mineEnter = Pool.Get<MineEnter>();
                    mineEnter.transform.position = pos;
                    _proximityService.Register(mineEnter.gameObject);
                }
            
            if (config.Houses != null)
                foreach (var house in config.Houses)
                {
                    var pos = new Vector3(house.X, house.Y, house.Z);
                    _houseConfigPoints.Add(pos);
                    var houseEnter = Pool.Get<HouseEnter>();
                    houseEnter.transform.position = pos;
                    _proximityService.Register(houseEnter.gameObject);
                }

            var lumberAsset = await bundleService.LoadAssetByName<UnityEngine.TextAsset>("LumberConfig");
            if (lumberAsset == null) return;

            var lumberConfig = JsonConvert.DeserializeObject<LumberConfigJson>(lumberAsset.text);
            if (lumberConfig?.LumberAreas == null) return;

            foreach (var area in lumberConfig.LumberAreas)
            {
                _lumberAreaPoints.Add(new Vector3(area.X, area.Y, area.Z));
                foreach (var tree in area.Trees)
                {
                    var lumberTree = Pool.Get<LumberTree>();
                    lumberTree.TreeId = tree.Id;
                    lumberTree.transform.position = new Vector3(tree.X, tree.Y, tree.Z);
                    _proximityService.Register(lumberTree.gameObject);
                }
            }
        }

        [Serializable]
        private class NpcPositionJson
        {
            public NpcType Type;
            public float X, Y, Z;
        }

        [Serializable]
        private class MinePositionJson
        {
            public float X, Y, Z;
        }
        
        [Serializable]
        private class HousePositionJson
        {
            public float X, Y, Z;
        }

        [Serializable]
        private class MapPointsConfigJson
        {
            public List<NpcPositionJson> Npcs;
            public List<MinePositionJson> Mines;
            public List<HousePositionJson> Houses;
        }

        [Serializable]
        private class LumberTreeJson
        {
            public int Id;
            public float X, Y, Z;
        }

        [Serializable]
        private class LumberAreaJson
        {
            public float X, Y, Z;
            public List<LumberTreeJson> Trees;
        }

        [Serializable]
        private class LumberConfigJson
        {
            public List<LumberAreaJson> LumberAreas;
        }
    }
}

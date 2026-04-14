using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.services.bundles;
using network;
using Newtonsoft.Json;
using tools;
using UnityEngine;

namespace infrastructure.services.house
{
    public class HouseService : IHouseService
    {
        public enum FromClientMessage : byte
        {
            GetHouse,
            GetCurrentHouseUpgrades,
            StartUpgrade,
            ApplyUpgrade,
            BuyPlot,
            GetPlot,
        }

        public enum FromServerMessage : byte
        {
            House,
            CurrentHouseUpgrades,
            StartUpgrade,
            UpgradeResult,
            CancelStartUpgrade,
            BuyPlotResult,
            Plot,
        }

        private readonly INetworkManager _networkManager;
        private readonly IBundleService _bundleService;

        public HouseService(INetworkManager networkManager, IBundleService bundleService)
        {
            _networkManager = networkManager;
            _bundleService = bundleService;
        }

        private Dictionary<HousePlaceType, HousePlaceInfo> _housePlaceLevels = new Dictionary<HousePlaceType, HousePlaceInfo>();
        private Dictionary<HousePlaceType, CurrentHouseUpgrade> _houseUpgrades = new Dictionary<HousePlaceType, CurrentHouseUpgrade>();
        private Dictionary<HousePlaceType, HousePlaceConfigData> _houseConfig = new Dictionary<HousePlaceType, HousePlaceConfigData>();

        public CurrencyType PlotCurrencyType { get; private set; }
        public int PlotPrice { get; private set; }

        public event Action OnHouseReceived;
        public event Action<bool> OnPlotStatusReceived;
        public event Action<bool> OnBuyPlotResult;
        public event Action<HousePlaceType, bool> OnUpgradeResult;

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.House:
                    SetHouse(message);
                    break;
                case FromServerMessage.CurrentHouseUpgrades:
                    SetCurrentHouseUpgrades(message);
                    break;
                case FromServerMessage.StartUpgrade:
                    SetStartUpgrade(message);
                    break;
                case FromServerMessage.UpgradeResult:
                    SetUpgradeResult(message);
                    break;
                case FromServerMessage.CancelStartUpgrade:
                    break;
                case FromServerMessage.BuyPlotResult:
                    SetBuyPlotResult(message);
                    break;
                case FromServerMessage.Plot:
                    SetPlot(message);
                    break;
            }
        }

        public void GetHouse()
        {
            var message = new Message(MessageType.House)
                .AddByte(FromClientMessage.GetHouse.ToByte());

            _networkManager.SendMessage(message);
        }

        public void GetCurrentHouseUpgrades()
        {
            var message = new Message(MessageType.House)
                .AddByte(FromClientMessage.GetCurrentHouseUpgrades.ToByte());

            _networkManager.SendMessage(message);
        }

        public void GetPlot()
        {
            var message = new Message(MessageType.House)
                .AddByte(FromClientMessage.GetPlot.ToByte());

            _networkManager.SendMessage(message);
        }

        public void BuyPlot()
        {
            var message = new Message(MessageType.House)
                .AddByte(FromClientMessage.BuyPlot.ToByte());

            _networkManager.SendMessage(message);
        }

        public void StartUpgrade(HousePlaceType housePlaceType)
        {
            var message = new Message(MessageType.House)
                .AddByte(FromClientMessage.StartUpgrade.ToByte())
                .AddByte(housePlaceType.ToByte());

            _networkManager.SendMessage(message);
        }

        public void ApplyUpgrade(HousePlaceType housePlaceType)
        {
            var message = new Message(MessageType.House)
                .AddByte(FromClientMessage.ApplyUpgrade.ToByte())
                .AddByte(housePlaceType.ToByte());

            _networkManager.SendMessage(message);
        }

        public HousePlaceInfo GetHousePlaceInfo(HousePlaceType housePlaceType)
        {
            _housePlaceLevels.TryGetValue(housePlaceType, out var info);
            return info;
        }

        public void LoadHouseConfig() => LoadHouseConfigAsync().Forget();

        private async UniTask LoadHouseConfigAsync()
        {
            var data = await _bundleService.LoadAssetByName<TextAsset>("HouseConfig");
            var list = JsonConvert.DeserializeObject<List<HousePlaceConfigData>>(data.text);
            _houseConfig = list.ToDictionary(d => d.HousePlaceType, d => d);
        }

        public HousePlaceCraftData GetHouseUpgradeData(HousePlaceType type, int currentLevel)
        {
            if (!_houseConfig.TryGetValue(type, out var config)) return null;
            if (currentLevel < 0 || currentLevel >= config.HousePlaceCrafts.Count) return null;
            return config.HousePlaceCrafts[currentLevel];
        }

        public bool IsCurrentUpgrade(HousePlaceType type) => _houseUpgrades.ContainsKey(type);
        public bool IsMaxLevel(HousePlaceType type) => _housePlaceLevels[type].IsMaxLevel;

        public bool IsCompleteUpgrade(HousePlaceType type)
        {
            if (!_houseUpgrades.TryGetValue(type, out var upgrade)) return false;
            return upgrade.FinishDate <= NetworkManager.ServerNow;
        }

        public DateTime GetFinishTimeUpgrade(HousePlaceType type)
        {
            if (!_houseUpgrades.TryGetValue(type, out var upgrade)) return default;
            return upgrade.FinishDate;
        }

        private void SetHouse(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                var housePlaceType = message.GetByteEnum<HousePlaceType>();
                var level = message.GetInt();
                var requiredHouseLevel = message.GetInt();
                var maxLevel = message.GetInt();
                
                _housePlaceLevels[housePlaceType] = new HousePlaceInfo
                {
                    HousePlaceType = housePlaceType,
                    Level = level,
                    RequiredHouseLevel = requiredHouseLevel,
                    MaxLevel = maxLevel
                };
            }
            
            OnHouseReceived?.Invoke();
        }

        private void SetCurrentHouseUpgrades(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                var housePlaceType = message.GetByteEnum<HousePlaceType>();
                var nextLevel = message.GetInt();
                var endTime = message.GetLong();

                var finishDate = DateTimeOffset.FromUnixTimeSeconds(endTime).UtcDateTime;

                _houseUpgrades[housePlaceType] = new CurrentHouseUpgrade()
                {
                    HousePlaceType = housePlaceType,
                    NextLevel = nextLevel,
                    FinishDate = finishDate
                };
            }

            OnHouseReceived?.Invoke();
        }

        private void SetStartUpgrade(Message message)
        {
            var housePlaceType = message.GetByteEnum<HousePlaceType>();
            var nextLevel = message.GetInt();
            var duration = message.GetFloat();

            var finishDate = DateTime.UtcNow + TimeSpan.FromSeconds(duration);

            _houseUpgrades[housePlaceType] = new CurrentHouseUpgrade()
            {
                HousePlaceType = housePlaceType,
                NextLevel = nextLevel,
                FinishDate = finishDate
            };

            OnHouseReceived?.Invoke();
        }

        private void SetPlot(Message message)
        {
            var hasPlot = message.GetBool();
            PlotCurrencyType = (CurrencyType)message.GetByte();
            PlotPrice = message.GetInt();
            OnPlotStatusReceived?.Invoke(hasPlot);
        }

        private void SetUpgradeResult(Message message)
        {
            var housePlaceType = message.GetByteEnum<HousePlaceType>();
            var success = message.GetBool();

            if (success)
            {
                if (_houseUpgrades.TryGetValue(housePlaceType, out var upgrade) &&
                    _housePlaceLevels.TryGetValue(housePlaceType, out var info))
                {
                    info.Level = upgrade.NextLevel;
                }
                _houseUpgrades.Remove(housePlaceType);
            }

            OnUpgradeResult?.Invoke(housePlaceType, success);
            OnHouseReceived?.Invoke();
        }

        private void SetBuyPlotResult(Message message)
        {
            var success = message.GetBool();
            OnBuyPlotResult?.Invoke(success);
        }
    }
}

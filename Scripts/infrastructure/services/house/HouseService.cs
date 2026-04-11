using System;
using System.Collections.Generic;
using network;
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

        public HouseService(INetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        private Dictionary<HousePlaceType, HousePlaceInfo> _housePlaceLevels = new Dictionary<HousePlaceType, HousePlaceInfo>();
        private Dictionary<HousePlaceType, CurrentHouseUpgrade> _houseUpgrades = new Dictionary<HousePlaceType, CurrentHouseUpgrade>();

        public CurrencyType PlotCurrencyType { get; private set; }
        public int PlotPrice { get; private set; }

        public event Action OnHouseReceived;
        public event Action<bool> OnPlotStatusReceived;
        public event Action<bool> OnBuyPlotResult;

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

        private void SetHouse(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                var housePlaceType = message.GetByteEnum<HousePlaceType>();
                var level = message.GetInt();
                var requiredHouseLevel = message.GetInt();

                _housePlaceLevels[housePlaceType] = new HousePlaceInfo
                {
                    Level = level,
                    RequiredHouseLevel = requiredHouseLevel
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
                var duration = message.GetFloat();

                var finishDate = DateTime.UtcNow + TimeSpan.FromSeconds(duration);

                _houseUpgrades[housePlaceType] = new CurrentHouseUpgrade()
                {
                    HousePlaceType = housePlaceType,
                    NextLevel = nextLevel,
                    FinishDate = finishDate
                };
            }
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
        }

        private void SetPlot(Message message)
        {
            var hasPlot = message.GetBool();
            PlotCurrencyType = (CurrencyType)message.GetByte();
            PlotPrice = message.GetInt();
            OnPlotStatusReceived?.Invoke(hasPlot);
        }

        private void SetBuyPlotResult(Message message)
        {
            var success = message.GetBool();
            OnBuyPlotResult?.Invoke(success);
        }
    }
}

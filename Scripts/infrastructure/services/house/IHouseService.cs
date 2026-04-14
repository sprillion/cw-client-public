using System;
using network;

namespace infrastructure.services.house
{
    public interface IHouseService : IReceiver
    {
        CurrencyType PlotCurrencyType { get; }
        int PlotPrice { get; }

        event Action OnHouseReceived;
        event Action<bool> OnPlotStatusReceived;
        event Action<bool> OnBuyPlotResult;
        event Action<HousePlaceType, bool> OnUpgradeResult;

        HousePlaceInfo GetHousePlaceInfo(HousePlaceType housePlaceType);
        void GetHouse();
        void GetCurrentHouseUpgrades();
        void GetPlot();
        void BuyPlot();
        void StartUpgrade(HousePlaceType housePlaceType);
        void ApplyUpgrade(HousePlaceType housePlaceType);

        void LoadHouseConfig();
        HousePlaceCraftData GetHouseUpgradeData(HousePlaceType type, int currentLevel);
        bool IsCurrentUpgrade(HousePlaceType type);
        bool IsCompleteUpgrade(HousePlaceType type);
        public bool IsMaxLevel(HousePlaceType type);
        DateTime GetFinishTimeUpgrade(HousePlaceType type);
    }
}

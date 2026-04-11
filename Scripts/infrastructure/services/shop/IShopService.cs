using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using infrastructure.services.npc;
using infrastructure.services.shop.capes;
using infrastructure.services.shop.skins;
using network;

namespace infrastructure.services.shop
{
    public interface IShopService : IReceiver
    {
        List<int> AvailableSkins { get; }
        Dictionary<int, SkinData> Skins { get; }

        List<int> AvailableCapes { get; }
        Dictionary<int, CapeData> Capes { get; }

        event Action OnDataLoaded;
        event Action OnAvailableSkinsLoaded;
        event Action<int> OnAvailableSkinAdded;
        event Action<CurrencyType> OnNotEnoughCurrency;
        event Action<int> OnSkinEquiped;
        event Action OnAvailableCapesLoaded;
        event Action<int> OnAvailableCapeAdded;
        event Action<int> OnCapeEquiped;

        void LoadData();
        UniTask<character.SkinData> GetSkinDataAsync(int skinId);
        void GetSkins();
        bool BuySkin(int skinId);
        bool PutOnSkin(int skinId);

        UniTask<character.CapeData> GetCapeDataAsync(int capeId);
        void GetCapes();
        bool BuyCape(int capeId);
        bool PutOnCape(int capeId);
    }
}

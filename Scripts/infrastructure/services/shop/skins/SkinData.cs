using System;
using infrastructure.services.npc;

namespace infrastructure.services.shop.skins
{
    [Serializable]
    public class SkinData
    {
        public int Id;
        public bool CanBuy;
        public RareType RareType;
        public CurrencyType CurrencyType;
        public int Price;
    }
}
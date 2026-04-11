using System;
using infrastructure.services.npc;

namespace infrastructure.services.shop.capes
{
    [Serializable]
    public class CapeData
    {
        public int Id;
        public bool CanBuy;
        public RareType RareType;
        public CurrencyType CurrencyType;
        public int Price;
    }
}

using infrastructure.services.npc;

namespace infrastructure.services.inventory
{
    public struct PurchasingSlot
    {
        public bool CanBuy;
        public int Price;
        public CurrencyType CurrencyType;
    }
}
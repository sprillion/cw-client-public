using System;
using System.Collections.Generic;

namespace infrastructure.services.npc
{
    [Serializable]
    public class NpcDataJson
    {
        public NpcType NpcType;
        public List<ItemToPurchasing> ItemsToPurchasing;
        public List<ItemToBarter> ItemsToBarter;
    }
    
    [Serializable]
    public class ItemToPurchasing
    {
        public Item Item;
        public CurrencyType CurrencyType;
        public int Price;
        public int NpcLevel;
    }
    
    [Serializable]
    public class ItemToBarter
    {
        public Item Item;
        public List<Item> NeededItems;
    }
    
    [Serializable]
    public class Item
    {
        public int Id;
        public int Count;
    }
    
    public enum CurrencyType : byte
    {
        Gold,
        Diamonds,
    }
}
using System;
using System.Collections.Generic;

namespace infrastructure.services.craft
{
    
    [Serializable]
    public class CraftDataJson
    {
        public int Id;
        public CraftPlaceType CraftPlaceType;
        public float Duration;
        public List<Item> NeededItems;
        public List<Item> ResultItems;
    }

    [Serializable]
    public class Item
    {
        public int Id;
        public int Count;
    }
    
}
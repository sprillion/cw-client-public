using System;
using System.Collections.Generic;
using infrastructure.services.craft;

namespace infrastructure.services.house
{
    [Serializable]
    public class HousePlaceConfigData
    {
        public HousePlaceType HousePlaceType;
        public List<HousePlaceCraftData> HousePlaceCrafts;
    }

    [Serializable]
    public class HousePlaceCraftData
    {
        public float Duration;
        public int RequiredHouseLevel;
        public List<Item> NeededItems;
        public List<int> UpcomingCrafts;
    }
}

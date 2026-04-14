namespace infrastructure.services.house
{
    public class HousePlaceInfo
    {
        public HousePlaceType HousePlaceType;
        public int Level;
        public int RequiredHouseLevel;
        public int MaxLevel;

        public bool IsMaxLevel => Level >= MaxLevel;
    }
}

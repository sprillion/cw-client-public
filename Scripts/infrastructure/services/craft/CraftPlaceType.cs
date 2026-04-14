using System;
using infrastructure.services.house;

namespace infrastructure.services.craft
{
    public enum CraftPlaceType : byte
    {
        Workbench = 0,
        Furnace = 1,
        Anvil = 2,
        Brewing = 3,
        Garden = 4,
    }
    
    public static class CraftPlaceTypeExtencion
    {
        public static HousePlaceType ToHousePlaceType(this CraftPlaceType craftPlaceType)
        {
            return craftPlaceType switch
            {
                CraftPlaceType.Workbench => HousePlaceType.Workbench,
                CraftPlaceType.Furnace => HousePlaceType.Furnace,
                CraftPlaceType.Anvil => HousePlaceType.Anvil,
                CraftPlaceType.Brewing => HousePlaceType.Brewing,
                CraftPlaceType.Garden => HousePlaceType.Garden,
                _ => throw new ArgumentOutOfRangeException(nameof(craftPlaceType), craftPlaceType, null)
            };
        }
    }
}
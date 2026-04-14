using infrastructure.services.house;
using Zenject;

namespace environment.interior.interiors
{
    public class HouseInterior : Interior
    {
        [Inject]
        public void Construct(IHouseService houseService)
        {
            houseService.GetHouse();
            houseService.GetCurrentHouseUpgrades();
        }
    }
}
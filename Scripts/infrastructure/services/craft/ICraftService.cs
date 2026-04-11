using System;
using System.Collections.Generic;
using network;

namespace infrastructure.services.craft
{
    public interface ICraftService : IReceiver
    {
        event Action OnCraftsUpdated;
        void GetAvailableCrafts();
        void GetCurrentCrafts();
        void StartCraft(int craftId);
        void TakeCraftResult(int craftId);

        List<CraftDataJson> GetCraftsFromPlace(CraftPlaceType craftPlaceType);

        bool IsCurrentCraft(int craftId);
        bool IsCompleteCraft(int craftId);
        DateTime GetFinishTimeCraft(int craftId);

        void LoadCraftData();
    }
}
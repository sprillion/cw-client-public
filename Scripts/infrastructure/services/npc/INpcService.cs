using System;
using System.Collections.Generic;
using network;

namespace infrastructure.services.npc
{
    public interface INpcService : IReceiver
    {
        NpcData CurrentNpcData { get; set; }
        int CurrentAttitudeLevel { get; }
        int CurrentAttitudeProgress { get; }
        int CurrentMoneySpent { get; }

        event Action OnNpcLoaded;

        event Action OnAttitudeLoaded;
        
        void LoadAllNpc();
        void GetAttitudeNpcInfo();
        List<ItemToPurchasing> GetShopItems();
    }
}
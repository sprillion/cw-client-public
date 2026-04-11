using System;
using System.Collections.Generic;
using network;
using UnityEngine;

namespace infrastructure.services.npc
{
    public interface INpcService : IReceiver
    {
        IReadOnlyDictionary<NpcType, Npc> Npcs { get; }
        NpcData CurrentNpcData { get; set; }
        int CurrentAttitudeLevel { get; }
        int CurrentAttitudeProgress { get; }
        int CurrentMoneySpent { get; }

        event Action OnNpcLoaded;

        event Action OnAttitudeLoaded;
        
        void LoadAllNpc();
        void GetAttitudeNpcInfo();
        void BuyItem(int itemId);
        List<ItemToPurchasing> GetShopItems();
        Sprite GetNpcAvatarIcon(NpcType npcType);
    }
}
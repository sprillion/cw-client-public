using System;
using network;

namespace infrastructure.services.chests
{
    public interface IChestsService : IReceiver
    {
        event Action OnGetAllItems;
        
        void OpenChest(ushort chestId);
        void GetAllItems(ushort chestId);
        void GetItem(ushort chestId, int itemId);
    }
}
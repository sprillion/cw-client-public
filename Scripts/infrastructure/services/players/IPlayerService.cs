using System;
using network;

namespace infrastructure.services.users
{
    public interface IPlayerService
    {
        Player ClientPlayer { get;  }
        event Action OnClientPlayerLoaded;
        void SetClientPlayer(Message message);
    }
}
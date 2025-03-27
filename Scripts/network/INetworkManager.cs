using System;

namespace network
{
    public interface INetworkManager
    {
        bool ConnectedToServer { get; }
        event Action Update;
        event Action<Message> OnMessageEvent;
        event Action OnServerConnected;

        void SendMessage(Message message);
        void Connect();
        
    }
}
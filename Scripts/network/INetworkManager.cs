using System;

namespace network
{
    public interface INetworkManager : IReceiver
    {
        bool ConnectedToServer { get; }
        event Action Update;
        event Action<Message> OnMessageEvent;
        event Action OnServerConnected;
        event Action OnServerDisconnected;
        event Action<bool> OnVersionCheckResult;

        void SendMessage(Message message);
        void Connect();

    }
}
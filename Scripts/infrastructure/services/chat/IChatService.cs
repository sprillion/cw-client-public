using System;
using System.Collections.Generic;
using network;

namespace infrastructure.services.chat
{
    public interface IChatService : IReceiver
    {
        List<ChatMessage> AllMessages { get; }
        List<ChatMessage> NearbyMessages { get; }
        List<ChatMessage> ClanMessages { get; }
        event Action OnMessagesUpdated;
        ChatMessageType CurrentChatMessageType { get; }
        void SendChatMessage(string messageText);
        void SetChatMessageType(ChatMessageType chatMessageType);
    }
}
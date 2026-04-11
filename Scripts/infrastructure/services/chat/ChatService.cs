using System;
using System.Collections.Generic;
using network;
using tools;

namespace infrastructure.services.chat
{
    public class ChatService : IChatService
    {
        public const int MaxMessagesInChat = 200;
        
        private enum FromClientMessage : byte
        {
            NewMessage,
        }

        private enum FromServerMessage : byte
        {
            NewMessage,
        }
        
        private readonly INetworkManager _networkManager;

        public List<ChatMessage> AllMessages { get; } = new List<ChatMessage>();
        public List<ChatMessage> NearbyMessages { get; } = new List<ChatMessage>();
        public List<ChatMessage> ClanMessages { get; } = new List<ChatMessage>();
        public event Action OnMessagesUpdated;
        public ChatMessageType CurrentChatMessageType { get; private set; }

        public ChatService(INetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            
            switch (type)
            {
                case FromServerMessage.NewMessage:
                    AddMessage(message);
                    break;
            }
        }

        public void SendChatMessage(string messageText)
        {
            var message = new Message(MessageType.Chat);

            message.AddByte(FromClientMessage.NewMessage.ToByte());
            message.AddByte(CurrentChatMessageType.ToByte());
            message.AddString(messageText);
            
            _networkManager.SendMessage(message);
        }

        public void SetChatMessageType(ChatMessageType chatMessageType)
        {
            CurrentChatMessageType = chatMessageType;
        }

        private void AddMessage(Message message)
        {
            var chatType = (ChatMessageType)message.GetByte();
            
            var chatMessage = new ChatMessage()
            {
                ChatMessageType = chatType,
                PlayerId = message.GetInt(),
                Message = message.GetString(),
                Nickname = message.GetString(),
            };

            var hasClan = message.GetBool();
            if (hasClan)
            {
                chatMessage.Clan = message.GetString();
            }

            switch (chatType)
            {
                case ChatMessageType.World:
                    AllMessages.Add(chatMessage);
                    break;
                case ChatMessageType.Nearby:
                    AllMessages.Add(chatMessage);
                    NearbyMessages.Add(chatMessage);
                    break;
                case ChatMessageType.Clan:
                    AllMessages.Add(chatMessage);
                    ClanMessages.Add(chatMessage);
                    break;
            }
            
            CheckCountMessages();
            OnMessagesUpdated?.Invoke();
        }

        private void CheckCountMessages()
        {
            while (NearbyMessages.Count > MaxMessagesInChat)
            {
                NearbyMessages.RemoveAt(NearbyMessages.Count - 1);
            }
            while (ClanMessages.Count > MaxMessagesInChat)
            {
                ClanMessages.RemoveAt(ClanMessages.Count - 1);
            }
            while (AllMessages.Count > MaxMessagesInChat * 3)
            {
                AllMessages.RemoveAt(AllMessages.Count - 1);
            }
        }
    }
}
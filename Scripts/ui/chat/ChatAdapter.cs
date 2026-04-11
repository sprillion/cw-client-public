using System;
using System.Collections.Generic;
using infrastructure.services.chat;
using UnityEngine;

namespace ui.chat
{
    public class ChatAdapter : Adapter
    {
        [SerializeField] private ChatMessageView _chatMessageViewPrefab;
        
        private List<ChatMessage> _chatMessages;

        public override event Action OnDataChange;

        public override int GetItemCount()
        {
            return _chatMessages.Count;
        }

        public override GameObject CreateView(int index, Transform parent)
        {
            var message = Instantiate(_chatMessageViewPrefab, parent);
            return message.gameObject;
        }

        public override void BindView(GameObject view, int index)
        {
            var message = view.GetComponent<ChatMessageView>();
            message.Bind(_chatMessages[index]);
        }
        
        public void SetMessages(List<ChatMessage> messages)
        {
            _chatMessages = messages;
            OnDataChange?.Invoke();
        }
    }
}
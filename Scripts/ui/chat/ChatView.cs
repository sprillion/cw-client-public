using infrastructure.services.chat;
using infrastructure.services.input;
using TMPro;
using ui.popup;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.chat
{
    public class ChatView : Popup
    {
        [SerializeField] private ChatAdapter _adapter;
        [SerializeField] private ScrollRect _scrollRect;
        
        [SerializeField] private TMP_InputField _inputField;

        [SerializeField] private Button _worldButton;
        [SerializeField] private Button _nearbyButton;
        [SerializeField] private Button _clanButton;
        
        [SerializeField] private Button _sendButton;
        [SerializeField] private KeyboardButton _keyboardButton;

        private IChatService _chatService;
        private IInputService _inputService;

        [Inject]
        public void Construct(IChatService chatService, IInputService inputService)
        {
            _chatService = chatService;
            _inputService = inputService;

            _chatService.OnMessagesUpdated += OnMessagesUpdated;
            
            _worldButton.onClick.AddListener(() => ChangeChatType(ChatMessageType.World));
            _nearbyButton.onClick.AddListener(() => ChangeChatType(ChatMessageType.Nearby));
            _clanButton.onClick.AddListener(() => ChangeChatType(ChatMessageType.Clan));
            
            _sendButton.onClick.AddListener(SendMessage);
            
            _inputField.onValueChanged.AddListener(OnTextChanged);
            _inputField.onSelect.AddListener(OnInputFieldSelected);
            _inputField.onDeselect.AddListener(OnInputFieldDeselected);

            _inputService.OnChangeCursorEvent += DeselectInputField;

            _sendButton.interactable = false;
            
            ChangeChatType(ChatMessageType.World);
        }

        private void OnMessagesUpdated()
        {
            var needUpdateScrollPosition = _scrollRect.verticalNormalizedPosition < 0.05f; 
            
            _adapter.SetMessages(_chatService.CurrentChatMessageType switch
            {
                ChatMessageType.World => _chatService.AllMessages,
                ChatMessageType.Nearby => _chatService.NearbyMessages,
                ChatMessageType.Clan => _chatService.ClanMessages,
                _ => null
            });

            if (needUpdateScrollPosition)
            {
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }
        
        private void ChangeChatType(ChatMessageType chatMessageType)
        {
            _chatService.SetChatMessageType(chatMessageType);
            
            _worldButton.interactable = true;
            _nearbyButton.interactable = true;
            _clanButton.interactable = true;

            switch (chatMessageType)
            {
                case ChatMessageType.World:
                    _worldButton.interactable = false;
                    break;
                case ChatMessageType.Nearby:
                    _nearbyButton.interactable = false;
                    break;
                case ChatMessageType.Clan:
                    _clanButton.interactable = false;
                    break;
            }
            
            OnMessagesUpdated();
        }

        private void SendMessage()
        {
            var text = _inputField.text;
            _inputField.text = "";
            if (!TextIsValid(text)) return;   
            _chatService.SendChatMessage(text);
        }

        private void OnTextChanged(string text)
        {
            _sendButton.interactable = TextIsValid(text);
        }

        private bool TextIsValid(string text)
        {
            text = text.Replace(" ", "");
            return !string.IsNullOrEmpty(text);
        }

        private void OnInputFieldSelected(string _)
        {
            _inputService.DisableFullInput();
            _keyboardButton.enabled = true;
        }
        
        private void OnInputFieldDeselected(string _)
        {
            _inputService.EnableFullInput();
            _keyboardButton.enabled = false;
        }

        private void DeselectInputField()
        {
            if (!_inputService.CursorIsLocked) return;
            _inputField.DeactivateInputField();
        }
    }
}
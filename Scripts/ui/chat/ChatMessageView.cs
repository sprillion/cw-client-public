using System;
using infrastructure.services.chat;
using TMPro;
using tools;
using UnityEngine;
using UnityEngine.UI;

namespace ui.chat
{
    public class ChatMessageView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nickname;
        [SerializeField] private TMP_Text _message;

        [SerializeField] private Image _background;

        [SerializeField] private Color _worldColor;
        [SerializeField] private Color _nearbyColor;
        [SerializeField] private Color _clanColor;

        public void Bind(ChatMessage chatMessage)
        {
            _message.text = chatMessage.Message;
            if (string.IsNullOrEmpty(chatMessage.Clan))
            {
                _nickname.text = chatMessage.Nickname;
            }
            else
            {
                _nickname.text = $"{chatMessage.Clan.SetClanColor().SetSize(15)}/n{chatMessage.Nickname}";
            }

            _background.color = chatMessage.ChatMessageType switch
            {
                ChatMessageType.World => _worldColor,
                ChatMessageType.Nearby => _nearbyColor,
                ChatMessageType.Clan => _clanColor,
                _ => new Color()
            };
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}
using System;
using infrastructure.services.saveLoad;
using infrastructure.services.users;
using network;
using tools;
using UnityEngine;

namespace infrastructure.services.auth
{
    public class Authorization : IAuthorization
    {
        private enum FromClientMessages : byte
        {
            Registration = 0,
            Login = 1,
            LoginVK = 2,
            LoginYandex = 3,
            LoginGoogle = 4,
            ValidateNickname = 5,
            LinkVK = 6,
            LinkYandex = 7,
            LinkGoogle = 8,
            RequestLinkedPlatforms = 9,
            LoginVKGames = 10,
        }

        private enum FromServerMessage : byte
        {
            Registration = 0,
            Login = 1,
            LoginVK = 2,
            LoginYandex = 3,
            LoginGoogle = 4,
            NicknameResult = 5,
            LinkedPlatforms = 6,
            LinkResult = 7,
            Error = 255,
        }

        public event Action OnAuthRequired;
        public event Action OnNicknameValid;
        public event Action<string> OnNicknameError;
        public event Action<LinkedPlatformsData> OnLinkedPlatformsReceived;
        public event Action<LinkResultData> OnLinkResult;

        private readonly INetworkManager _networkManager;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IPlayerService _playerService;

        private readonly ConnectionData _connectionData;

        public Authorization(INetworkManager networkManager, ISaveLoadService saveLoadService,
            IPlayerService playerService)
        {
            _networkManager = networkManager;
            _saveLoadService = saveLoadService;
            _playerService = playerService;

            _connectionData = GameResources.Data.connection_data<ConnectionData>();
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            switch (type)
            {
                case FromServerMessage.Registration:
                    UserRegistered(message);
                    break;
                case FromServerMessage.Login:
                    UserLogged(message);
                    break;
                case FromServerMessage.LoginVK:
                    UserLoggedVK(message);
                    break;
                case FromServerMessage.LoginYandex:
                    UserLoggedYandex(message);
                    break;
                case FromServerMessage.LoginGoogle:
                    UserLoggedGoogle(message);
                    break;
                case FromServerMessage.NicknameResult:
                    HandleNicknameResult(message);
                    break;
                case FromServerMessage.LinkedPlatforms:
                    HandleLinkedPlatforms(message);
                    break;
                case FromServerMessage.LinkResult:
                    HandleLinkResult(message);
                    break;
                case FromServerMessage.Error:
                    Debug.LogWarning(message.GetString());
                    break;
            }
        }

        public void Authorize()
        {
            if (_saveLoadService.HasToken() ||
                (Application.isEditor && !string.IsNullOrEmpty(_connectionData.GetToken())))
            {
                Login();
            }
            else
            {
                OnAuthRequired?.Invoke();
            }
        }

        public void ValidateNickname(string nickname)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.ValidateNickname.ToByte()).AddString(nickname);
            _networkManager.SendMessage(message);
        }

        public void Registration(string nickname, int skinId)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.Registration.ToByte()).AddString(nickname).AddInt(skinId);
            _networkManager.SendMessage(message);
        }

        public void LoginVK(string vkToken)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.LoginVK.ToByte());
            message.AddString(vkToken);
            _networkManager.SendMessage(message);
        }

        public void LoginVKGames(string signedParams)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.LoginVKGames.ToByte());
            message.AddString(signedParams);
            _networkManager.SendMessage(message);
        }

        public void LoginYandex(string yandexToken)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.LoginYandex.ToByte());
            message.AddString(yandexToken);
            _networkManager.SendMessage(message);
        }

        public void LoginGoogle(string googleToken)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.LoginGoogle.ToByte());
            message.AddString(googleToken);
            _networkManager.SendMessage(message);
        }

        public void RequestLinkedPlatforms()
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.RequestLinkedPlatforms.ToByte());
            _networkManager.SendMessage(message);
        }

        public void LinkVK(string token)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.LinkVK.ToByte());
            message.AddString(token);
            _networkManager.SendMessage(message);
        }

        public void LinkYandex(string token)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.LinkYandex.ToByte());
            message.AddString(token);
            _networkManager.SendMessage(message);
        }

        public void LinkGoogle(string token)
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.LinkGoogle.ToByte());
            message.AddString(token);
            _networkManager.SendMessage(message);
        }

        private void Login()
        {
            var message = new Message(MessageType.Auth);
            message.AddByte(FromClientMessages.Login.ToByte());

            string token = "";

            if (Application.isEditor)
            {
                token = _connectionData.GetToken();
            }

            if (string.IsNullOrEmpty(token))
            {
                token = _saveLoadService.GetToken();
            }

            message.AddString(token);
            _networkManager.SendMessage(message);
        }

        private void HandleLinkedPlatforms(Message message)
        {
            var data = new LinkedPlatformsData
            {
                HasVK = message.GetBool(),
                HasYandex = message.GetBool(),
                HasGoogle = message.GetBool()
            };
            OnLinkedPlatformsReceived?.Invoke(data);
        }

        private void HandleLinkResult(Message message)
        {
            var data = new LinkResultData
            {
                Success = message.GetBool(),
                Platform = message.GetString(),
                Error = message.GetString()
            };
            OnLinkResult?.Invoke(data);
        }

        private void HandleNicknameResult(Message message)
        {
            var isValid = message.GetBool();
            if (isValid)
                OnNicknameValid?.Invoke();
            else
                OnNicknameError?.Invoke(message.GetString());
        }

        private void UserRegistered(Message message)
        {
            var token = message.GetString();
            _saveLoadService.SetToken(token);
            Login();
        }

        private void UserLogged(Message message)
        {
            _playerService.SetClientPlayer(message);
        }

        private void UserLoggedVK(Message message)
        {
            var sessionToken = message.GetString();
            _saveLoadService.SetToken(sessionToken);
            _playerService.SetClientPlayer(message);
        }

        private void UserLoggedYandex(Message message)
        {
            var sessionToken = message.GetString();
            _saveLoadService.SetToken(sessionToken);
            _playerService.SetClientPlayer(message);
        }

        private void UserLoggedGoogle(Message message)
        {
            var sessionToken = message.GetString();
            _saveLoadService.SetToken(sessionToken);
            _playerService.SetClientPlayer(message);
        }
    }
}
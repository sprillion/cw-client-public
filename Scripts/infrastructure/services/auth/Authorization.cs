using System;
using infrastructure.services.saveLoad;
using infrastructure.services.users;
using network;

namespace infrastructure.services.auth
{
    public class Authorization : IAuthorization
    {
        private enum FromClientMessages : byte
        {
            Registration,
            Login,
        }
        
        private enum FromServerMessage : byte
        {
            Registration,
            Login,
        }

        private readonly INetworkManager _networkManager;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IPlayerService _playerService;

        public Authorization(INetworkManager networkManager, ISaveLoadService saveLoadService, IPlayerService playerService)
        {
            _networkManager = networkManager;
            _saveLoadService = saveLoadService;
            _playerService = playerService;
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Authorize()
        {
            if (_saveLoadService.HasToken())
            {
                Login();
            }
            else
            {
                Registration();
            }
        }

        private void Registration()
        {
            var message = new Message(ClientToServerId.Auth);
            message.AddByte((byte)FromClientMessages.Registration);
            _networkManager.SendMessage(message);
        }

        private void Login()
        {
            var message = new Message(ClientToServerId.Auth);
            message.AddByte((byte)FromClientMessages.Login);
            message.AddString(_saveLoadService.GetToken());
            _networkManager.SendMessage(message);
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
    }
}
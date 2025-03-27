using System;
using infrastructure.services.players;
using network;

namespace infrastructure.services.users
{
    public class PlayerService : IPlayerService
    {
        
        private readonly ICharacterService _characterService;
        
        public Player ClientPlayer { get; private set; }

        public event Action OnClientPlayerLoaded;

        public PlayerService(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        public void SetClientPlayer(Message message)
        {
            var id = message.GetInt();
            var nickname = message.GetString();
            var position = message.GetVector3();
            
            ClientPlayer = new Player(id, nickname, position);
            ClientPlayer.SetCharacter(_characterService.CurrentCharacter);
            OnClientPlayerLoaded?.Invoke();
        }
    }
}
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
            var playerId = message.GetInt();
            var characterId = message.GetInt();
            var nickname = message.GetString();
            var position = message.GetVector3();
            var isDead = message.GetBool();
            var skin = message.GetInt();
            var role = (PlayerRole)message.GetByte();

            ClientPlayer = new Player(playerId, nickname, position, role);
            ClientPlayer.SetCharacter(_characterService.CurrentCharacter);
            _characterService.CurrentCharacter.Id = characterId;
            _characterService.SetMainSkin(skin);
            OnClientPlayerLoaded?.Invoke();
            if (isDead)
            {
                _characterService.DeathCurrentPlayer();
            }
        }
    }
}
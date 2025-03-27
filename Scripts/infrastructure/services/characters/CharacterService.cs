using System;
using System.Collections.Generic;
using character;
using factories.characters;
using network;

namespace infrastructure.services.players
{
    public class CharacterService : ICharacterService
    {
        public enum FromClientMessage : byte
        {
            Move,
            Hit,
            Throwing,
        }
        
        private enum FromServerMessage : byte
        {
            Create,
            Remove,
            Positions,
            AllStats,
            Stat,
        }
        
        private readonly ICharacterFactory _characterFactory;
        private readonly INetworkManager _networkManager;
        
        private readonly Dictionary<int, EnemyCharacter> _characters = new Dictionary<int, EnemyCharacter>();
        
        public Character CurrentCharacter { get; private set; }
        public event Action OnStatsLoaded; 
        
        public CharacterService(ICharacterFactory characterFactory, INetworkManager networkManager)
        {
            _characterFactory = characterFactory;
            _networkManager = networkManager;
            
            CreateCurrentCharacter();
        }

        public void CreateCurrentCharacter()
        {
            CurrentCharacter = _characterFactory.CreateCharacter();
        }

        public void LaunchSendPosition()
        {
            _networkManager.Update += SendCharacterPosition;
        }
        
        public EnemyCharacter GetNewEnemy(int id, string nickname)
        {
            var character = _characterFactory.CreateEnemy(nickname);
            _characters.Add(id, character);
            return character;
        }
        
        public void RemoveCharacter(int id)
        {
            if (!_characters.TryGetValue(id, out var enemyCharacter)) return;
            enemyCharacter.Release();
            _characters.Remove(id);
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            switch (type)
            {
                case FromServerMessage.Create:
                    CreateCharacter(message);
                    break;
                case FromServerMessage.Remove:
                    RemoveCharacter(message);
                    break;
                case FromServerMessage.Positions:
                    MoveCharacters(message);
                    break;
                case FromServerMessage.AllStats:
                    SetAllStatsCharacter(message);
                    break;
                case FromServerMessage.Stat:
                    SetStat(message);
                    break;
            }
        }

        private void CreateCharacter(Message message)
        {
            var id = message.GetInt();
            var nickname = message.GetString();
            var position = message.GetVector3();
            var rotation = message.GetFloat();

            var character = _characterFactory.CreateEnemy(nickname);
            _characters.TryAdd(id, character);
            character.SetPosition(position, rotation);
        }

        private void RemoveCharacter(Message message)
        {
            var id = message.GetInt();
            if (!_characters.TryGetValue(id, out var character)) return;
            character?.Release();
            _characters.Remove(id);
        }
        
        private void MoveCharacters(Message message)
        {
            int count = message.GetUShort();
            for (int i = 0; i < count; i++)
            {
                var id = message.GetInt();
                var position = message.GetVector3();
                var rotation = message.GetFloat();
                if (_characters.TryGetValue(id, out var character))
                {
                    character?.SetPosition(position, rotation);
                }
            }
        }

        private void SendCharacterPosition()
        {
            if (CurrentCharacter == null) return;
            var message = new Message(ClientToServerId.Character);
            message.AddByte((byte)FromClientMessage.Move);
            message.AddVector3(CurrentCharacter.transform.position);
            message.AddFloat(CurrentCharacter.transform.rotation.eulerAngles.y);
            _networkManager.SendMessage(message);
        }


        private void SetAllStatsCharacter(Message message)
        {
            CurrentCharacter.CharacterStats.SetGold(message.GetInt());
            CurrentCharacter.CharacterStats.SetDiamonds(message.GetInt());
            CurrentCharacter.CharacterStats.SetPurchasedDiamonds(message.GetInt());
            CurrentCharacter.CharacterStats.SetMaxHealth(message.GetInt());
            CurrentCharacter.CharacterStats.SetCurrentHealth(message.GetInt());
            CurrentCharacter.CharacterStats.SetLevel(message.GetInt());
            CurrentCharacter.CharacterStats.SetNeededExperience(message.GetInt());
            CurrentCharacter.CharacterStats.SetExperience(message.GetInt());
            
            OnStatsLoaded?.Invoke();
        }

        private void SetStat(Message message)
        {
            var statType = (StatType)message.GetByte();
            
            switch (statType)
            {
                case StatType.Gold:
                    CurrentCharacter.CharacterStats.SetGold(message.GetInt());
                    break;
                case StatType.Diamonds:
                    CurrentCharacter.CharacterStats.SetDiamonds(message.GetInt());
                    break;
                case StatType.PurchasedDiamonds:
                    CurrentCharacter.CharacterStats.SetPurchasedDiamonds(message.GetInt());
                    break;
                case StatType.MaxHealth:
                    CurrentCharacter.CharacterStats.SetMaxHealth(message.GetInt());
                    CurrentCharacter.CharacterStats.SetCurrentHealth(message.GetInt());
                    break;
                case StatType.Health:
                    CurrentCharacter.CharacterStats.SetCurrentHealth(message.GetInt());
                    break;
                case StatType.Level:
                    CurrentCharacter.CharacterStats.SetLevel(message.GetInt());
                    CurrentCharacter.CharacterStats.SetNeededExperience(message.GetInt());
                    break;
                case StatType.Experience:
                    CurrentCharacter.CharacterStats.SetExperience(message.GetInt());
                    break;
            }
        }
    }
}
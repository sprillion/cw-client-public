using System;
using System.Collections.Generic;
using character;
using DG.Tweening;
using factories.characters;
using factories.inventory;
using infrastructure.services.input;
using infrastructure.services.inventory.items;
using network;
using tools;
using UnityEngine;

namespace infrastructure.services.players
{
    public class CharacterService : ICharacterService
    {
        public enum FromClientMessage : byte
        {
            Move,
            Hit,
            Throwing,
            ChangeHandItem,
            Revival,
        }

        private enum FromServerMessage : byte
        {
            Create,
            Remove,
            Positions,
            AllStats,
            Stat,
            Throwing,
            ChangeSkin,
            ChangeCape,
            ChangeArmor,
            ChangeHandItem,
            Death,
            Revival,
        }

        private readonly ICharacterFactory _characterFactory;
        private readonly INetworkManager _networkManager;
        private readonly IInventoryFactory _inventoryFactory;
        private readonly IInputService _inputService;

        private readonly Dictionary<int, EnemyCharacter> _characters = new Dictionary<int, EnemyCharacter>();

        public Character CurrentCharacter { get; private set; }
        public IReadOnlyDictionary<int, EnemyCharacter> OtherCharacters => _characters;
        public event Action OnStatsLoaded;
        public event Action OnCurrentCharacterDead;
        public event Action OnCurrentCharacterRevival;
        public event Action<int> OnSkinChanged;
        public event Action<int> OnCapeChanged;
        public event Action<int, int, Vector3, float, int> OnCharacterMounted;

        public CharacterService(ICharacterFactory characterFactory, INetworkManager networkManager,
            IInventoryFactory inventoryFactory, IInputService inputService)
        {
            _characterFactory = characterFactory;
            _networkManager = networkManager;
            _inventoryFactory = inventoryFactory;
            _inputService = inputService;
            
            CreateCurrentCharacter();
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
                case FromServerMessage.Throwing:
                    break;
                case FromServerMessage.ChangeSkin:
                    ChangeSkin(message);
                    break;
                case FromServerMessage.ChangeCape:
                    ChangeCape(message);
                    break;
                case FromServerMessage.ChangeArmor:
                    ChangeArmor(message);
                    break;
                case FromServerMessage.ChangeHandItem:
                    ChangeHandItem(message);
                    break;
                case FromServerMessage.Death:
                    Death(message);
                    break;
                case FromServerMessage.Revival:
                    RevivalRespone(message);
                    break;
            }
        }

        public void CreateCurrentCharacter()
        {
            CurrentCharacter = _characterFactory.CreateCharacter();
            CurrentCharacter.CharacterSkin.Initialize();
        }

        public void LaunchSendPosition()
        {
            _networkManager.Update += SendCharacterPosition;
        }

        public void StopSendPosition()
        {
            _networkManager.Update -= SendCharacterPosition;
        }

        public void ResumeSendPosition()
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

        public void UpdateHandItem(int itemId)
        {
            var message = new Message(MessageType.Character)
                .AddByte(FromClientMessage.ChangeHandItem.ToByte())
                .AddInt(itemId);
            _networkManager.SendMessage(message);
        }

        public void DeathCurrentPlayer()
        {
            CurrentCharacter.IsDead = true;
            CurrentCharacter.CharacterMovement.CharacterAnimator.Death();
            _inputService.DisableFullInput();
            OnCurrentCharacterDead?.Invoke();
        }

        public void RevivalRequest()
        {
            var message = new Message(MessageType.Character)
                .AddByte(FromClientMessage.Revival.ToByte());
            _networkManager.SendMessage(message);
        }

        public void SetMainSkin(int skinId)
        {
            CurrentCharacter.CharacterSkin.SetSkin(skinId);
            OnSkinChanged?.Invoke(skinId);
        }

        public void SetMainCape(int capeId)
        {
            CurrentCharacter.CharacterSkin.SetCape(capeId);
            OnCapeChanged?.Invoke(capeId);
        }

        private void CreateCharacter(Message message)
        {
            var id = message.GetInt();
            var nickname = message.GetString();
            var position = message.GetVector3();
            var rotation = message.GetFloat();
            var isDeath = message.GetBool();
            var skinId = message.GetInt();
            var capeId = message.GetInt();
            var headArmor = message.GetByteEnum<ArmorType>();
            var bodyArmor = message.GetByteEnum<ArmorType>();
            var legsArmor = message.GetByteEnum<ArmorType>();
            var footArmor = message.GetByteEnum<ArmorType>();

            var character = _characterFactory.CreateEnemy(nickname);
            _characters.TryAdd(id, character);
            character.EnemyController.ApplySnapshot(new EnemySnapshot(NetworkManager.CurrentTick, position, rotation));
            character.CharacterSkin.Initialize();
            character.CharacterSkin.SetSkin(skinId);
            character.CharacterSkin.SetCape(capeId);
            character.CharacterSkin.SetArmor(headArmor, ArmorPlaceType.Head);
            character.CharacterSkin.SetArmor(bodyArmor, ArmorPlaceType.Body);
            character.CharacterSkin.SetArmor(legsArmor, ArmorPlaceType.Legs);
            character.CharacterSkin.SetArmor(footArmor, ArmorPlaceType.Foot);
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
            var tick = message.GetInt();
            NetworkManager.UpdateTick(tick);

            int count = message.GetUShort();
            for (int i = 0; i < count; i++)
            {
                var id = message.GetInt();
                var position = message.GetVector3();
                var rotation = message.GetFloat();
                var isMounted = message.GetBool();
                var transportTypeId = isMounted ? message.GetInt() : 0;

                if (_characters.TryGetValue(id, out var character))
                {
                    character?.EnemyController.ApplySnapshot(new EnemySnapshot(tick, position, rotation));
                }

                if (isMounted)
                    OnCharacterMounted?.Invoke(id, tick, position, rotation, transportTypeId);
            }
        }

        private void SendCharacterPosition()
        {
            if (CurrentCharacter == null) return;
            if (CurrentCharacter.IsDead) return;
            var message = new Message(MessageType.Character)
                .AddByte(FromClientMessage.Move.ToByte())
                .AddVector3(CurrentCharacter.transform.position)
                .AddFloat(CurrentCharacter.transform.rotation.eulerAngles.y);
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
            CurrentCharacter.CharacterStats.SetDamage(message.GetInt());
            CurrentCharacter.CharacterStats.SetAttackSpeed(message.GetFloat());

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
                case StatType.Armor:
                    CurrentCharacter.CharacterStats.SetArmor(message.GetInt());
                    break;
                case StatType.Damage:
                    CurrentCharacter.CharacterStats.SetDamage(message.GetInt());
                    break;
                case StatType.AttackSpeed:
                    CurrentCharacter.CharacterStats.SetAttackSpeed(message.GetFloat());
                    break;
            }
        }

        private void ChangeArmor(Message message)
        {
            var id = message.GetInt();
            var headArmor = message.GetByteEnum<ArmorType>();
            var bodyArmor = message.GetByteEnum<ArmorType>();
            var legsArmor = message.GetByteEnum<ArmorType>();
            var footArmor = message.GetByteEnum<ArmorType>();
            CharacterSkin characterSkin = null;

            if (CurrentCharacter.Id == id)
            {
                characterSkin = CurrentCharacter.CharacterSkin;
            }
            else
            {
                if (_characters.TryGetValue(id, out var character))
                {
                    characterSkin = character.CharacterSkin;
                }
            }

            if (characterSkin == null) return;

            characterSkin.SetArmor(headArmor, ArmorPlaceType.Head);
            characterSkin.SetArmor(bodyArmor, ArmorPlaceType.Body);
            characterSkin.SetArmor(legsArmor, ArmorPlaceType.Legs);
            characterSkin.SetArmor(footArmor, ArmorPlaceType.Foot);
        }
        
        private void ChangeSkin(Message message)
        {
            var characterId = message.GetInt();
            var skinId = message.GetInt();
            
            CharacterSkin characterSkin = null;

            if (CurrentCharacter.Id == characterId)
            {
                characterSkin = CurrentCharacter.CharacterSkin;
                OnSkinChanged?.Invoke(skinId);
            }
            else
            {
                if (_characters.TryGetValue(characterId, out var character))
                {
                    characterSkin = character.CharacterSkin;
                }
            }

            if (characterSkin == null) return;

            characterSkin.SetSkin(skinId);
        }

        private void ChangeCape(Message message)
        {
            var characterId = message.GetInt();
            var capeId = message.GetInt();

            CharacterSkin characterSkin = null;

            if (CurrentCharacter.Id == characterId)
            {
                characterSkin = CurrentCharacter.CharacterSkin;
                OnCapeChanged?.Invoke(capeId);
            }
            else
            {
                if (_characters.TryGetValue(characterId, out var character))
                {
                    characterSkin = character.CharacterSkin;
                }
            }

            if (characterSkin == null) return;

            characterSkin.SetCape(capeId);
        }

        private void ChangeHandItem(Message message)
        {
            var id = message.GetInt();
            var itemId = message.GetInt();

            if (CurrentCharacter.Id == id) return;

            if (!_characters.TryGetValue(id, out var character)) return;
            Item item = null;
            if (itemId >= 0)
            {
                item = new Item
                {
                    Id = itemId,
                };
            }
            character.HandItemsController.ChangeHandItem(item);
        }

        private void Death(Message message)
        {
            var id = message.GetInt();
            
            if (CurrentCharacter.Id == id)
            {
                DeathCurrentPlayer();
            }
            else
            {
                if (_characters.TryGetValue(id, out var character))
                {
                    
                }
            }
        }

        private void RevivalRespone(Message message)
        {
            var position = message.GetVector3();
            CurrentCharacter.IsDead = false;
            CurrentCharacter.CharacterMovement.SetPosition(position);
            CurrentCharacter.CharacterMovement.CharacterAnimator.Revivel();
            OnCurrentCharacterRevival?.Invoke();
            _inputService.DisableFullInput();
            DOVirtual.DelayedCall(5f, () => _inputService.EnableFullInput());
        }
    }
}
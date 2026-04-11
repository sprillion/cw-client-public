using System;
using System.Collections.Generic;
using character;
using network;
using UnityEngine;

namespace infrastructure.services.players
{
    public interface ICharacterService : IReceiver
    {
        Character CurrentCharacter { get; }
        IReadOnlyDictionary<int, EnemyCharacter> OtherCharacters { get; }
        event Action OnStatsLoaded;
        event Action OnCurrentCharacterDead;
        event Action OnCurrentCharacterRevival;
        event Action<int> OnSkinChanged;
        event Action<int> OnCapeChanged;
        // (characterId, tick, position, rotation, transportTypeId)
        event Action<int, int, Vector3, float, int> OnCharacterMounted;

        void CreateCurrentCharacter();
        void LaunchSendPosition();
        void StopSendPosition();
        void ResumeSendPosition();
        EnemyCharacter GetNewEnemy(int id, string nickname);
        void RemoveCharacter(int id);
        void UpdateHandItem(int itemId);
        void DeathCurrentPlayer();
        void RevivalRequest();
        void SetMainSkin(int skinId);
        void SetMainCape(int capeId);
    }
}

using System;
using character;
using network;

namespace infrastructure.services.players
{
    public interface ICharacterService : IReceiver
    {
        Character CurrentCharacter { get; }
        event Action OnStatsLoaded;

        void CreateCurrentCharacter();
        void LaunchSendPosition();
        EnemyCharacter GetNewEnemy(int id, string nickname);
        void RemoveCharacter(int id);
    }
}
using System;
using System.Collections.Generic;
using network;

namespace infrastructure.services.lumber
{
    public interface ILumberService : IReceiver
    {
        int CurrentTreeId { get; set; }
        int CurrentHealth { get; }
        int MaxHealth { get; }
        float CooldownRemaining { get; }
        int CurrentEnergy { get; }
        int MaxEnergy { get; }
        int RecoveryEnergyDuration { get; }
        DateTimeOffset EnergyUpdateTime { get; }
        IReadOnlyList<(int itemId, int count)> LastDrops { get; }

        event Action OnInfoUpdated;
        event Action OnHitResult;
        event Action OnCooldown;

        void GetInfo(int treeId);
        void Hit(int treeId);
        bool UpdateLocalEnergy();
    }
}

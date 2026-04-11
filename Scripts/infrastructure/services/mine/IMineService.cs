using System;
using network;

namespace infrastructure.services.mine
{
    public interface IMineService : IReceiver
    {
        int BlockHealth { get; }
        int CountDestroyed { get; }
        int CurrentEnergy { get; }
        int MaxEnergy { get; }
        int RecoveryEnergyDuration { get; }
        DateTimeOffset EnergyUpdateTime { get; }

        event Action OnInfoUpdated;
        event Action OnHitResult;
        event Action<MineResourceType> OnReward;
        void GetInfo();
        void Hit();
        float GetChancePercent(MineResourceType type);
        bool UpdateLocalEnergy();
    }
}
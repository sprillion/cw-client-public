using System;
using System.Collections.Generic;
using System.Linq;
using network;
using tools;
using UnityEngine;

namespace infrastructure.services.mine
{
    public class MineService : IMineService
    {
        public enum FromClientMessage : byte
        {
            GetInfo,
            Hit,
        }

        public enum FromServerMessage : byte
        {
            Info,
            HitResult,
        }

        private readonly INetworkManager _networkManager;
        private readonly Dictionary<MineResourceType, int> _chances = new Dictionary<MineResourceType, int>();
        
        public int BlockHealth { get; private set; }
        public int CountDestroyed { get; private set; }
        public int CurrentEnergy { get; private set; }
        public int MaxEnergy { get; private set; }
        public int RecoveryEnergyDuration { get; private set; }
        public DateTimeOffset EnergyUpdateTime { get; private set; }

        public event Action OnInfoUpdated;
        public event Action OnHitResult;
        public event Action<MineResourceType> OnReward;
        
        public MineService(INetworkManager networkManager)
        {
            _networkManager = networkManager;
        }
        
        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.Info:
                    SetInfo(message);
                    break;
                case FromServerMessage.HitResult:
                    SetHitResult(message);
                    break;
            }
        }

        public void GetInfo()
        {
            var message = new Message(MessageType.Mine)
                .AddByte(FromClientMessage.GetInfo.ToByte());
            
            _networkManager.SendMessage(message);
        }

        public void Hit()
        {
            var message = new Message(MessageType.Mine)
                .AddByte(FromClientMessage.Hit.ToByte());
            
            _networkManager.SendMessage(message);
        }
        
        public float GetChancePercent(MineResourceType type)
        {
            float totalWeight = _chances.Values.Sum();
            return _chances[type] / totalWeight * 100f;
        }

        public bool UpdateLocalEnergy()
        {
            if (RecoveryEnergyDuration == 0) return false;
            
            var time = NetworkManager.ServerNow - EnergyUpdateTime;

            var recoveredEnergy = (int)time.TotalSeconds / RecoveryEnergyDuration; 
            
            if (recoveredEnergy > 0)
            {
                CurrentEnergy = Mathf.Clamp(CurrentEnergy + recoveredEnergy, 0, MaxEnergy);
                EnergyUpdateTime = DateTimeOffset.UtcNow;
            }

            return recoveredEnergy > 0;
        }

        private void SetInfo(Message message)
        {
            BlockHealth = message.GetByte();
            CountDestroyed = message.GetInt();
            _chances[MineResourceType.Stone] = message.GetByte();
            _chances[MineResourceType.Coal] = message.GetByte();
            _chances[MineResourceType.Iron] = message.GetByte();
            _chances[MineResourceType.Gold] = message.GetByte();
            _chances[MineResourceType.Diamond] = message.GetByte();
            CurrentEnergy = message.GetInt();
            MaxEnergy = message.GetInt();
            RecoveryEnergyDuration = message.GetInt();
            EnergyUpdateTime = DateTimeOffset.FromUnixTimeMilliseconds(message.GetLong()).UtcDateTime;
            OnInfoUpdated?.Invoke();
        }

        private void SetHitResult(Message message)
        {
            BlockHealth = message.GetByte();
            CountDestroyed = message.GetInt();
            CurrentEnergy = message.GetInt();
            EnergyUpdateTime = DateTimeOffset.FromUnixTimeMilliseconds(message.GetLong()).UtcDateTime;

            OnHitResult?.Invoke();
            
            var blockDestroyed = message.GetBool();
            if (blockDestroyed)
            {
                var reward = message.GetByteEnum<MineResourceType>();
                OnReward?.Invoke(reward);
            }
        }
    }
}
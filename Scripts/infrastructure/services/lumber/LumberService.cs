using System;
using System.Collections.Generic;
using network;
using tools;
using UnityEngine;

namespace infrastructure.services.lumber
{
    public class LumberService : ILumberService
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
            Cooldown,
        }

        private readonly INetworkManager _networkManager;
        private readonly List<(int itemId, int count)> _lastDrops = new List<(int itemId, int count)>();

        public int CurrentTreeId { get; set; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public float CooldownRemaining { get; private set; }
        public int CurrentEnergy { get; private set; }
        public int MaxEnergy { get; private set; }
        public int RecoveryEnergyDuration { get; private set; }
        public DateTimeOffset EnergyUpdateTime { get; private set; }
        public IReadOnlyList<(int itemId, int count)> LastDrops => _lastDrops;

        public event Action OnInfoUpdated;
        public event Action OnHitResult;
        public event Action OnCooldown;

        public LumberService(INetworkManager networkManager)
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
                case FromServerMessage.Cooldown:
                    SetCooldown(message);
                    break;
            }
        }

        public void GetInfo(int treeId)
        {
            var message = new Message(MessageType.Lumber)
                .AddByte(FromClientMessage.GetInfo.ToByte())
                .AddInt(treeId);

            _networkManager.SendMessage(message);
        }

        public void Hit(int treeId)
        {
            var message = new Message(MessageType.Lumber)
                .AddByte(FromClientMessage.Hit.ToByte())
                .AddInt(treeId);

            _networkManager.SendMessage(message);
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
            CurrentHealth = message.GetInt();
            CooldownRemaining = message.GetFloat();
            CurrentEnergy = message.GetInt();
            MaxEnergy = message.GetInt();
            RecoveryEnergyDuration = message.GetInt();
            EnergyUpdateTime = DateTimeOffset.FromUnixTimeMilliseconds(message.GetLong()).UtcDateTime;

            if (CooldownRemaining == 0 && CurrentHealth > MaxHealth)
                MaxHealth = CurrentHealth;

            OnInfoUpdated?.Invoke();
        }

        private void SetHitResult(Message message)
        {
            CurrentHealth = message.GetInt();
            CurrentEnergy = message.GetInt();
            EnergyUpdateTime = DateTimeOffset.FromUnixTimeMilliseconds(message.GetLong()).UtcDateTime;

            var treeDestroyed = message.GetBool();

            _lastDrops.Clear();
            if (treeDestroyed)
            {
                var dropsCount = message.GetByte();
                for (int i = 0; i < dropsCount; i++)
                {
                    var itemId = message.GetInt();
                    var count = message.GetInt();
                    _lastDrops.Add((itemId, count));
                }
            }

            OnHitResult?.Invoke();
        }

        private void SetCooldown(Message message)
        {
            CooldownRemaining = message.GetFloat();
            OnCooldown?.Invoke();
        }
    }
}

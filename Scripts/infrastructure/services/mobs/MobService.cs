using System.Collections.Generic;
using character;
using factories.mobs;
using network;
using UnityEngine;

namespace infrastructure.services.mobs
{
    public class MobService : IMobService
    {
        private enum FromServerMessage : byte
        {
            Create,
            Remove,
            Positions,
            ChangedHealth,
            Death,
            Hit,
        }
        
        private enum FromClientMessages : byte
        {

        }
        
        private readonly Dictionary<ushort, Mob> _mobs = new Dictionary<ushort, Mob>();

        public IReadOnlyDictionary<ushort, Mob> Mobs => _mobs;

        private readonly IMobFactory _mobFactory;
        
        public MobService(IMobFactory mobFactory)
        {
            _mobFactory = mobFactory;
        }
        
        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            switch (type)
            {
                case FromServerMessage.Create:
                    CreateMob(message);
                    break;
                case FromServerMessage.Remove:
                    RemoveMob(message);
                    break;
                case FromServerMessage.Positions:
                    SetPositions(message);
                    break;
                case FromServerMessage.ChangedHealth:
                    ChangeHealth(message);
                    break;
                case FromServerMessage.Death:
                    Death(message);
                    break;
                case FromServerMessage.Hit:
                    MobHit(message);
                    break;
            }
        }
        
        private void CreateMob(Message message)
        {
            var id = message.GetUShort();
            var type = (MobType)message.GetUShort();
            var position = message.GetVector3();
            var rotation = message.GetFloat();
            var maxHealth = message.GetInt();
            var currentHealth = message.GetInt();
            var level = message.GetInt();
            
            var mob = _mobFactory.GetNewMob(type);
            _mobs.TryAdd(id, mob);
            mob.Initialize(id, type);
            mob.SetPosition(new EnemySnapshot(NetworkManager.CurrentTick, position, rotation));
            mob.SetBaseValues(maxHealth, currentHealth, level);
        }

        private void RemoveMob(Message message)
        {
            var id = message.GetUShort();
            if (!_mobs.TryGetValue(id, out var mob)) return;
            mob.Release();
            _mobs.Remove(id);
        }
        
        private void SetPositions(Message message)
        {
            var tick = message.GetInt();
            NetworkManager.UpdateTick(tick);
            
            var count = message.GetUShort();
            
            for (int i = 0; i < count; i++)
            {
                var id = message.GetUShort();
                var position = message.GetVector3();
                var rotation = message.GetFloat();

                if (!_mobs.TryGetValue(id, out var mob)) continue;
                mob.SetPosition(new EnemySnapshot(tick, position, rotation));
            }
        }

        private void ChangeHealth(Message message)
        {
            var id = message.GetUShort();
            if (!_mobs.TryGetValue(id, out var mob)) return;
            mob.SetCurrentHealth(message.GetInt());
        }

        private void Death(Message message)
        {
            var id = message.GetUShort();
            if (!_mobs.TryGetValue(id, out var mob)) return;
            mob.Death().Forget();
            _mobs.Remove(id);
        }

        private void MobHit(Message message)
        {
            var id = message.GetUShort();
            if (!_mobs.TryGetValue(id, out var mob)) return;
            mob.Attack();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using infrastructure.factories;
using infrastructure.services.mobs;
using UnityEngine;
using Zenject;

namespace factories.mobs
{
    public class MobFactory : IMobFactory
    {
        private readonly Dictionary<MobType, MobData> _mobsData;
        private readonly Dictionary<MobModelType, ObjectPool> _mobsModelsPools;

        public MobFactory(DiContainer container)
        {
            var parent = new GameObject("Mobs").transform;
            
            _mobsData = Resources.LoadAll<MobData>("Data/Mobs").ToDictionary(d => d.MobType, d => d);
            _mobsModelsPools = Resources.LoadAll<MobModel>("Prefabs/Mobs/Models")
                .ToDictionary(m => m.MobModelType, m => new ObjectPool(m, 0, parent, container));
        }

        public Mob GetNewMob(MobType mobType)
        {
            var mob = Pool.Get<Mob>();
            var mobData = _mobsData[mobType];
            var mobModel = _mobsModelsPools[mobData.MobModelType].GetObject<MobModel>();
            mobModel.Initialize(mobData);
            mob.SetModel(mobModel);
            return mob;
        }
    }
}
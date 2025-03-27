using System.Collections.Generic;
using System.Linq;
using environment.chests;
using infrastructure.services.npc;
using UnityEngine;
using Zenject;

namespace infrastructure.factories.environment
{
    public class EnvironmentFactory : IEnvironmentFactory
    {
        private readonly ObjectPool _chestsFromMobPool;
        private readonly ObjectPool _npcPool;

        private readonly ChestFromMob _chestFromMobPrefab;

        private readonly Dictionary<NpcType, NpcData> _npcDatas;
        
        public EnvironmentFactory(DiContainer container)
        {
            var parent = new GameObject("Environment").transform;

            _npcDatas = Resources.LoadAll<NpcData>("Data/Npc/").ToDictionary(d => d.NpcType, d => d);
            
            _chestFromMobPrefab = Resources.Load<ChestFromMob>("Prefabs/Environment/Chests/ChestFromMob");
            _chestsFromMobPool = new ObjectPool(_chestFromMobPrefab, 0, parent, container);
            _npcPool = new ObjectPool(Resources.Load<Npc>("Prefabs/Environment/NPC"), 0, parent, container);
        }

        public ChestFromMob CreateChestFromMob()
        {
            return _chestsFromMobPool.GetObject<ChestFromMob>();
        }

        public Npc CreateNpc(NpcType npcType)
        {
            var npc = _npcPool.GetObject<Npc>();
            npc.SetData(_npcDatas[npcType]);
            return npc;
        }
    }
}
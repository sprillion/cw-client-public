using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.factories;
using infrastructure.factories.environment;
using infrastructure.services.bundles;
using network;
using Newtonsoft.Json;
using tools;
using UnityEngine;

namespace infrastructure.services.npc
{
    public class NpcService : INpcService
    {
        public enum FromClientMessage : byte
        {
            Attitude,
            BuyItem,
        }

        public enum FromServerMessage : byte
        {
            Create,
            Remove,
            Attitude,
        }

        private Dictionary<NpcType, NpcData> _npcDatas;
        private readonly Dictionary<NpcType, Npc> _npcs = new Dictionary<NpcType, Npc>();
        private readonly Dictionary<NpcType, NpcDataJson> _npcsShopData = new Dictionary<NpcType, NpcDataJson>();
        
        private readonly IEnvironmentFactory _environmentFactory;
        private readonly INetworkManager _networkManager;
        private readonly IBundleService _bundleService;


        public IReadOnlyDictionary<NpcType, Npc> Npcs => _npcs;
        public NpcData CurrentNpcData { get; set; }
        public int CurrentAttitudeLevel { get; private set; }
        public int CurrentAttitudeProgress { get; private set; }
        public int CurrentMoneySpent { get; private set; }

        public event Action OnNpcLoaded;

        public event Action OnAttitudeLoaded;
        
        public NpcService(INetworkManager networkManager, IBundleService bundleService)
        {
            _networkManager = networkManager;
            _bundleService = bundleService;
            // _npcDatas = Resources.LoadAll<NpcData>("Data/Npc").ToDictionary(d => d.NpcType, d => d);
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.Create:
                    CreateNpc(message);
                    break;
                case FromServerMessage.Remove:
                    RemoveNpc(message);
                    break;
                case FromServerMessage.Attitude:
                    SetAttitudeNpcInfo(message);
                    break;
            }
        }

        public void LoadAllNpc()
        {
            LoadAllNpcTask().Forget();
        }

        public void GetAttitudeNpcInfo()
        {
            var message = new Message(MessageType.Npc)
                .AddByte(FromClientMessage.Attitude.ToByte())
                .AddByte(CurrentNpcData.NpcType.ToByte());

            _networkManager.SendMessage(message);
        }

        public List<ItemToPurchasing> GetShopItems()
        {
            return _npcsShopData[CurrentNpcData.NpcType].ItemsToPurchasing;
        }
        
        public void GetBarterInfo(NpcType npcType)
        {
            
        }
        
        public void GetQuestsInfo(NpcType npcType)
        {
            
        }

        public Sprite GetNpcAvatarIcon(NpcType npcType)
        {
            return _npcDatas.TryGetValue(npcType, out var data) ? data.AvatarIcon : null;
        }

        public void BuyItem(int itemId)
        {
            var message = new Message(MessageType.Npc)
                .AddByte(FromClientMessage.BuyItem.ToByte())
                .AddByte(CurrentNpcData.NpcType.ToByte())
                .AddInt(itemId);
            
            _networkManager.SendMessage(message);
        }

        private async UniTaskVoid LoadAllNpcTask()
        {
            Debug.Log("[NpcService] Loading NPC data...");
            await LoadNpcData();
            Debug.Log("[NpcService] Loading NPC shop data...");
            await LoadNpcShopData();
            Debug.Log("[NpcService] All NPC data loaded.");
            OnNpcLoaded?.Invoke();
        }

        private async UniTask LoadNpcShopData()
        {
            var data = await _bundleService.LoadAssetsByLabel<TextAsset>("Npc");
            Debug.Log($"[NpcService] NpcShopData assets loaded: {data.Count}");
            foreach (var textAsset in data)
            {
                var npcDataJson = JsonConvert.DeserializeObject<NpcDataJson>(textAsset.text);
                _npcsShopData.Add(npcDataJson.NpcType, npcDataJson);
            }
        }

        private async UniTask LoadNpcData()
        {
            var data = await _bundleService.LoadAssetsByLabel<NpcData>("Npc");
            Debug.Log($"[NpcService] NpcData assets loaded: {data.Count}");
            if (data.Count > 0)
                _npcDatas = data.ToDictionary(d => d.NpcType, d => d);
        }

        private void CreateNpc(Message message)
        {
            var npcType = (NpcType)message.GetByte();
            var position = message.GetVector3();
            var rotation = message.GetFloat();

            var npc = Pool.Get<Npc>();
            npc.SetData(_npcDatas[npcType]);
            npc.transform.position = position;
            npc.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));

            if (!_npcs.TryAdd(npcType, npc))
            {
                npc.Release();
            }
        }

        private void RemoveNpc(Message message)
        {
            var type = (NpcType)message.GetByte();
            _npcs[type]?.Release();
            _npcs.Remove(type);
        }

        private void SetAttitudeNpcInfo(Message message)
        {
            CurrentAttitudeLevel = message.GetInt();
            CurrentAttitudeProgress = message.GetInt();
            CurrentMoneySpent = message.GetInt();
            
            OnAttitudeLoaded?.Invoke();
        }
    }
}
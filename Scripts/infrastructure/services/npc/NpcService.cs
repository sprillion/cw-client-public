using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.factories.environment;
using network;
using Newtonsoft.Json;
using secrets;
using tools;
using UnityEngine;
using UnityEngine.Networking;

namespace infrastructure.services.npc
{
    public class NpcService : INpcService
    {
        public enum FromClientMessage : byte
        {
            Attitude,
        }

        public enum FromServerMessage : byte
        {
            Create,
            Remove,
            Attitude,
        }

        private readonly Dictionary<NpcType, Npc> _npcs = new Dictionary<NpcType, Npc>();
        private readonly Dictionary<NpcType, NpcDataJson> _npcsData = new Dictionary<NpcType, NpcDataJson>();
        
        private readonly IEnvironmentFactory _environmentFactory;
        private readonly INetworkManager _networkManager;


        public NpcData CurrentNpcData { get; set; }
        public int CurrentAttitudeLevel { get; private set; }
        public int CurrentAttitudeProgress { get; private set; }
        public int CurrentMoneySpent { get; private set; }

        public event Action OnNpcLoaded;

        public event Action OnAttitudeLoaded;
        

        public NpcService(INetworkManager networkManager, IEnvironmentFactory environmentFactory)
        {
            _networkManager = networkManager;
            _environmentFactory = environmentFactory;
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
            var message = new Message(ClientToServerId.Npc);
            message.AddByte(FromClientMessage.Attitude.ToByte());
            message.AddByte(CurrentNpcData.NpcType.ToByte());

            _networkManager.SendMessage(message);
        }

        public List<ItemToPurchasing> GetShopItems()
        {
            return _npcsData[CurrentNpcData.NpcType].ItemsToPurchasing;
        }
        
        public void GetBarterInfo(NpcType npcType)
        {
            
        }
        
        public void GetQuestsInfo(NpcType npcType)
        {
            
        }

        private async UniTaskVoid LoadAllNpcTask()
        {
            foreach (NpcType value in Enum.GetValues(typeof(NpcType)))
            {
                if (value != NpcType.Alchemist) continue;
                await LoadNpcData(value);
            }
            
            OnNpcLoaded?.Invoke();
        }

        private async UniTask LoadNpcData(NpcType npcType)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{SecretKey.NpcDataUrl}/{npcType}.json"))
            {
#if UNITY_WEBGL
                webRequest.SetRequestHeader("Cache-Control", "max-age=3600, must-revalidate");
#endif
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error downloading JSON: " + webRequest.error);
                }
                else
                {
                    var npcDataJson = JsonConvert.DeserializeObject<NpcDataJson>(webRequest.downloadHandler.text);
                    _npcsData.Add(npcDataJson.NpcType, npcDataJson);
                }
            }
        }

        private void CreateNpc(Message message)
        {
            var type = (NpcType)message.GetByte();
            var position = message.GetVector3();
            var rotation = message.GetFloat();

            var npc = _environmentFactory.CreateNpc(type);
            npc.transform.position = position;
            npc.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));

            if (!_npcs.TryAdd(type, npc))
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
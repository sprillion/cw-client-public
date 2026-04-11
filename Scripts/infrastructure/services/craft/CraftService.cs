using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.services.bundles;
using network;
using Newtonsoft.Json;
using tools;
using UnityEngine;

namespace infrastructure.services.craft
{
    public class CraftService : ICraftService
    {
        public enum FromClientMessage : byte
        {
            GetAvailableCrafts,
            GetCurrentCrafts,
            StartCraft,
            GetCraftResult,
        }

        public enum FromServerMessage : byte
        {
            AvailableCrafts,
            CurrentCrafts,
            StartCraft,
            TakeResult,
            AddCraft,
        }

        private readonly IBundleService _bundleService;
        private readonly INetworkManager _networkManager;

        private Dictionary<int, CraftDataJson> _craftData;
        private readonly List<int> _availableCrafts = new List<int>(256);
        private readonly Dictionary<int, DateTime> _currentCrafts = new Dictionary<int, DateTime>(64);

        public event Action OnCraftsUpdated;
        
        public CraftService(IBundleService bundleService, INetworkManager networkManager)
        {
            _bundleService = bundleService;
            _networkManager = networkManager;
        }
        
        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.AvailableCrafts:
                    SetAvailableCrafts(message);
                    break;
                case FromServerMessage.CurrentCrafts:
                    SetCurrentCrafts(message);
                    break;
                case FromServerMessage.StartCraft:
                    SetStartCraft(message);
                    break;
                case FromServerMessage.TakeResult:
                    SetTakeResult(message);
                    break;
                case FromServerMessage.AddCraft:
                    AddAvailableCraft(message);
                    break;
            }
        }

        public void GetAvailableCrafts()
        {
            var message = new Message(MessageType.Crafts)
                .AddByte(FromClientMessage.GetAvailableCrafts.ToByte());
            
            _networkManager.SendMessage(message);
        }

        public void GetCurrentCrafts()
        {
            var message = new Message(MessageType.Crafts)
                .AddByte(FromClientMessage.GetCurrentCrafts.ToByte());
            
            _networkManager.SendMessage(message);
        }

        public void StartCraft(int craftId)
        {
            var message = new Message(MessageType.Crafts)
                .AddByte(FromClientMessage.StartCraft.ToByte())
                .AddInt(craftId);
            
            _networkManager.SendMessage(message);
        }

        public void TakeCraftResult(int craftId)
        {
            var message = new Message(MessageType.Crafts)
                .AddByte(FromClientMessage.GetCraftResult.ToByte())
                .AddInt(craftId);
            
            _networkManager.SendMessage(message);
        }

        public List<CraftDataJson> GetCraftsFromPlace(CraftPlaceType craftPlaceType)
        {
            var result = new List<CraftDataJson>();

            foreach (var availableCraft in _availableCrafts)
            {
                if (_craftData[availableCraft].CraftPlaceType == craftPlaceType)
                {
                    result.Add(_craftData[availableCraft]);
                }
            }

            return result;
        }

        public bool IsCurrentCraft(int craftId)
        {
            return _currentCrafts.TryGetValue(craftId, out var _);
        }
        
        public bool IsCompleteCraft(int craftId)
        {
            if (!_currentCrafts.TryGetValue(craftId, out var finishTime))
            {
                return false;
            }

            return finishTime <= NetworkManager.ServerNow;
        }
        
        public DateTime GetFinishTimeCraft(int craftId)
        {
            if (!_currentCrafts.TryGetValue(craftId, out var finishTime))
            {
                return new DateTime();
            }

            return finishTime;
        }

        public void LoadCraftData()
        {
            LoadCraftsDataAsync().Forget();
        }
        
        private async UniTask LoadCraftsDataAsync()
        {
            GetAvailableCrafts();
            GetCurrentCrafts();

            var data = await _bundleService.LoadAssetByName<TextAsset>("Crafts");

            _craftData = JsonConvert.DeserializeObject<List<CraftDataJson>>(data.text).ToDictionary(d => d.Id, d => d);
        }

        private void SetAvailableCrafts(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                var craftId = message.GetInt();
                
                AddAvailableCraft(craftId);
            }
        }

        private void SetCurrentCrafts(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                var craftId = message.GetInt();
                var finishTime = DateTimeOffset.FromUnixTimeMilliseconds(message.GetLong()).UtcDateTime;
                
                AddCurrentCraft(craftId, finishTime);
            }
        }

        private void AddCurrentCraft(int craftId, DateTime finishTime)
        {
            _currentCrafts.Add(craftId, finishTime);
        }
        
        private void AddAvailableCraft(int craftId)
        {
            _availableCrafts.Add(craftId);
        }

        private void SetStartCraft(Message message)
        {
            var craftId = message.GetInt();
            var success = message.GetBool();

            if (success)
            {
                var finishTime = DateTimeOffset.FromUnixTimeMilliseconds(message.GetLong()).UtcDateTime;
                AddCurrentCraft(craftId, finishTime);
                OnCraftsUpdated?.Invoke();
            }
        }

        private void SetTakeResult(Message message)
        {
            var craftId = message.GetInt();
            var success = message.GetBool();

            if (success)
            {
                _currentCrafts.Remove(craftId);
                OnCraftsUpdated?.Invoke();
            }
        }

        private void AddAvailableCraft(Message message)
        {
            var craftId = message.GetInt();
            
            AddAvailableCraft(craftId);
            OnCraftsUpdated?.Invoke();
        }
    }
}
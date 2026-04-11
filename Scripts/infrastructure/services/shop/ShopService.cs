using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.services.bundles;
using infrastructure.services.npc;
using infrastructure.services.players;
using infrastructure.services.shop.capes;
using infrastructure.services.shop.skins;
using network;
using Newtonsoft.Json;
using tools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace infrastructure.services.shop
{
    public class ShopService : IShopService
    {
        public enum FromClientMessage : byte
        {
            GetSkins,
            BuySkin,
            PutOnSkin,
            GetCapes,
            BuyCape,
            PutOnCape,
        }

        public enum FromServerMessage : byte
        {
            Skins,
            AddSkin,
            EquippedSkin,
            Capes,
            AddCape,
            EquippedCape,
        }

        private readonly INetworkManager _networkManager;
        private readonly IBundleService _bundleService;
        private readonly ICharacterService _characterService;

        public List<int> AvailableSkins { get; } = new List<int>();
        public Dictionary<int, SkinData> Skins { get; private set; }

        public List<int> AvailableCapes { get; } = new List<int>();
        public Dictionary<int, CapeData> Capes { get; private set; }

        private readonly Dictionary<int, character.SkinData> _skinDatas = new();
        private readonly Dictionary<int, AsyncOperationHandle<character.SkinData>> _skinHandles = new();

        private readonly Dictionary<int, character.CapeData> _capeDatas = new();
        private readonly Dictionary<int, AsyncOperationHandle<character.CapeData>> _capeHandles = new();

        public event Action OnDataLoaded;
        public event Action OnAvailableSkinsLoaded;
        public event Action<int> OnAvailableSkinAdded;
        public event Action<CurrencyType> OnNotEnoughCurrency;
        public event Action<int> OnSkinEquiped;
        public event Action OnAvailableCapesLoaded;
        public event Action<int> OnAvailableCapeAdded;
        public event Action<int> OnCapeEquiped;

        public ShopService(INetworkManager networkManager, IBundleService bundleService, ICharacterService characterService)
        {
            _networkManager = networkManager;
            _bundleService = bundleService;
            _characterService = characterService;
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.Skins:
                    SetSkins(message);
                    break;
                case FromServerMessage.AddSkin:
                    AddSkin(message);
                    break;
                case FromServerMessage.EquippedSkin:
                    SetEquipedSkin(message);
                    break;
                case FromServerMessage.Capes:
                    SetCapes(message);
                    break;
                case FromServerMessage.AddCape:
                    AddCape(message);
                    break;
                case FromServerMessage.EquippedCape:
                    SetEquipedCape(message);
                    break;
            }
        }

        public void LoadData()
        {
            LoadDataAsync().Forget();
        }

        private async UniTaskVoid LoadDataAsync()
        {
            character.CharacterSkin.SkinLoader = GetSkinDataAsync;
            character.CharacterSkin.CapeLoader = GetCapeDataAsync;
            await LoadSkins();
            await LoadCapes();
            OnDataLoaded?.Invoke();
        }

        public async UniTask<character.SkinData> GetSkinDataAsync(int skinId)
        {
            if (_skinDatas.TryGetValue(skinId, out var cached)) return cached;

            if (!_skinHandles.TryGetValue(skinId, out var handle))
            {
                handle = Addressables.LoadAssetAsync<character.SkinData>($"SkinData/{skinId}");
                _skinHandles[skinId] = handle;
            }

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                _skinDatas[skinId] = handle.Result;
            else
                Debug.LogError($"[ShopService] Failed to load SkinData/{skinId}");

            return handle.Result;
        }

        public async UniTask<character.CapeData> GetCapeDataAsync(int capeId)
        {
            if (_capeDatas.TryGetValue(capeId, out var cached)) return cached;

            if (!_capeHandles.TryGetValue(capeId, out var handle))
            {
                handle = Addressables.LoadAssetAsync<character.CapeData>($"CapeData/{capeId}");
                _capeHandles[capeId] = handle;
            }

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                _capeDatas[capeId] = handle.Result;
            else
                Debug.LogError($"[ShopService] Failed to load CapeData/{capeId}");

            return handle.Result;
        }

        public void GetSkins()
        {
            var message = new Message(MessageType.Shop)
                .AddByte(FromClientMessage.GetSkins.ToByte());

            _networkManager.SendMessage(message);
        }

        public bool BuySkin(int skinId)
        {
            var data = Skins[skinId];
            if (!_characterService.CurrentCharacter.CharacterStats.HaveCurrency(data.CurrencyType, data.Price))
            {
                OnNotEnoughCurrency?.Invoke(data.CurrencyType);
                return false;
            }

            var message = new Message(MessageType.Shop)
                .AddByte(FromClientMessage.BuySkin.ToByte())
                .AddInt(skinId);

            _networkManager.SendMessage(message);
            return true;
        }

        public bool PutOnSkin(int skinId)
        {
            if (!AvailableSkins.Contains(skinId)) return false;

            var message = new Message(MessageType.Shop)
                .AddByte(FromClientMessage.PutOnSkin.ToByte())
                .AddInt(skinId);

            _networkManager.SendMessage(message);
            return true;
        }

        public void GetCapes()
        {
            var message = new Message(MessageType.Shop)
                .AddByte(FromClientMessage.GetCapes.ToByte());

            _networkManager.SendMessage(message);
        }

        public bool BuyCape(int capeId)
        {
            var data = Capes[capeId];
            if (!_characterService.CurrentCharacter.CharacterStats.HaveCurrency(data.CurrencyType, data.Price))
            {
                OnNotEnoughCurrency?.Invoke(data.CurrencyType);
                return false;
            }

            var message = new Message(MessageType.Shop)
                .AddByte(FromClientMessage.BuyCape.ToByte())
                .AddInt(capeId);

            _networkManager.SendMessage(message);
            return true;
        }

        public bool PutOnCape(int capeId)
        {
            if (capeId != -1 && !AvailableCapes.Contains(capeId)) return false;

            var message = new Message(MessageType.Shop)
                .AddByte(FromClientMessage.PutOnCape.ToByte())
                .AddInt(capeId);

            _networkManager.SendMessage(message);
            return true;
        }

        private void SetSkins(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                AvailableSkins.Add(message.GetInt());
            }

            OnAvailableSkinsLoaded?.Invoke();
        }

        private void AddSkin(Message message)
        {
            var skinId = message.GetInt();
            var success = message.GetBool();

            if (success)
            {
                AvailableSkins.Add(skinId);
                OnAvailableSkinAdded?.Invoke(skinId);
            }
        }

        private void SetEquipedSkin(Message message)
        {
            var skinId = message.GetInt();
            var success = message.GetBool();

            if (success)
            {
                OnSkinEquiped?.Invoke(skinId);
            }
        }

        private void SetCapes(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                AvailableCapes.Add(message.GetInt());
            }

            OnAvailableCapesLoaded?.Invoke();
        }

        private void AddCape(Message message)
        {
            var capeId = message.GetInt();
            var success = message.GetBool();

            if (success)
            {
                AvailableCapes.Add(capeId);
                OnAvailableCapeAdded?.Invoke(capeId);
            }
        }

        private void SetEquipedCape(Message message)
        {
            var capeId = message.GetInt();
            var success = message.GetBool();

            if (success)
            {
                OnCapeEquiped?.Invoke(capeId);
            }
        }

        private async UniTask LoadSkins()
        {
            var json = await _bundleService.LoadAssetByName<TextAsset>("SkinsConfig");
            Skins = JsonConvert.DeserializeObject<List<SkinData>>(json.text).ToDictionary(d => d.Id, d => d);
        }

        private async UniTask LoadCapes()
        {
            var json = await _bundleService.LoadAssetByName<TextAsset>("CapesConfig");
            Capes = JsonConvert.DeserializeObject<List<CapeData>>(json.text).ToDictionary(d => d.Id, d => d);
        }
    }
}

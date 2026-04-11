using System;
using System.Collections.Generic;
using System.Linq;
using character;
using Cysharp.Threading.Tasks;
using infrastructure.factories;
using infrastructure.services.bundles;
using infrastructure.services.input;
using infrastructure.services.players;
using network;
using Newtonsoft.Json;
using tools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace infrastructure.services.transport
{
    public class TransportService : ITransportService
    {
        private enum FromServerMessage : byte
        {
            Transports         = 0,
            AddTransport       = 1,
            SpawnedTransport   = 2,
            DespawnedTransport = 3,
            Positions          = 4,
            Mounted            = 5,
            Dismounted         = 6,
        }

        private enum FromClientMessage : byte
        {
            GetTransports     = 0,
            BuyTransport      = 1,
            SpawnTransport    = 2,
            DespawnTransport  = 3,
            MountTransport    = 4,
            DismountTransport = 5,
        }

        private readonly INetworkManager _networkManager;
        private readonly ICharacterService _characterService;
        private readonly IInputService _inputService;
        private readonly IBundleService _bundleService;

        private bool _isLocalMounted;

        public bool IsLocalMounted => _isLocalMounted;

        private Transport _localTransport;
        private readonly Dictionary<ushort, EnemyTransport> _enemyTransports = new();
        private readonly Dictionary<int, ushort> _characterToTransport = new();
        private readonly HashSet<int> _ownedTransportIds = new();

        private Dictionary<int, TransportData> _transportDatas = new();
        private readonly Dictionary<int, TransportAssetData> _assetDatas = new();
        private readonly Dictionary<int, AsyncOperationHandle<TransportAssetData>> _assetHandles = new();

        // TransportModelType → pool of TransportModel prefabs
        private readonly Dictionary<TransportModelType, ObjectPool> _modelPools;

        public IReadOnlyCollection<int> OwnedTransportIds => _ownedTransportIds;
        public IReadOnlyDictionary<int, TransportData> TransportDatas => _transportDatas;
        public int LocalSpawnedTransportTypeId => _localSpawnedTransportTypeId;

        private int _localSpawnedTransportTypeId;
        private ushort _localSpawnedInstanceId;

        public event Action OnOwnedTransportsChanged;
        public event Action<CurrencyType> OnNotEnoughCurrency;
        public event Action<int> OnLocalTransportSpawned;
        public event Action OnLocalTransportDespawned;
        public event Action OnLocalMounted;
        public event Action OnLocalDismounted;

        public TransportService(INetworkManager networkManager, ICharacterService characterService,
            IInputService inputService, IBundleService bundleService, DiContainer container)
        {
            _networkManager = networkManager;
            _characterService = characterService;
            _inputService = inputService;
            _bundleService = bundleService;

            var modelsParent = new GameObject("TransportModels").transform;

            _modelPools = Resources.LoadAll<TransportModel>("Prefabs/Transport/Models")
                .ToDictionary(m => m.ModelType, m => new ObjectPool(m, 0, modelsParent, container));

            _characterService.OnCharacterMounted += OnCharacterMountedTick;
        }

        // --- ITransportService ---

        public void LoadData() => LoadDataAsync().Forget();

        private async UniTaskVoid LoadDataAsync()
        {
            var json = await _bundleService.LoadAssetByName<TextAsset>("TransportsConfig");
            var list = JsonConvert.DeserializeObject<List<TransportData>>(json.text);
            _transportDatas = list.ToDictionary(d => d.Id, d => d);
        }

        public async UniTask<TransportAssetData> GetTransportAssetDataAsync(int transportTypeId)
        {
            if (_assetDatas.TryGetValue(transportTypeId, out var cached)) return cached;

            if (!_assetHandles.TryGetValue(transportTypeId, out var handle))
            {
                handle = Addressables.LoadAssetAsync<TransportAssetData>($"TransportData/{transportTypeId}");
                _assetHandles[transportTypeId] = handle;
            }

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                _assetDatas[transportTypeId] = handle.Result;
            else
                Debug.LogError($"[TransportService] Failed to load TransportData/{transportTypeId}");

            return handle.Result;
        }

        // --- IReceiver ---

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            switch (type)
            {
                case FromServerMessage.Transports:         HandleTransports(message);         break;
                case FromServerMessage.AddTransport:       HandleAddTransport(message);       break;
                case FromServerMessage.SpawnedTransport:   HandleSpawnedTransport(message);   break;
                case FromServerMessage.DespawnedTransport: HandleDespawnedTransport(message); break;
                case FromServerMessage.Positions:          HandlePositions(message);          break;
                case FromServerMessage.Mounted:            HandleMounted(message);            break;
                case FromServerMessage.Dismounted:         HandleDismounted(message);         break;
            }
        }

        // --- Client → Server ---

        public void GetTransports()
        {
            var message = new Message(MessageType.Transport)
                .AddByte(FromClientMessage.GetTransports.ToByte());
            _networkManager.SendMessage(message);
        }

        public bool BuyTransport(int transportTypeId)
        {
            if (!_transportDatas.TryGetValue(transportTypeId, out var data)) return false;
            if (!_characterService.CurrentCharacter.CharacterStats.HaveCurrency(data.CurrencyType, data.Price))
            {
                OnNotEnoughCurrency?.Invoke(data.CurrencyType);
                return false;
            }
            var message = new Message(MessageType.Transport)
                .AddByte(FromClientMessage.BuyTransport.ToByte())
                .AddInt(transportTypeId);
            _networkManager.SendMessage(message);
            return true;
        }

        public void SpawnTransport(int transportTypeId)
        {
            var message = new Message(MessageType.Transport)
                .AddByte(FromClientMessage.SpawnTransport.ToByte())
                .AddInt(transportTypeId);
            _networkManager.SendMessage(message);
        }

        public void DespawnTransport()
        {
            var message = new Message(MessageType.Transport)
                .AddByte(FromClientMessage.DespawnTransport.ToByte());
            _networkManager.SendMessage(message);
        }

        public void MountTransport()
        {
            var message = new Message(MessageType.Transport)
                .AddByte(FromClientMessage.MountTransport.ToByte());
            _networkManager.SendMessage(message);
        }

        public void DismountTransport()
        {
            var message = new Message(MessageType.Transport)
                .AddByte(FromClientMessage.DismountTransport.ToByte());
            _networkManager.SendMessage(message);
        }

        public void SetAutopilotDirection(Vector3 direction) => _localTransport?.StartAutopilot(direction);
        public void StopAutopilot()                          => _localTransport?.StopAutopilot();

        // --- Server → Client handlers ---

        private void HandleTransports(Message message)
        {
            _ownedTransportIds.Clear();
            var count = message.GetInt();
            for (int i = 0; i < count; i++)
                _ownedTransportIds.Add(message.GetInt());
            OnOwnedTransportsChanged?.Invoke();
        }

        private void HandleAddTransport(Message message)
        {
            var transportTypeId = message.GetInt();
            var success = message.GetBool();
            if (!success) return;
            _ownedTransportIds.Add(transportTypeId);
            OnOwnedTransportsChanged?.Invoke();
        }

        private void HandleSpawnedTransport(Message message)
        {
            var instanceId      = message.GetUShort();
            var ownerCharId     = message.GetInt();
            var transportTypeId = message.GetInt();
            var position        = message.GetVector3();
            var rotation        = message.GetFloat();
            SpawnTransportAsync(instanceId, ownerCharId, transportTypeId, position, rotation).Forget();
        }

        private async UniTaskVoid SpawnTransportAsync(ushort instanceId, int ownerCharId, int transportTypeId,
            Vector3 position, float rotation)
        {
            if (!_transportDatas.TryGetValue(transportTypeId, out var configData)) return;
            var assetData = await GetTransportAssetDataAsync(transportTypeId);
            if (assetData == null) return;
            if (!_modelPools.TryGetValue(assetData.ModelType, out var modelPool)) return;

            if (ownerCharId == _characterService.CurrentCharacter?.Id)
            {
                var transport = Pool.Get<Transport>();
                var model = modelPool.GetObject<TransportModel>();
                model.Initialize(assetData);
                transport.Initialize();
                transport.SetModel(model);
                transport.SetSpeed(configData.MoveSpeed);
                transport.SetPosition(position);
                _localTransport = transport;
                _localSpawnedTransportTypeId = transportTypeId;
                _localSpawnedInstanceId = instanceId;
                transport.OnInteractRequested = MountTransport;
                transport.EnableInteract();
                OnLocalTransportSpawned?.Invoke(transportTypeId);
            }
            else
            {
                var transport = Pool.Get<EnemyTransport>();
                var model = modelPool.GetObject<TransportModel>();
                model.Initialize(assetData);
                transport.Initialize();
                transport.SetModel(model);
                transport.ApplySnapshot(new EnemySnapshot(NetworkManager.CurrentTick, position, rotation));
                _enemyTransports.TryAdd(instanceId, transport);
            }
        }

        private void HandleDespawnedTransport(Message message)
        {
            var instanceId = message.GetUShort();

            if (instanceId == _localSpawnedInstanceId)
            {
                if (_isLocalMounted)
                {
                    var currentChar = _characterService.CurrentCharacter;
                    if (currentChar != null)
                        _localTransport.DismountLocal(currentChar, _characterService);
                    _isLocalMounted = false;
                    _inputService.OnInteractEvent -= HandleDismountInput;
                    _characterToTransport.Remove(currentChar?.Id ?? -1);
                    OnLocalDismounted?.Invoke();
                }
                _localSpawnedTransportTypeId = 0;
                _localSpawnedInstanceId = 0;
                _localTransport?.Release();
                _localTransport = null;
                OnLocalTransportDespawned?.Invoke();
                return;
            }

            if (!_enemyTransports.TryGetValue(instanceId, out var transport)) return;

            foreach (var kvp in _characterToTransport)
            {
                if (kvp.Value != instanceId) continue;
                transport.Dismount();
                if (_characterService.OtherCharacters.TryGetValue(kvp.Key, out var enemy))
                    enemy.SetVisible(true);
                _characterToTransport.Remove(kvp.Key);
                break;
            }

            transport.Release();
            _enemyTransports.Remove(instanceId);
        }

        private void HandlePositions(Message message)
        {
            var tick       = message.GetInt();
            var instanceId = message.GetUShort();
            var position   = message.GetVector3();
            var rotation   = message.GetFloat();

            NetworkManager.UpdateTick(tick);

            if (_enemyTransports.TryGetValue(instanceId, out var transport))
                transport.ApplySnapshot(new EnemySnapshot(tick, position, rotation));
        }

        private void HandleMounted(Message message)
        {
            var characterId = message.GetInt();
            var instanceId  = message.GetUShort();

            _characterToTransport[characterId] = instanceId;

            var currentChar = _characterService.CurrentCharacter;
            if (currentChar != null && currentChar.Id == characterId)
            {
                _localTransport.MountLocal(currentChar, _characterService);
                _localTransport.OnInteractRequested = DismountTransport;
                _isLocalMounted = true;
                _inputService.OnInteractEvent += HandleDismountInput;
                OnLocalMounted?.Invoke();
            }
            else if (_enemyTransports.TryGetValue(instanceId, out var enemyTransport) &&
                     _characterService.OtherCharacters.TryGetValue(characterId, out var enemy))
            {
                enemyTransport.Mount(enemy.CharacterSkin);
                enemy.SetVisible(false);
                enemy.EnemyController.Clear();
            }
        }

        private void HandleDismounted(Message message)
        {
            var characterId = message.GetInt();
            var instanceId  = message.GetUShort();

            if (!_characterToTransport.TryGetValue(characterId, out var storedId)) return;
            if (storedId != instanceId) return;

            var currentChar = _characterService.CurrentCharacter;
            if (currentChar != null && currentChar.Id == characterId)
            {
                _localTransport.DismountLocal(currentChar, _characterService);
                _localTransport.OnInteractRequested = MountTransport;
                _isLocalMounted = false;
                _inputService.OnInteractEvent -= HandleDismountInput;
                _localTransport.EnableInteract();
                OnLocalDismounted?.Invoke();
            }
            else if (_enemyTransports.TryGetValue(instanceId, out var enemyTransport))
            {
                enemyTransport.Dismount();
                if (_characterService.OtherCharacters.TryGetValue(characterId, out var enemy))
                    enemy.SetVisible(true);
            }

            _characterToTransport.Remove(characterId);
        }

        // --- CharacterService subscription ---

        private void OnCharacterMountedTick(int characterId, int tick, Vector3 position, float rotation,
            int transportTypeId)
        {
            if (_characterService.CurrentCharacter?.Id == characterId) return;

            if (!_characterToTransport.TryGetValue(characterId, out var instanceId)) return;
            if (_enemyTransports.TryGetValue(instanceId, out var view))
                view.ApplySnapshot(new EnemySnapshot(tick, position, rotation));
        }

        // --- Helpers ---

        private void HandleDismountInput()
        {
            _localTransport?.DisableInteract();
            DismountTransport();
        }
    }
}

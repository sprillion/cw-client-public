using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using infrastructure.services.auth;
using infrastructure.services.clan;
using infrastructure.services.platform;
using infrastructure.services.craft;
using infrastructure.services.gameLoop;
using infrastructure.services.house;
using infrastructure.services.input;
using infrastructure.services.inventory;
using infrastructure.services.map;
using infrastructure.services.npc;
using infrastructure.services.players;
using infrastructure.services.shop;
using infrastructure.services.transport;
using infrastructure.services.users;
using network;
using tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace infrastructure.services.loading
{
    public class LoadingService : MonoBehaviour, ILoadingService
    {
        private IMapService _mapService;
        private IPlayerService _playerService;
        private IAuthorization _authorization;
        private PlatformAuthFlow _platformAuthFlow;
        private INetworkManager _networkManager;
        private IGameLoop _gameLoop;
        private IInputService _inputService;
        private IInventoryService _inventoryService;
        private ICharacterService _characterService;
        private INpcService _npcService;
        private ICraftService _craftService;
        private IHouseService _houseService;
        private IShopService _shopService;
        private IClanService _clanService;
        private ITransportService _transportService;

        private bool _mapLoaded;
        private bool _authorized;
        private bool _playerLoaded;
        private bool _inventoryLoaded;
        private bool _statsLoaded;
        private bool _mapCreated;
        private bool _npcLoaded;
        private bool _shopDataLoaded;

        private CancellationTokenSource _connectionCts;

        public bool IsLoading { get; private set; }
        public LoadingStage CurrentStage { get; private set; }

        public event Action Loaded;
        public event Action LoadingMapException;
        public event Action<LoadingStage> OnStageChanged;
        public event Action OnServerUnavailable;
        public event Action OnVersionMismatch;

        private void SetStage(LoadingStage stage)
        {
            _gameLoop.AddToUnityThread(() =>
            {
                CurrentStage = stage;
                OnStageChanged?.Invoke(stage);
            });
        }

        [Inject]
        public void Construct(
            IMapService mapService,
            IPlayerService playerService,
            IAuthorization authorization,
            PlatformAuthFlow platformAuthFlow,
            INetworkManager networkManager,
            IGameLoop gameLoop,
            IInputService inputService,
            IInventoryService inventoryService,
            ICharacterService characterService,
            INpcService npcService,
            ICraftService craftService,
            IHouseService houseService,
            IShopService shopService,
            IClanService clanService,
            ITransportService transportService
        )
        {
            _mapService = mapService;
            _playerService = playerService;
            _authorization = authorization;
            _platformAuthFlow = platformAuthFlow;
            _networkManager = networkManager;
            _gameLoop = gameLoop;
            _inputService = inputService;
            _inventoryService = inventoryService;
            _characterService = characterService;
            _npcService = npcService;
            _craftService = craftService;
            _houseService = houseService;
            _shopService = shopService;
            _clanService = clanService;
            _transportService = transportService;

            Loaded += () => _inputService.LockCursor();
        }

        private void Start()
        {
            IsLoading = true;
            Connection();
        }

        private void Connection()
        {
            Debug.Log("[Loading] Stage: Connecting to server...");
            SetStage(LoadingStage.Connecting);
            _networkManager.OnServerConnected += OnConnected;
            _networkManager.OnServerDisconnected += OnDisconnectedDuringLoad;
            _networkManager.Connect();
            WaitForConnectionTimeout().Forget();
        }

        private async UniTaskVoid WaitForConnectionTimeout()
        {
            _connectionCts?.Cancel();
            _connectionCts = new CancellationTokenSource();
            var cancelled = await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: _connectionCts.Token)
                .SuppressCancellationThrow();
            if (cancelled) return;
            ShowServerUnavailable();
        }

        private void OnConnected()
        {
            Debug.Log("[Loading] Server connected. Proceeding to version check.");
            _connectionCts?.Cancel();
            _networkManager.OnServerConnected -= OnConnected;
            _networkManager.OnServerDisconnected -= OnDisconnectedDuringLoad;
            _gameLoop.AddToUnityThread(CheckVersion);
        }

        private void OnDisconnectedDuringLoad()
        {
            Debug.LogWarning("[Loading] Disconnected during load.");
            _connectionCts?.Cancel();
            ShowServerUnavailable();
        }

        private void ShowServerUnavailable()
        {
            _networkManager.OnServerConnected -= OnConnected;
            _networkManager.OnServerDisconnected -= OnDisconnectedDuringLoad;
            SetStage(LoadingStage.ServerUnavailable);
            _gameLoop.AddToUnityThread(() => OnServerUnavailable?.Invoke());
        }

        public void Retry()
        {
            Connection();
        }

        private void CheckVersion()
        {
            Debug.Log("[Loading] Stage: CheckingVersion...");
            SetStage(LoadingStage.CheckingVersion);
            _networkManager.OnVersionCheckResult += OnVersionCheckResult;
            var message = new Message(MessageType.Network)
                .AddByte(NetworkManager.FromClientMessage.CheckVersion.ToByte())
                .AddString(Application.version);
            _networkManager.SendMessage(message);
        }

        private void OnVersionCheckResult(bool isValid)
        {
            Debug.Log($"[Loading] Version check result: {(isValid ? "OK" : "MISMATCH")}");
            _networkManager.OnVersionCheckResult -= OnVersionCheckResult;
            if (!isValid)
            {
                SetStage(LoadingStage.VersionMismatch);
                _gameLoop.AddToUnityThread(() => OnVersionMismatch?.Invoke());
                return;
            }
            LoadingResources();
        }

        private void LoadingResources()
        {
            Debug.Log("[Loading] Stage: LoadingMap + Npc (parallel)...");
            SetStage(LoadingStage.LoadingMap);
            _npcService.OnNpcLoaded += OnNpcLoaded;
            _gameLoop.AddToUnityThread(_npcService.LoadAllNpc);
            _mapService.OnMapLoaded += OnMapLoaded;
            _gameLoop.AddToUnityThread(_mapService.LoadMap);
        }

        private void LoadData()
        {
            Debug.Log("[Loading] LoadData: requesting shop data...");
            _shopService.OnDataLoaded += OnShopDataLoaded;
            _gameLoop.AddToUnityThread(_shopService.LoadData);
            _gameLoop.AddToUnityThread(_transportService.LoadData);
        }

        private void Loading()
        {
            Debug.Log("[Loading] Stage: Authorizing (platform auth + player/inventory/stats)...");
            SetStage(LoadingStage.Authorizing);
            _playerService.OnClientPlayerLoaded += OnPlayerLoaded;
            _inventoryService.OnInventoryLoaded += OnInventoryLoaded;
            _characterService.OnStatsLoaded += OnStatsLoaded;
            _gameLoop.AddToUnityThread(() => _platformAuthFlow.Run().Forget());
            LoadData();
        }

        private void CheckToLoading()
        {
            Debug.Log($"[Loading] CheckToLoading: mapLoaded={_mapLoaded}, npcLoaded={_npcLoaded}, mapCreated={_mapCreated}");
            if (!_mapLoaded || !_npcLoaded || _mapCreated) return;
            Debug.Log("[Loading] All resources ready — starting auth and data loading.");
            _gameLoop.AddToUnityThread(Loading);
        }

        private void OnNpcLoaded()
        {
            Debug.Log("[Loading] NPC data loaded.");
            _npcService.OnNpcLoaded -= OnNpcLoaded;
            _npcLoaded = true;
            _gameLoop.AddToUnityThread(CheckToLoading);
        }

        private void OnMapLoaded(bool success)
        {
            Debug.Log($"[Loading] Map loaded: success={success}");
            if (!success)
            {
                Debug.LogWarning("[Loading] Map load FAILED — retrying...");
                LoadingMapException?.Invoke();
                _mapService.LoadMap();
                return;
            }

            _mapService.OnMapLoaded -= OnMapLoaded;
            _mapLoaded = true;
            _gameLoop.AddToUnityThread(CheckToLoading);
        }

        private void OnPlayerLoaded()
        {
            Debug.Log("[Loading] Player loaded.");
            SetStage(LoadingStage.LoadingPlayerData);
            _playerService.OnClientPlayerLoaded -= OnPlayerLoaded;
            _playerLoaded = true;
            CheckToCreateMap();
            _gameLoop.AddToUnityThread(_craftService.LoadCraftData);
            _gameLoop.AddToUnityThread(_houseService.LoadHouseConfig);
        }

        private void OnInventoryLoaded()
        {
            Debug.Log("[Loading] Inventory loaded.");
            _inventoryService.OnInventoryLoaded -= OnInventoryLoaded;
            _inventoryLoaded = true;
            CheckToCreateMap();
        }

        private void OnStatsLoaded()
        {
            Debug.Log("[Loading] Stats loaded.");
            _characterService.OnStatsLoaded -= OnStatsLoaded;
            _statsLoaded = true;
            CheckToCreateMap();
        }

        private void OnShopDataLoaded()
        {
            Debug.Log("[Loading] Shop data loaded.");
            _shopService.OnDataLoaded -= OnShopDataLoaded;
        }

        private void CheckToCreateMap()
        {
            Debug.Log($"[Loading] CheckToCreateMap: player={_playerLoaded}, inventory={_inventoryLoaded}, stats={_statsLoaded}, map={_mapLoaded}, npc={_npcLoaded}, mapCreated={_mapCreated}");
            if (!_playerLoaded || !_inventoryLoaded || !_statsLoaded || !_mapLoaded || !_npcLoaded || _shopDataLoaded || _mapCreated) return;
            Debug.Log("[Loading] All data ready — creating map.");
            _gameLoop.AddToUnityThread(CreateMap);
        }

        private void CreateMap()
        {
            Debug.Log($"[Loading] Stage: CreatingWorld at position {_playerService.ClientPlayer.StartPosition}...");
            SetStage(LoadingStage.CreatingWorld);
            _mapCreated = true;
            _mapService.ChunksUpdated += OnMapCreated;
            _mapService.CreateStartChunks(_playerService.ClientPlayer.StartPosition);
        }

        private void OnMapCreated()
        {
            Debug.Log("[Loading] Chunks created. Finalizing...");
            _mapService.ChunksUpdated -= OnMapCreated;
            FinishLoad().Forget();
        }

        private async UniTaskVoid FinishLoad()
        {
            Debug.Log("[Loading] Stage: Finalizing...");
            SetStage(LoadingStage.Finalizing);
            await UniTask.Delay(TimeSpan.FromSeconds(1));

            _characterService.CurrentCharacter.CharacterMovement.SetPosition(_playerService.ClientPlayer.StartPosition);
            _characterService.LaunchSendPosition();
            _mapService.LaunchCheckPosition();
            _clanService.RequestClansSettings();
            _clanService.RequestMyClan();
            _transportService.GetTransports();

            Debug.Log("[Loading] DONE — game is ready.");
            IsLoading = false;
            Loaded?.Invoke();

            _networkManager.OnServerDisconnected += OnDisconnectedDuringGame;
        }

        private void OnDisconnectedDuringGame()
        {
            _networkManager.OnServerDisconnected -= OnDisconnectedDuringGame;
            ReconnectAfterDelay().Forget();
        }

        private async UniTaskVoid ReconnectAfterDelay()
        {
            SetStage(LoadingStage.Reconnecting);
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

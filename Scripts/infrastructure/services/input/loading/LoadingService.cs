using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.auth;
using infrastructure.services.gameLoop;
using infrastructure.services.input;
using infrastructure.services.inventory;
using infrastructure.services.map;
using infrastructure.services.npc;
using infrastructure.services.players;
using infrastructure.services.users;
using network;
using UnityEngine;
using Zenject;

namespace infrastructure.services.loading
{
    public class LoadingService : MonoBehaviour, ILoadingService
    {
        private IMapService _mapService;
        private IPlayerService _playerService;
        private IAuthorization _authorization;
        private INetworkManager _networkManager;
        private IGameLoop _gameLoop;
        private IInputService _inputService;
        private IInventoryService _inventoryService;
        private ICharacterService _characterService;
        private INpcService _npcService;

        private bool _mapLoaded;
        private bool _authorized;
        private bool _playerLoaded;
        private bool _inventoryLoaded;
        private bool _statsLoaded;
        private bool _mapCreated;
        private bool _npcLoaded;
        
        public bool IsLoading { get; private set; }

        public event Action Loaded;
        public event Action LoadingMapException;
        
        // коннект к серверу
        // загрузка карты
        // авторизация
        //

        [Inject]
        public void Construct(
            IMapService mapService,
            IPlayerService playerService,
            IAuthorization authorization,
            INetworkManager networkManager,
            IGameLoop gameLoop,
            IInputService inputService,
            IInventoryService inventoryService,
            ICharacterService characterService,
            INpcService npcService
        )
        {
            _mapService = mapService;
            _playerService = playerService;
            _authorization = authorization;
            _networkManager = networkManager;
            _gameLoop = gameLoop;
            _inputService = inputService;
            _inventoryService = inventoryService;
            _characterService = characterService;
            _npcService = npcService;

            Loaded += () => _inputService.LockCursor();
        }

        private void Start()
        {
            IsLoading = true;
            Connection();
        }

        private void Connection()
        {
            _networkManager.OnServerConnected += LoadingResources;
            _networkManager.Connect();
        }

        private void LoadingResources()
        {
            _networkManager.OnServerConnected -= Loading;
            _npcService.OnNpcLoaded += OnNpcLoaded;
            _gameLoop.AddResponseAction(_npcService.LoadAllNpc);
            _mapService.OnMapLoaded += OnMapLoaded;
            _gameLoop.AddResponseAction(_mapService.LoadMap);
        }

        private void Loading()
        {
            _playerService.OnClientPlayerLoaded += OnPlayerLoaded;
            _inventoryService.OnInventoryLoaded += OnInventoryLoaded;
            _characterService.OnStatsLoaded += OnStatsLoaded;
            _gameLoop.AddResponseAction(_authorization.Authorize);
        }
        
        private void CheckToLoading()
        {
            if (!_mapLoaded || !_npcLoaded || _mapCreated) return;
            _gameLoop.AddResponseAction(Loading);
        }
        
        private void OnNpcLoaded()
        {
            _npcService.OnNpcLoaded -= OnNpcLoaded;
            _npcLoaded = true;
            _gameLoop.AddResponseAction(CheckToLoading);
        }

        private void OnMapLoaded(bool success)
        {
            if (!success)
            {
                LoadingMapException?.Invoke();
                _mapService.LoadMap();
                return;
            }

            _mapService.OnMapLoaded -= OnMapLoaded;
            _mapLoaded = true;
            _gameLoop.AddResponseAction(CheckToLoading);
        }

        private void OnPlayerLoaded()
        {
            _playerService.OnClientPlayerLoaded -= OnPlayerLoaded;
            _playerLoaded = true;
            CheckToCreateMap();
        }

        private void OnInventoryLoaded()
        {
            _inventoryService.OnInventoryLoaded -= OnInventoryLoaded;
            _inventoryLoaded = true;
            CheckToCreateMap();
        }

        private void OnStatsLoaded()
        {
            _characterService.OnStatsLoaded -= OnStatsLoaded;
            _statsLoaded = true;
            CheckToCreateMap();
        }


        private void CheckToCreateMap()
        {
            if (!_playerLoaded || !_inventoryLoaded || !_statsLoaded || !_mapLoaded || !_npcLoaded || _mapCreated) return;
            _gameLoop.AddResponseAction(CreateMap);
        }

        private void CreateMap()
        {
            _mapCreated = true;
            _mapService.ChunksUpdated += OnMapCreated;
            _mapService.CreateStartChunks(_playerService.ClientPlayer.StartPosition);
        }

        private void OnMapCreated()
        {
            _mapService.ChunksUpdated -= OnMapCreated;
            FinishLoad().Forget();
        }

        private async UniTaskVoid FinishLoad()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            
            _characterService.CurrentCharacter.SetPosition(_playerService.ClientPlayer.StartPosition);
            _characterService.LaunchSendPosition();
            _mapService.LaunchCheckPosition();
            
            IsLoading = false;
            Loaded?.Invoke();
        }
    }
}
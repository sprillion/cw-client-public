using factories.characters;
using factories.inventory;
using factories.mobs;
using infrastructure.factories.environment;
using infrastructure.services.auth;
using infrastructure.services.chests;
using infrastructure.services.gameLoop;
using infrastructure.services.input;
using infrastructure.services.inventory;
using infrastructure.services.loading;
using infrastructure.services.map;
using infrastructure.services.map.chunk;
using infrastructure.services.mobs;
using infrastructure.services.npc;
using infrastructure.services.players;
using infrastructure.services.ui;
using infrastructure.services.ui.popup;
using infrastructure.services.users;
using UnityEngine;
using Zenject;

namespace infrastructure
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private PlayerInputController _playerInputController;
        [SerializeField] private MapService _mapService;
        [SerializeField] private LoadingService _loadingService;
        [SerializeField] private UiService _uiService;
        
        public override void InstallBindings()
        {
            BindGameLoop();
            
            BindPlayerInputController();
            BindInputService();

            BindEnvironmentFactory();
            BindNpcService();
            
            BindCharacterFactory();
            BindCharacterService();
            BindPlayerService();
            
            BindMobFactory();
            BindMobService();

            BindInventoryFactory();
            BindInventoryService();
            
            BindChestsService();
            
            BindAuthorization();
            
            BindChunkFactory();
            BindBlockTextureData();
            //BindTerrainGenerator();
            BindMapService();
            
            BindUiService();
            BindPopupService();
            
            BindLoadingService();
        }
        
        private void BindGameLoop()
        {
            Container
                .Bind<IGameLoop>()
                .To<GameLoop>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindAuthorization()
        {
            Container
                .Bind<IAuthorization>()
                .To<Authorization>()
                .AsSingle()
                .NonLazy();
        }

        private void BindCharacterService()
        {
            Container
                .Bind<ICharacterService>()
                .To<CharacterService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindPlayerService()
        {
            Container
                .Bind<IPlayerService>()
                .To<PlayerService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindCharacterFactory()
        {
            Container
                .Bind<ICharacterFactory>()
                .To<CharacterFactory>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindMobService()
        {
            Container
                .Bind<IMobService>()
                .To<MobService>()
                .AsSingle()
                .NonLazy();
        }
                
        private void BindMobFactory()
        {
            Container
                .Bind<IMobFactory>()
                .To<MobFactory>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindPlayerInputController()
        {
            Container
                .Bind<IPlayerInputController>()
                .To<PlayerInputController>()
                .FromComponentInNewPrefab(_playerInputController)
                .AsSingle()
                .NonLazy();
        }

        private void BindInputService()
        {
            Container
                .Bind<IInputService>()
                .To<InputService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindMapService()
        {
            Container
                .Bind<IMapService>()
                .To<MapService>()
                .FromComponentInNewPrefab(_mapService)
                .AsSingle()
                .NonLazy();
        }
        
        private void BindBlockTextureData()
        {
            Container
                .Bind<BlockTextureData>()
                .To<BlockTextureData>()
                .FromScriptableObjectResource("Data/Map/BlockTextureData")
                .AsSingle()
                .NonLazy();
        }
        private void BindChunkFactory()
        {
            Container
                .Bind<IChunkFactory>()
                .To<ChunkFactory>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLoadingService()
        {
            Container
                .Bind<ILoadingService>()
                .To<LoadingService>()
                .FromComponentInNewPrefab(_loadingService)
                .AsSingle()
                .NonLazy();
        }
        
        private void BindInventoryService()
        {
            Container
                .Bind<IInventoryService>()
                .To<InventoryService>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindInventoryFactory()
        {
            Container
                .Bind<IInventoryFactory>()
                .To<InventoryFactory>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindChestsService()
        {
            Container
                .Bind<IChestsService>()
                .To<ChestsService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindEnvironmentFactory()
        {
            Container
                .Bind<IEnvironmentFactory>()
                .To<EnvironmentFactory>()
                .AsSingle()
                .NonLazy();
        }

        private void BindUiService()
        {
            Container
                .Bind<IUiService>()
                .To<UiService>()
                .FromComponentOn(_uiService.gameObject)
                .AsSingle()
                .NonLazy();
        }

        private void BindPopupService()
        {
            Container
                .Bind<IPopupService>()
                .To<PopupService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindNpcService()
        {
            Container
                .Bind<INpcService>()
                .To<NpcService>()
                .AsSingle()
                .NonLazy();
        }
    }
}
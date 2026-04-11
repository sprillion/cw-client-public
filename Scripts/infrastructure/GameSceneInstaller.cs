using factories.characters;
using factories.inventory;
using factories.mobs;
using infrastructure.factories;
using infrastructure.factories.environment;
using infrastructure.services.auth;
using infrastructure.services.platform;
using infrastructure.services.chat;
using infrastructure.services.chests;
using infrastructure.services.clan;
using infrastructure.services.craft;
using infrastructure.services.gameLoop;
using infrastructure.services.input;
using infrastructure.services.inventory;
using infrastructure.services.loading;
using infrastructure.services.map;
using infrastructure.services.map.jobs;
using infrastructure.services.mapMarkers;
using infrastructure.services.navigation;
using infrastructure.services.waypoint;
using infrastructure.services.lumber;
using infrastructure.services.mine;
using infrastructure.services.mobs;
using infrastructure.services.npc;
using infrastructure.services.players;
using infrastructure.services.quests;
using infrastructure.services.house;
using infrastructure.services.interior;
using infrastructure.services.shop;
using infrastructure.services.settings;
using infrastructure.services.proximity;
using infrastructure.services.transport;
using infrastructure.services.sounds;
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
        [SerializeField] private SoundService _soundService;
        
        public override void InstallBindings()
        {
            Pool.Initialize(Container);
            
            BindGameLoop();
            
            BindChatService();
            BindClanService();

            BindPlayerInputController();
            BindInputService();

            BindEnvironmentFactory();
            BindQuestsService();
            BindNpcService();
            
            BindCharacterFactory();
            BindCharacterService();
            BindPlayerService();
            
            BindMobFactory();
            BindMobService();

            BindTransportService();
            BindNavigationService();

            BindInventoryFactory();
            BindInventoryService();
            
            BindCraftService();
            BindMineService();
            BindLumberService();

            BindShopService();
            BindHouseService();
            BindInteriorService();

            BindChestsService();
            
            BindAuthorization();
            BindPlatformAuthFlow();

            BindSettingsService();
            BindSoundService();

            BindBlockTextureData();
            BindMeshBakery();
            //BindTerrainGenerator();
            BindMapService();
            BindProximityService();
            BindMapMarkerService();
            BindWaypointService();
            BindQuestMarkerService();

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

        private void BindPlatformAuthFlow()
        {
            Container
                .Bind<PlatformAuthFlow>()
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

        private void BindMeshBakery()
        {
            Container
                .Bind<MeshBakery>()
                .FromNewComponentOnNewGameObject()
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
        
        private void BindQuestsService()
        {
            Container
                .Bind<IQuestsService>()
                .To<QuestsService>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindChatService()
        {
            Container
                .Bind<IChatService>()
                .To<ChatService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindClanService()
        {
            Container
                .Bind<IClanService>()
                .To<ClanService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindCraftService()
        {
            Container
                .Bind<ICraftService>()
                .To<CraftService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindShopService()
        {
            Container
                .Bind<IShopService>()
                .To<ShopService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindHouseService()
        {
            Container
                .Bind<IHouseService>()
                .To<HouseService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindInteriorService()
        {
            Container
                .Bind<IInteriorService>()
                .To<InteriorService>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindMineService()
        {
            Container
                .Bind<IMineService>()
                .To<MineService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLumberService()
        {
            Container
                .Bind<ILumberService>()
                .To<LumberService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindTransportService()
        {
            Container
                .Bind<ITransportService>()
                .To<TransportService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindSettingsService()
        {
            Container
                .Bind<ISettingsService>()
                .To<SettingsService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindSoundService()
        {
            Container
                .Bind<ISoundService>()
                .To<SoundService>()
                .FromComponentInNewPrefab(_soundService)
                .AsSingle()
                .NonLazy();
        }

        private void BindProximityService()
        {
            Container
                .Bind<IProximityService>()
                .To<ProximityService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindMapMarkerService()
        {
            Container
                .Bind<IMapMarkerService>()
                .To<MapMarkerService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindWaypointService()
        {
            Container
                .Bind<IWaypointService>()
                .To<WaypointService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindQuestMarkerService()
        {
            Container
                .Bind<IQuestMarkerService>()
                .To<QuestMarkerService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindNavigationService()
        {
            Container
                .Bind<INavigationService>()
                .To<NavigationService>()
                .AsSingle()
                .NonLazy();
        }
    }
}
using Cysharp.Threading.Tasks;
using environment;
using environment.chests;
using environment.house;
using environment.interior;
using environment.lumber;
using environment.mine;
using infrastructure.services.house;
using infrastructure.services.interior;
using infrastructure.services.loading;
using infrastructure.services.lumber;
using infrastructure.services.npc;
using infrastructure.services.players;
using infrastructure.services.users;
using ui.admin;
using UnityEngine.InputSystem;
using ui.clan;
using ui.confirm;
using ui.settings;
using ui.death;
using ui.house;
using ui.interaction;
using ui.inventory;
using ui.inventory.character;
using ui.inventory.chest;
using ui.lumber;
using ui.mine;
using ui.npc;
using ui.quest;
using ui.quickPanel;
using ui.shop;
using UnityEngine;
using Zenject;

namespace infrastructure.services.ui
{
    public class UiService : MonoBehaviour, IUiService
    {
        [Header("Views")]
        [SerializeField] private Inventory _inventory;
        [SerializeField] private ChestPopup _chestPopup;
        [SerializeField] private CharacterPopup _characterPopup;
        [SerializeField] private NpcPopup _npcPopup;
        [SerializeField] private QuestsInfoPopup _questsInfoPopup;
        [SerializeField] private ConfirmView _confirmView;
        [SerializeField] private DeathView _deathView;
        [SerializeField] private BuyPlotView _buyPlotView;
        [SerializeField] private ShopView _shopView;
        [SerializeField] private MineView _mineView;
        [SerializeField] private LumberView _lumberView;
        [SerializeField] private ClanView _clanView;
        [SerializeField] private SettingsPopup _settingsPopup;

        [SerializeField] private AdminPanelView _adminPanel;

        [SerializeField] private QuickPanel _quickPanel;
        [SerializeField] private StatsPanel _statsPanel;
        
        [Header("Interaction")]
        [SerializeField] private InteractButton _interactButton;
        [SerializeField] private GameObject _interactPanel;

        [Header("Canvases")]
        [SerializeField] private GameObject _allCanvases;
        [SerializeField] private GameObject _chatCanvases;

        private ILoadingService _loadingService;
        private INpcService _npcService;
        private IPlayerService _playerService;
        private ILumberService _lumberService;
        private IHouseService _houseService;
        private IInteriorService _interiorService;

        public Interaction Interaction { get; private set; }
        public Inventory Inventory => _inventory;
        public QuestsInfoPopup QuestsInfoPopup => _questsInfoPopup;
        public ConfirmView ConfirmView => _confirmView;
        public AdminPanelView AdminPanel => _adminPanel;
        
        [Inject]
        public void Construct(ILoadingService loadingService, ICharacterService characterService,
                              INpcService npcService, IPlayerService playerService,
                              ILumberService lumberService, IHouseService houseService,
                              IInteriorService interiorService)
        {
            _loadingService = loadingService;
            _npcService = npcService;
            _playerService = playerService;
            _lumberService = lumberService;
            _houseService = houseService;
            _interiorService = interiorService;
            _houseService.OnPlotStatusReceived += HandlePlotStatus;
            _loadingService.Loaded += InitializeUi;

            Interaction = new Interaction(characterService, _interactButton, _interactPanel);
            Interaction.OnInteract += ShowInteractView;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!kb.digit1Key.wasPressedThisFrame) return;
            if (!kb.leftAltKey.isPressed && !kb.rightAltKey.isPressed) return;
            if (_playerService.ClientPlayer == null) return;
            if (_playerService.ClientPlayer.Role < PlayerRole.Moderator) return;

            if (_adminPanel.IsActive) _adminPanel.Hide();
            else _adminPanel.Show();
        }

        private void InitializeUi()
        {
            _loadingService.Loaded -= InitializeUi;
            _statsPanel.Initialize();
            _inventory.Initialize();
            _quickPanel.Initialize();
            _characterPopup.Initialize();
            _npcPopup.Initialize();
            _confirmView.Initialize();
            _deathView.Initialize();
            _buyPlotView.Initialize();
            _shopView.Initialize();
            _mineView.Initialize();
            _lumberView.Initialize();
            _clanView.Initialize();
            _settingsPopup.Initialize();
            _adminPanel.Initialize();
        }

        private void ShowInteractView(IInteractable interactable)
        {
            switch (interactable)
            {
                case Npc npc:
                    _npcService.CurrentNpcData = npc.NpcData;
                    _npcPopup.Show();
                    break;
                case ChestFromMob chestFromMob:
                    _chestPopup.SetChestFromMob(chestFromMob);
                    _chestPopup.Show();
                    _inventory.Show();
                    break;
                case HouseEnter:
                    _houseService.GetPlot();
                    break;
                case InteriorExit interiorExit:
                    interiorExit.Interact();
                    break;
                case MineEnter:
                    _mineView.Show();
                    break;
                case LumberTree lumberTree:
                    _lumberService.CurrentTreeId = lumberTree.TreeId;
                    _lumberView.Show();
                    break;
            }
        }

        public void SetAllCanvasesActive(bool active) => _allCanvases.SetActive(active);

        private void HandlePlotStatus(bool hasPlot)
        {
            if (hasPlot)
                _interiorService.Enter(InteriorType.House).Forget();
            else
                _buyPlotView.Show();
        }
    }
}
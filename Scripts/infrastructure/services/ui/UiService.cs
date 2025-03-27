using environment;
using environment.chests;
using infrastructure.services.loading;
using infrastructure.services.npc;
using infrastructure.services.players;
using ui.interaction;
using ui.inventory;
using ui.inventory.character;
using ui.inventory.chest;
using ui.npc;
using ui.quickPanel;
using UnityEngine;
using Zenject;

namespace infrastructure.services.ui
{
    public class UiService : MonoBehaviour, IUiService
    {
        [SerializeField] private Inventory _inventory;
        [SerializeField] private ChestPopup _chestPopup;
        [SerializeField] private CharacterPopup _characterPopup;
        [SerializeField] private NpcPopup _npcPopup;
        
        [SerializeField] private QuickPanel _quickPanel;
        [SerializeField] private StatsPanel _statsPanel;
        
        [SerializeField] private InteractButton _interactButton;

        private ILoadingService _loadingService;
        private INpcService _npcService;

        public Interaction Interaction { get; private set; }
        public Inventory Inventory => _inventory;
        
        [Inject]
        public void Construct(ILoadingService loadingService, ICharacterService characterService, INpcService npcService)
        {
            _loadingService = loadingService;
            _npcService = npcService;
            _loadingService.Loaded += InitializeUi;

            Interaction = new Interaction(characterService, _interactButton);
            Interaction.OnInteract += ShowInteractView;
        }

        private void InitializeUi()
        {
            _loadingService.Loaded -= InitializeUi;
            _inventory.Initialize();
            _quickPanel.Initialize();
            _statsPanel.Initialize();
            _npcPopup.Initialize();
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
            }
        }
    }
}
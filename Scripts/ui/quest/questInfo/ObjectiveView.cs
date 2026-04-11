using factories.inventory;
using I2.Loc;
using infrastructure.services.inventory;
using infrastructure.services.mobs;
using infrastructure.services.quests;
using TMPro;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.quest
{
    public class ObjectiveView : PooledObject
    {
        [SerializeField] private TMP_Text _objectiveText;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Image _icon;
        [SerializeField] private Button _transferButton;
        [SerializeField] private Button _waypointButton;
        [SerializeField] private Image _waypointButtonIcon;
        [SerializeField] private Sprite _markerActiveSprite;
        [SerializeField] private Sprite _markerInactiveSprite;

        private Objective _currentObjective;
        private bool _isCurrent;

        private SpriteCatalog _mobIcons;

        private IInventoryFactory _inventoryFactory;
        private IQuestsService _questsService;
        private IInventoryService _inventoryService;
        private IQuestMarkerService _questMarkerService;

        [Inject]
        public void Construct(IInventoryFactory inventoryFactory, IQuestsService questsService,
            IInventoryService inventoryService, IQuestMarkerService questMarkerService)
        {
            _inventoryFactory = inventoryFactory;
            _questsService = questsService;
            _inventoryService = inventoryService;
            _questMarkerService = questMarkerService;
            _questMarkerService.OnMarkedObjectiveChanged += _ => UpdateWaypointButtonIcon();
        }

        private void Awake()
        {
            _mobIcons = GameResources.Data.Catalogs.mob_icons<SpriteCatalog>();
            _transferButton.onClick.AddListener(TransferItems);
            _waypointButton.onClick.AddListener(SetWaypoint);
        }

        public void SetInfo(Objective objective, bool isCurrent)
        {
            _currentObjective = objective;
            _isCurrent = isCurrent;
            
            _objectiveText.text = LocalizationManager.GetTranslation($"Quest/Objective/{objective.ObjectiveType}");
            _countText.text = isCurrent ? $"{objective.CurrentValue}/{objective.TargetValue}" : $"x{objective.TargetValue}";
            _transferButton.gameObject.SetActive(isCurrent && objective.ObjectiveType == ObjectiveType.TransferItems);
            _transferButton.interactable = isCurrent && objective.ObjectiveType == ObjectiveType.TransferItems &&
                                           _inventoryService.CountItems(_currentObjective.TargetId) > 0;
            _waypointButton.gameObject.SetActive(objective.HasPosition);
            UpdateWaypointButtonIcon();
            
            _icon.sprite = objective.ObjectiveType switch
            {
                ObjectiveType.KillMob => _mobIcons.GetSprite((MobType)objective.TargetId),
                ObjectiveType.FindItems => _inventoryFactory.GetItemData(objective.TargetId).Icon,
                ObjectiveType.TransferItems => _inventoryFactory.GetItemData(objective.TargetId).Icon,
                _ => null
            };
        }

        public void UpdateInfo()
        {
            if (_currentObjective == null) return;
            SetInfo(_currentObjective, _isCurrent);
        }

        private void TransferItems()
        {
            if (_inventoryService.CountItems(_currentObjective.TargetId) == 0) return;
            _questsService.TransferItems(_currentObjective.Quest.Id, _currentObjective.Id);
        }

        private void SetWaypoint()
        {
            _questMarkerService.SetMarker(_currentObjective);
        }

        private void UpdateWaypointButtonIcon()
        {
            if (_waypointButtonIcon == null) return;
            bool isMarked = _questMarkerService.MarkedObjective == _currentObjective;
            _waypointButtonIcon.sprite = isMarked ? _markerActiveSprite : _markerInactiveSprite;
        }
    }
}
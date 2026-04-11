using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using infrastructure.factories;
using infrastructure.services.npc;
using infrastructure.services.quests;
using ui.popup;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.quest
{
    public class QuestsPopup : Popup
    {
        [SerializeField] private QuestsInfoPopup _questsInfoPopup;
        
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private bool _changeBackground;

        [SerializeField] private DropdownButton _currentDropdown;
        [SerializeField] private DropdownButton _availableDropdown;

        [SerializeField] private Transform _currentParent;
        [SerializeField] private Transform _availableParent;

        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _noHaveCurrentQuests;
        [SerializeField] private GameObject _noHaveAvailableQuests;
        [SerializeField] private LoadingPanel _loadingPanel;

        private readonly List<QuestPanel> _currentQuests = new List<QuestPanel>();
        private readonly List<QuestPanel> _availableQuests = new List<QuestPanel>();

        private IQuestsService _questsService;
        private INpcService _npcService;

        private bool _isFromNpc;
        private NpcType _currentNpc;

        private QuestPanel _currentSelectedQuest;

        [Inject]
        public void Construct(IQuestsService questsService, INpcService npcService)
        {
            _questsService = questsService;
            _npcService = npcService;
        }

        public override void Show()
        {
            if (IsActive) return;
            if (_changeBackground && _npcService.CurrentNpcData)
            {
                _backgroundImage.color = _npcService.CurrentNpcData.BackgroundColor;
            }

            _loadingPanel.Show();
            ClearQuests();
            _questsService.OnAvailableQuestsLoaded += SetQuests;
            _questsService.OnCurrentQuestAdded += AddCurrentQuest;
            _questsService.OnQuestRemoved += RemoveCurrentQuest;
            _questsService.GetAvailableQuests();
            base.Show();
            UpdateLayouts();
        }

        public override void Hide()
        {
            _isFromNpc = false;
            _questsService.OnAvailableQuestsLoaded -= SetQuests;
            _questsService.OnCurrentQuestAdded -= AddCurrentQuest;
            _questsService.OnQuestRemoved -= RemoveCurrentQuest;
            base.Hide();
        }

        public void SetNpc(NpcType npcType)
        {
            _isFromNpc = true;
            _currentNpc = npcType;
            _questsInfoPopup.SetFromNpc();
        }

        private void SetQuests()
        {
            ClearQuests();
            
            var currentQuests = _questsService.CurrentQuests;
            var availableQuests = _questsService.AvailableQuests;

            if (_isFromNpc)
            {
                currentQuests = currentQuests.Where(q => q.NpcType == _currentNpc).ToList();
                availableQuests = availableQuests.Where(q => q.NpcType == _currentNpc).ToList();
            }

            _noHaveAvailableQuests.SetActive(availableQuests.Count == 0);
            _noHaveCurrentQuests.SetActive(currentQuests.Count == 0);

            currentQuests.ForEach(q =>
            {
                var panel = Pool.Get<QuestPanel>();
                panel.SetQuest(q);
                panel.OnSelected += OnQuestSelected;
                panel.SetParentPreserveScale(_currentParent);
                _currentQuests.Add(panel);
            });

            availableQuests.ForEach(q =>
            {
                var panel = Pool.Get<QuestPanel>();
                panel.SetQuest(q);
                panel.OnSelected += OnQuestSelected;
                panel.SetParentPreserveScale(_availableParent);
                _availableQuests.Add(panel);
            });

            SelectFirstQuest();
            SetTitles(currentQuests.Count, availableQuests.Count);
            _loadingPanel.Hide();
            UpdateLayouts();
        }

        private void AddCurrentQuest(Quest quest)
        {
            var panel = _availableQuests.FirstOrDefault(q => q.CurrentQuest == quest);
            if (panel == null) return;

            _availableQuests.Remove(panel);
            _currentQuests.Add(panel);
            panel.transform.SetParent(_currentParent);
            panel.ShowInfo();
            _noHaveCurrentQuests.SetActive(false);
            UpdateLayouts();
        }
        
        private void RemoveCurrentQuest(Quest quest)
        {
            var panel = _currentQuests.FirstOrDefault(q => q.CurrentQuest == quest);
            if (panel == null) return;

            _currentQuests.Remove(panel);
            panel.Release();
            _noHaveCurrentQuests.SetActive(_currentQuests.Count > 0);
            UpdateLayouts();
        }

        private void ClearQuests()
        {
            _currentQuests.ForEach(q => q.Release());
            _availableQuests.ForEach(q => q.Release());
            _currentQuests.Clear();
            _availableQuests.Clear();
        }

        private void SetTitles(int currentCount, int availableCount)
        {
            _currentDropdown.SetTitle(string.Format(LocalizationManager.GetTranslation("Quest/Current"),
                currentCount));
            _availableDropdown.SetTitle(string.Format(LocalizationManager.GetTranslation("Quest/Available"),
                availableCount));
        }

        private void OnQuestSelected(QuestPanel questPanel)
        {
            if (_currentSelectedQuest == questPanel) return;
            _currentSelectedQuest?.Unselect();
            _currentSelectedQuest = questPanel;
        }

        private void SelectFirstQuest()
        {
            _currentSelectedQuest?.Unselect();
            _currentSelectedQuest = _currentQuests.FirstOrDefault();
            if (_currentSelectedQuest == null)
            {
                _currentSelectedQuest = _availableQuests.FirstOrDefault();
            }
            _currentSelectedQuest?.ShowInfo();
        }

        private void UpdateLayouts()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_availableParent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_currentParent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_currentParent.parent as RectTransform);
        }
    }
}
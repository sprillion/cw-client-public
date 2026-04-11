using System.Collections.Generic;
using I2.Loc;
using infrastructure.factories;
using infrastructure.services.npc;
using infrastructure.services.quests;
using infrastructure.services.waypoint;
using TMPro;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.quest
{
    public class QuestsInfoPopup : Popup
    {
        [SerializeField] private List<Popup> _popupsToClose;
        [SerializeField] private Button _closeBGButton;

        [SerializeField] private VerticalLayoutGroup _layoutGroup;

        [SerializeField] private TMP_Text _questName;
        [SerializeField] private TMP_Text _npcName;
        [SerializeField] private Image _npcIcon;
        [SerializeField] private TMP_Text _description;

        [SerializeField] private Transform _objectivesParent;
        [SerializeField] private Transform _rewardsParent;

        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _completeButton;
        [SerializeField] private Button _waypointButton;

        [SerializeField] private LoadingPanel _loadingPanel;

        private readonly List<ObjectiveView> _objectives = new List<ObjectiveView>();
        private readonly List<RewardView> _rewards = new List<RewardView>();

        private IQuestsService _questsService;
        private INpcService _npcService;
        private IWaypointService _waypointService;

        private Quest _currentQuest;

        private bool _isFromNpc;

        [Inject]
        public void Construct(IQuestsService questsService, INpcService npcService, IWaypointService waypointService)
        {
            _questsService = questsService;
            _npcService = npcService;
            _waypointService = waypointService;

            _questsService.OnShowQuestInfo += SetQuest;
            _questsService.OnUpdateObjectives += UpdateObjectives;

            _closeButton.onClick.AddListener(Back);
            _acceptButton.onClick.AddListener(AcceptQuest);
            _completeButton.onClick.AddListener(CompleteQuest);
            _waypointButton.onClick.AddListener(SetWaypoint);
        }

        public override void Show()
        {
            if (IsActive) return;
            _questsService.OnQuestInfoLoaded += OnInfoLoaded;
            base.Show();
        }

        public override void Hide()
        {
            _popupsToClose.ForEach(popup => popup.Back());
            _questsService.OnQuestInfoLoaded -= OnInfoLoaded;
            _isFromNpc = false;
            base.Hide();
        }

        public void SetFromNpc()
        {
            _isFromNpc = true;
        }

        private void OnEnable()
        {
            _closeBGButton.gameObject.SetActive(true);
            _closeBGButton.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            _closeBGButton.gameObject.SetActive(false);
            _closeBGButton.onClick.RemoveListener(Back);
        }

        private void SetQuest(Quest quest)
        {
            _currentQuest = quest;
            SetInfo();
        }

        private void SetInfo()
        {
            Clear();
            if (_currentQuest.IsLoaded)
            {
                SetFullInfo();
            }
            else
            {
                _loadingPanel.Show();
                _questsService.GetQuestInfo(_currentQuest.Id);
            }

            _questName.text = LocalizationManager.GetTranslation($"QuestNames/{_currentQuest.Id}");
            _description.text = LocalizationManager.GetTranslation($"QuestDescriptions/{_currentQuest.Id}");
            _npcName.text = LocalizationManager.GetTranslation($"Npc/Name/{_currentQuest.NpcType}");
        }

        private void SetFullInfo()
        {
            SetObjective();
            SetReward();
            SetButtons();

            _npcIcon.sprite = _npcService.GetNpcAvatarIcon(_currentQuest.NpcType);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.transform as RectTransform);
            _loadingPanel.Hide();
        }

        private void SetObjective()
        {
            _currentQuest.Objectives.ForEach(objective =>
            {
                var view = Pool.Get<ObjectiveView>();
                view.SetInfo(objective, _currentQuest.IsCurrent);
                view.SetParentPreserveScale(_objectivesParent);
                _objectives.Add(view);
            });
        }

        private void SetReward()
        {
            _currentQuest.Rewards.ForEach(reward =>
            {
                RewardView view = reward.RewardType switch
                {
                    RewardType.Item => Pool.Get<ItemRewardView>(),
                    RewardType.Resource => Pool.Get<ResourceRewardView>()
                };
                view.SetInfo(reward);
                view.SetParentPreserveScale(_rewardsParent);
                _rewards.Add(view);
            });
        }

        private void SetButtons()
        {
            _acceptButton.gameObject.SetActive(_isFromNpc && _currentQuest.IsAvailable && !_currentQuest.IsCompleted);
            _completeButton.gameObject.SetActive(_isFromNpc && !_currentQuest.IsAvailable &&
                                                 !_currentQuest.IsCompleted && _currentQuest.IsProgressCompleted);
            _waypointButton.gameObject.SetActive(_currentQuest.HasNpcPosition);
        }

        private void SetWaypoint()
        {
            _waypointService.SetWaypoint(_currentQuest.NpcPosition);
        }

        private void OnInfoLoaded(int id)
        {
            if (_currentQuest.Id != id || !_currentQuest.IsLoaded) return;
            SetFullInfo();
        }

        private void Clear()
        {
            _objectives.ForEach(objective => objective.Release());
            _rewards.ForEach(reward => reward.Release());
            _objectives.Clear();
            _rewards.Clear();
        }

        private void AcceptQuest()
        {
            _questsService.AcceptQuest(_currentQuest);
        }

        private void CompleteQuest()
        {
            _questsService.CompleteQuest(_currentQuest);
            _completeButton.gameObject.SetActive(false);
        }

        private void UpdateObjectives()
        {
            foreach (var objectiveView in _objectives)
            {
                objectiveView.UpdateInfo();
            }
            SetButtons();
        }
    }
}
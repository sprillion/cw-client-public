using System;
using System.Collections.Generic;
using System.Linq;

namespace infrastructure.services.quests
{
    public class QuestMarkerService : IQuestMarkerService
    {
        public Objective MarkedObjective { get; private set; }
        public event Action<Objective> OnMarkedObjectiveChanged;

        private readonly IQuestsService _questsService;
        private readonly HashSet<int> _pendingAutoSelect = new HashSet<int>();

        public QuestMarkerService(IQuestsService questsService)
        {
            _questsService = questsService;
            _questsService.OnQuestRemoved += OnQuestRemoved;
            _questsService.OnCurrentQuestAdded += OnQuestAdded;
            _questsService.OnQuestInfoLoaded += OnQuestInfoLoaded;
        }

        public void SetMarker(Objective objective)
        {
            MarkedObjective = objective;
            OnMarkedObjectiveChanged?.Invoke(objective);
        }

        public void ClearMarker()
        {
            MarkedObjective = null;
            OnMarkedObjectiveChanged?.Invoke(null);
        }

        private void OnQuestAdded(Quest quest)
        {
            if (MarkedObjective != null) return;
            if (TryAutoSelect(quest)) return;
            _pendingAutoSelect.Add(quest.Id);
        }

        private void OnQuestInfoLoaded(int questId)
        {
            if (!_pendingAutoSelect.Remove(questId)) return;
            if (MarkedObjective != null) return;
            var quest = _questsService.CurrentQuests.FirstOrDefault(q => q.Id == questId);
            if (quest != null) TryAutoSelect(quest);
        }

        private void OnQuestRemoved(Quest quest)
        {
            _pendingAutoSelect.Remove(quest.Id);
            if (MarkedObjective?.Quest == quest)
                ClearMarker();
        }

        private bool TryAutoSelect(Quest quest)
        {
            if (quest.Objectives == null || quest.Objectives.Count == 0) return false;
            var first = quest.Objectives.FirstOrDefault(o => o.HasPosition);
            if (first == null) return false;
            SetMarker(first);
            return true;
        }
    }
}

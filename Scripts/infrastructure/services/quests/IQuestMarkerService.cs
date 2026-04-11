using System;

namespace infrastructure.services.quests
{
    public interface IQuestMarkerService
    {
        Objective MarkedObjective { get; }
        event Action<Objective> OnMarkedObjectiveChanged;

        void SetMarker(Objective objective);
        void ClearMarker();
    }
}

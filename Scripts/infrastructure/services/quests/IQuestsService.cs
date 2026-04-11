using System;
using System.Collections.Generic;
using infrastructure.services.npc;
using network;

namespace infrastructure.services.quests
{
    public interface IQuestsService : IReceiver
    {
        List<Quest> AvailableQuests { get; }
        List<Quest> CurrentQuests { get; }
        List<Quest> NpcQuests(NpcType npcType);
        event Action<int> OnQuestInfoLoaded;
        event Action OnAvailableQuestsLoaded;
        event Action<Quest> OnShowQuestInfo;
        event Action<Quest> OnCurrentQuestAdded;
        event Action<Quest> OnQuestRemoved;
        event Action OnUpdateObjectives;
        void GetAvailableQuests();
        void GetNpcQuests();
        void GetQuestInfo(int questId);
        void ShowQuestsInfo(Quest quest);
        void AcceptQuest(Quest quest);
        void CompleteQuest(Quest quest);
        void TransferItems(int questId, int objectiveId);
    }
}
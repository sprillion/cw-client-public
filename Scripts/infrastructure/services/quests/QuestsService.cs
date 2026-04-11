using System;
using System.Collections.Generic;
using System.Linq;
using infrastructure.services.npc;
using network;
using tools;

namespace infrastructure.services.quests
{
    public class QuestsService : IQuestsService
    {
        public enum FromClientMessage : byte
        {
            GetAvailableQuests,
            GetNpcQuests,
            GetQuestInfo,
            AcceptQuest,
            CompleteQuest,
            TransferItems,
        }

        public enum FromServerMessage : byte
        {
            AvailableQuests,
            AllCurrentQuests,
            AddCurrentQuest,
            RemoveCurrentQuest,
            NpcQuests,
            QuestInfo,
            UpdateProgress,
        }

        private readonly Dictionary<int, Quest> _quests = new Dictionary<int, Quest>();

        private readonly INetworkManager _networkManager;

        public List<Quest> AvailableQuests => _quests.Values.Where(quest => quest.IsAvailable).ToList();
        public List<Quest> CurrentQuests => _quests.Values.Where(quest => quest.IsCurrent).ToList();
        public List<Quest> NpcQuests(NpcType npcType) => _quests.Values.Where(quest => quest.NpcType == npcType).ToList();

        public event Action<int> OnQuestInfoLoaded;
        public event Action OnAvailableQuestsLoaded;
        public event Action<Quest> OnShowQuestInfo;

        public event Action<Quest> OnCurrentQuestAdded;
        public event Action<Quest> OnQuestRemoved;

        public event Action OnUpdateObjectives;
        
        public QuestsService(INetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.AvailableQuests:
                    SetAvailableQuests(message);
                    break;
                case FromServerMessage.AllCurrentQuests:
                    SetCurrentQuests(message);
                    break;
                case FromServerMessage.AddCurrentQuest:
                    AddCurrentQuest(message);
                    break;
                case FromServerMessage.RemoveCurrentQuest:
                    RemoveCurrentQuest(message);
                    break;
                case FromServerMessage.NpcQuests:
                    SetAvailableQuests(message);
                    //SetCurrentQuests(message);
                    break;
                case FromServerMessage.QuestInfo:
                    SetQuestInfo(message);
                    break;
                case FromServerMessage.UpdateProgress:
                    UpgradeProgress(message);
                    break;
            }
        }

        public void GetAvailableQuests()
        {
            var message = new Message(MessageType.Quests);
            message.AddByte(FromClientMessage.GetAvailableQuests.ToByte());
            
            _networkManager.SendMessage(message);
        }
        
        public void GetNpcQuests()
        {
            var message = new Message(MessageType.Quests);
            message.AddByte(FromClientMessage.GetNpcQuests.ToByte());
            
            _networkManager.SendMessage(message);
        }

        public void GetQuestInfo(int questId)
        {
            if (_quests.TryGetValue(questId, out var quest) && quest.IsLoaded) return;
            
            var message = new Message(MessageType.Quests);
            message.AddByte(FromClientMessage.GetQuestInfo.ToByte())
                .AddInt(questId);

            _networkManager.SendMessage(message);
        }

        public void ShowQuestsInfo(Quest quest)
        {
            OnShowQuestInfo?.Invoke(quest);
        }

        public void AcceptQuest(Quest quest)
        {
            var message = new Message(MessageType.Quests);
            
            message.AddByte(FromClientMessage.AcceptQuest.ToByte())
                .AddInt(quest.Id)
                .AddByte(quest.NpcType.ToByte());
            
            _networkManager.SendMessage(message);
        }

        public void CompleteQuest(Quest quest)
        {
            var message = new Message(MessageType.Quests)
                .AddByte(FromClientMessage.CompleteQuest.ToByte())
                .AddInt(quest.Id);
            
            _networkManager.SendMessage(message);
        }

        public void TransferItems(int questId, int objectiveId)
        {
            var message = new Message(MessageType.Quests)
                .AddByte(FromClientMessage.TransferItems.ToByte())
                .AddInt(questId)
                .AddInt(objectiveId);
            
            _networkManager.SendMessage(message);
        }

        private void SetAvailableQuests(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                AddAvailableQuest(message);
            }
            
            OnAvailableQuestsLoaded?.Invoke();
        }

        private void AddCurrentQuest(Message message)
        {
            var quest = AddQuest(message);
            
            quest.IsCurrent = true;
            quest.IsAvailable = false;

            OnCurrentQuestAdded?.Invoke(quest);
        }

        private void AddAvailableQuest(Message message)
        {
            var quest = AddQuest(message);
            
            quest.IsCurrent = false;
            quest.IsAvailable = true;
        }

        private Quest AddQuest(Message message)
        {
            var questId = message.GetInt();
            var npcType = (NpcType)message.GetByte();
            if (_quests.TryGetValue(questId, out var quest)) return quest;

            quest = new Quest()
            {
                Id = questId,
                NpcType = npcType
            };
                
            _quests.Add(questId, quest);
            return quest;
        }

        private void RemoveCurrentQuest(Message message)
        {
            var questId = message.GetInt();
            var quest = _quests[questId];
            quest.IsCompleted = true;
            _quests.Remove(questId);
            
            OnQuestRemoved?.Invoke(quest);
        }

        private void SetCurrentQuests(Message message)
        {
            var count = message.GetInt();

            for (int i = 0; i < count; i++)
            {
                var questId = message.GetInt();
                var npcType = (NpcType)message.GetByte();
                if (_quests.TryGetValue(questId, out var quest)) continue;

                quest = new Quest()
                {
                    Id = questId,
                    NpcType = npcType,
                    IsAvailable = false,
                    IsCurrent = true
                };
                
                _quests.Add(questId, quest);
            }
        }

        private void SetQuestInfo(Message message)
        {
            var questId = message.GetInt();

            if (!_quests.TryGetValue(questId, out var quest))
            {
                quest = new Quest()
                {
                    Id = questId
                };
            }
            
            quest.NpcType = (NpcType)message.GetByte();
            quest.HasNpcPosition = message.GetBool();
            if (quest.HasNpcPosition)
                quest.NpcPosition = message.GetVector3();
            
            var status = message.GetBool();
            quest.IsAvailable = !status;
            quest.IsCurrent = status;
            
            quest.Objectives.Clear();
            quest.Rewards.Clear();
            
            var objectiveCount = message.GetInt();
            
            for (int i = 0; i < objectiveCount; i++)
            {
                var objective = new Objective()
                {
                    Id = message.GetInt(),
                    ObjectiveType = message.GetByteEnum<ObjectiveType>(),
                    TargetId = message.GetInt(),
                    TargetValue = message.GetInt(),
                    CurrentValue = message.GetInt(),
                    HasPosition = message.GetBool(),
                    Quest = quest
                };
                if (objective.HasPosition)
                    objective.Position = message.GetVector3();
                
                quest.Objectives.Add(objective);
            }

            var rewardCount = message.GetInt();

            for (int i = 0; i < rewardCount; i++)
            {
                var reward = new Reward()
                {
                    RewardType = (RewardType)message.GetByte(),
                    Id = message.GetInt(),
                    Count = message.GetInt()
                };
                quest.Rewards.Add(reward);
            }
            
            quest.IsLoaded = true;
            OnQuestInfoLoaded?.Invoke(questId);
        }

        private void UpgradeProgress(Message message)
        {
            var questId = message.GetInt();
            var objectiveId = message.GetInt();
            var targetValue = message.GetInt();
            var currentValue = message.GetInt();
            
            if (!_quests.TryGetValue(questId, out var quest)) return;
            var objective = quest.Objectives.FirstOrDefault(o => o.Id == objectiveId);
            if (objective == null) return;
            objective.TargetValue = targetValue;
            objective.CurrentValue = currentValue;
            
            OnUpdateObjectives?.Invoke();
        }
    }
}
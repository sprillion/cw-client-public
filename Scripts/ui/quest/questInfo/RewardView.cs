using infrastructure.services.quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ui.quest
{
    public class RewardView : PooledObject
    {
        [SerializeField] protected Image _icon;
        [SerializeField] protected TMP_Text _count;

        private Reward _currentReward;
        public virtual void SetInfo(Reward reward)
        {
            _currentReward = reward;
            _count.text = reward.Count.ToString();
        }
    }
}
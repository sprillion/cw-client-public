using factories.inventory;
using infrastructure.services.quests;
using Zenject;

namespace ui.quest
{
    public class ItemRewardView : RewardView
    {
        private IInventoryFactory _inventoryFactory;

        [Inject]
        public void Construct(IInventoryFactory inventoryFactory)
        {
            _inventoryFactory = inventoryFactory;
        }
        public override void SetInfo(Reward reward)
        {
            _icon.sprite = _inventoryFactory.GetItemData(reward.Id).Icon;
            base.SetInfo(reward);
        }
    }
}
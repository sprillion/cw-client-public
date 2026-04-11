using infrastructure.services.quests;
using ui.tools;

namespace ui.quest
{
    public class ResourceRewardView : RewardView
    {
        private SpriteCatalog _resources;
        
        private void Awake()
        {
            _resources = GameResources.Data.Catalogs.resource_sprites<SpriteCatalog>();
        }

        public override void SetInfo(Reward reward)
        {
            _icon.sprite = _resources.GetSprite((ResourceType)reward.Id);
            base.SetInfo(reward);
        }
    }
}
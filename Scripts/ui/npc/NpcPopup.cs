using infrastructure.services.npc;
using infrastructure.services.ui;
using ui.popup;
using ui.quest;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.npc
{
    public class NpcPopup : Popup
    {
        [SerializeField] private ShopPopup _shopPopup;
        [SerializeField] private QuestsPopup _questsPopup;
        
        [SerializeField] private NpcCard _npcCard;
        
        [SerializeField] private Image _backgroundImage;

        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _barterButton;
        [SerializeField] private Button _questsButton;
        
        [SerializeField] private Button _closeButton;

        private IUiService _uiService;
        private INpcService _npcService;
        
        [Inject]
        public void Construct(IUiService uiService, INpcService npcService)
        {
            _uiService = uiService;
            _npcService = npcService;
            
            _shopButton.onClick.AddListener(GetShopInfo);
            _barterButton.onClick.AddListener(GetBarterInfo);
            _questsButton.onClick.AddListener(GetQuestsInfo);
        }
        
        public override void Initialize()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            _npcService.GetAttitudeNpcInfo();
            _backgroundImage.color = _npcService.CurrentNpcData.BackgroundColor;
            _npcCard.Show();
            
            base.Show();
        }

        public override void Hide()
        {
            _npcCard.Hide();
            base.Hide();
        }

        private void GetShopInfo()
        {
            _shopPopup.Show();
            _uiService.Inventory.Show(this);
        }

        private void GetBarterInfo()
        {
            
        }

        private void GetQuestsInfo()
        {
            _questsPopup.SetNpc(_npcService.CurrentNpcData.NpcType);
            _questsPopup.Show();
            _uiService.QuestsInfoPopup.Show(this);
        }

    }
}
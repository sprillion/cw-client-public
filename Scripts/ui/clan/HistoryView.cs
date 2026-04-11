using infrastructure.services.clan;
using ui.clan.history;
using ui.popup;
using UnityEngine;
using Zenject;

namespace ui.clan
{
    public class HistoryView : Popup
    {
        [SerializeField] private HistoryAdapter _adapter;
        private IClanService _clanService;
        
        [Inject]
        public void Construct(IClanService clanService)
        {
            _clanService = clanService;
            _clanService.OnClanHistoryReceived += _adapter.SetHistory;
        }

        public override void Show()
        {
            _clanService.RequestClanHistory();
            base.Show();
        }
    }
}
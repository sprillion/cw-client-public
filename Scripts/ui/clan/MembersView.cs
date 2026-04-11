using infrastructure.services.clan;
using ui.popup;
using UnityEngine;
using Zenject;

namespace ui.clan
{
    public class MembersView : Popup
    {
        [SerializeField] private MembersAdapter _membersAdapter;

        private IClanService _clanService;
        
        [Inject]
        public void Construct(IClanService clanService)
        {
            _clanService = clanService;
        }

        public override void Show()
        {
            Debug.Log($"Members {_clanService.MyClan.Members.Count}");
            _membersAdapter.SetMembers(_clanService.MyClan.Members);
            base.Show();
        }
    }
}
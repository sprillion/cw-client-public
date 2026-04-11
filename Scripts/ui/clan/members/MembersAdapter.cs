using System;
using System.Collections.Generic;
using infrastructure.services.clan;
using UnityEngine;
using Zenject;

namespace ui.clan
{
    public class MembersAdapter : Adapter
    {
        [SerializeField] private MemberView _memberViewPrefab;
        
        public override event Action OnDataChange;

        private IClanService _clanService;
        private List<ClanMember> _members;

        
        [Inject]
        public void Construct(IClanService clanService)
        {
            _clanService = clanService;
        }
        
        public override int GetItemCount()
        {
            return _members.Count;
        }

        public override GameObject CreateView(int index, Transform parent)
        {
            var memberView = Instantiate(_memberViewPrefab, parent);
            return memberView.gameObject;
        }

        public override void BindView(GameObject view, int index)
        {
            var message = view.GetComponent<MemberView>();
            message.Bind(_members[index], _clanService.MyMember.HasPermission(ClanPermission.DemoteMembers));
        }

        public void SetMembers(List<ClanMember> members)
        {
            _members = members;
            OnDataChange?.Invoke();
        }
    }
}
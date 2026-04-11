using infrastructure.services.clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ui.clan
{
    public class MemberView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _roleText;
        [SerializeField] private Image _onlineIcon;

        [SerializeField] private Button _editButton;
        
        public void Bind(ClanMember clanMember, bool havePermissions)
        {
            _nameText.text = clanMember.CharacterName;
            _roleText.text = clanMember.Role.ToString();
            _onlineIcon.color = clanMember.IsOnline ? Color.green : Color.red;
            
            _editButton.gameObject.SetActive(havePermissions);
        }
    }
}
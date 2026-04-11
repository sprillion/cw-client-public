using infrastructure.services.clan;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;

namespace ui.clan
{
    public class SelectFactionView : Popup
    {
        [SerializeField] private CreateClanView _createClanView;
        
        [SerializeField] private Button _lightButton;
        [SerializeField] private Button _darkButton;
        
        [SerializeField] private Button _backButton;

        public override void Initialize()
        {
            _createClanView.Initialize();
            
            _backButton.onClick.AddListener(Back);
            _lightButton.onClick.AddListener(() => SelectFaction(FactionType.Light));
            _darkButton.onClick.AddListener(() => SelectFaction(FactionType.Dark));
        }

        private void SelectFaction(FactionType factionType)
        {
            _createClanView.Show(this, factionType);
        }
    }
}
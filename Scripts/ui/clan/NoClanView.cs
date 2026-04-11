using infrastructure.services.clan;
using infrastructure.services.players;
using TMPro;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.clan
{
    public class NoClanView : Popup
    {
        [SerializeField] private SelectFactionView _selectFactionView;
        [SerializeField] private GameObject _noHaveLevel;
        [SerializeField] private TMP_Text _noHaveLevelText;
        [SerializeField] private Button _createButton;

        private ICharacterService _characterService;
        private IClanService _clanService;
        
        [Inject]
        public void Construct(ICharacterService characterService, IClanService clanService)
        {
            _characterService = characterService;
            _clanService = clanService;
        }

        public override void Initialize()
        {
            _selectFactionView.Initialize();
            _createButton.onClick.AddListener(() => _selectFactionView.Show(this));
        }

        public override void Show()
        {
            _noHaveLevelText.text =
                string.Format("Clan/ToCreateReachLevel".Loc(), _clanService.ClansSettings.MinLevelToCreate);
            var canCreateClan = _characterService.CurrentCharacter.CharacterStats.Level >= _clanService.ClansSettings.MinLevelToCreate;
            _createButton.gameObject.SetActive(canCreateClan);
            _noHaveLevel.SetActive(!canCreateClan);
            
            base.Show();
        }
    }
}
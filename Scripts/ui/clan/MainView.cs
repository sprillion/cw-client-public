using System;
using System.Linq;
using infrastructure.services.clan;
using TMPro;
using ui.popup;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.clan
{
    public class MainView : Popup
    {
        [SerializeField] private TMP_Text _clanNameText;
        [SerializeField] private TMP_Text _shortClanNameText;
        [SerializeField] private TMP_Text _factionText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _membersText;
        [SerializeField] private TMP_Text _ownerText;
        [SerializeField] private TMP_Text _typeText;

        [SerializeField] private Image _clanIcon;
        [SerializeField] private Image _factionIcon;

        [SerializeField] private Transform _buffsParent;

        private IClanService _clanService;

        private SpriteCatalog _factionIcons;
        private ColorCatalog _factionColors;
        private IdToSpriteCatalog _lightIcons;
        private IdToSpriteCatalog _darkIcons;
        
        [Inject]
        public void Construct(IClanService clanService)
        {
            _clanService = clanService;
            
            _factionIcons = GameResources.Data.Catalogs.Clan.faction_catalog<SpriteCatalog>();
            _factionColors = GameResources.Data.Catalogs.Clan.faction_color_catalog<ColorCatalog>();
            _lightIcons = GameResources.Data.Catalogs.Clan.light_icons<IdToSpriteCatalog>();
            _darkIcons = GameResources.Data.Catalogs.Clan.dark_icons<IdToSpriteCatalog>();
        }
        
        public override void Show()
        {
            _clanNameText.text = _clanService.MyClan.ClanName;
            _shortClanNameText.text = $"[{_clanService.MyClan.ShortName}]";
            SetFaction();
            _levelText.text = string.Format("Clan/Level".Loc(), _clanService.MyClan.Level);
            _membersText.text = string.Format("Clan/NumberOfMembers".Loc(), _clanService.MyClan.Members.Count, _clanService.MyClan.MaxMembers);
            _ownerText.text = string.Format("Clan/Owner".Loc(), _clanService.MyClan.Members.First(m => m.CharacterId == _clanService.MyClan.OwnerId).CharacterName);

            _clanIcon.sprite = _clanService.MyClan.FactionType switch
            {
                FactionType.Light => _lightIcons.GetSprite(_clanService.MyClan.IconId),
                FactionType.Dark => _darkIcons.GetSprite(_clanService.MyClan.IconId),
                _ => _lightIcons.DefaultSprite
            };
            _factionIcon.sprite = _factionIcons.GetSprite(_clanService.MyClan.FactionType);
            base.Show();
        }

        private void SetFaction()
        {
            var faction = _clanService.MyClan.FactionType switch
            {
                FactionType.Light => "Clan/FactionType/Light",
                FactionType.Dark => "Clan/FactionType/Dark",
                _ => ""
            };

            _factionText.text = $"{"Clan/Faction".Loc()}: {faction.Loc().Bold().SetColor(_factionColors.GetColor(_clanService.MyClan.FactionType))}";
        }
    }
}
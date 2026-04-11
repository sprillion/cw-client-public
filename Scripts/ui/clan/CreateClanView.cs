using System;
using System.Collections.Generic;
using infrastructure.factories;
using infrastructure.services.clan;
using TMPro;
using ui.popup;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.clan
{
    public class CreateClanView : Popup
    {
        [SerializeField] private TMP_InputField _clanNameInput;
        [SerializeField] private TMP_InputField _shortClanNameInput;
        [SerializeField] private Transform _iconsParent;
        [SerializeField] private TMP_Text _creationPriceText;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _backButton;

        [SerializeField] private LoadingPanel _loadingPanel;

        private readonly List<ClanIcon> _clanIcons = new List<ClanIcon>();
        
        private IdToSpriteCatalog _lightIcons;
        private IdToSpriteCatalog _darkIcons;
        
        private IClanService _clanService;
        private FactionType _currentFactionType;

        private ClanIcon _selectedClanIcon;
        
        [Inject]
        public void Construct(IClanService clanService)
        {
            _clanService = clanService;
            _clanService.OnClanCreated += OnClanCreated;

            _lightIcons = GameResources.Data.Catalogs.Clan.light_icons<IdToSpriteCatalog>();
            _darkIcons = GameResources.Data.Catalogs.Clan.dark_icons<IdToSpriteCatalog>();
        }

        public override void Initialize()
        {
            _createButton.onClick.AddListener(CreateClan);
            _backButton.onClick.AddListener(Back);

            ClanIcon.OnSelected += OnSelectClanIcon;
        }

        private void OnDestroy()
        {
            ClanIcon.OnSelected -= OnSelectClanIcon;
        }

        public override void Show(Popup backPopup, params object[] args)
        {
            _currentFactionType = (FactionType)args[0];
            
            FillIcons();
            
            base.Show(backPopup, args);
        }

        private void CreateClan()
        {
            _clanService.CreateClan(_clanNameInput.text, _shortClanNameInput.text, _currentFactionType, _selectedClanIcon.Id);
            _loadingPanel.Show();
        }

        private void OnClanCreated(bool success, int clanId, ClanData clanData)
        {
            if (success)
            {
                Hide();
            }
            
            _loadingPanel.Hide();
        }

        private void FillIcons()
        {
            foreach (var clanIcon in _clanIcons)
            {
                clanIcon.Release();
            }
            
            _clanIcons.Clear();
            
            var iconsDictionary = _currentFactionType switch
            {
                FactionType.Light => _lightIcons.Sprites,
                FactionType.Dark => _darkIcons.Sprites,
                _ => null
            };
            
            if (iconsDictionary == null) return;
            
            foreach (var pair in iconsDictionary)
            {
                var icon = Pool.Get<ClanIcon>();
                icon.SetParentPreserveScale(_iconsParent);
                icon.SetIcon(pair.Value, pair.Key);
                _clanIcons.Add(icon);
            }
            
            _clanIcons[0].Select();
        }

        private void OnSelectClanIcon(ClanIcon clanIcon)
        {
            _selectedClanIcon?.Unselect();
            _selectedClanIcon = clanIcon;
        }
    }
}
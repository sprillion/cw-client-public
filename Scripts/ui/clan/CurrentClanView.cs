using System;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;

namespace ui.clan
{
    public class CurrentClanView : Popup
    {
        [SerializeField] private Button _mainButton;
        [SerializeField] private Button _membersButton;
        [SerializeField] private Button _craftsButton;
        [SerializeField] private Button _historyButton;
        [SerializeField] private Button _settingsButton;
        
        [SerializeField] private MainView _mainView;
        [SerializeField] private MembersView _membersView;
        [SerializeField] private CraftsView _craftsView;
        [SerializeField] private HistoryView _historyView;
        [SerializeField] private SettingsView _settingsView;

        private Button _currentButton;
        private Popup _currentView;
        
        private enum ViewType
        {
            Main,
            Members,
            Crafts,
            History,
            Settings,
        }
        
        public override void Initialize()
        {
            _mainView.Initialize();
            _membersView.Initialize();
            _craftsView.Initialize();
            _historyView.Initialize();
            _settingsView.Initialize();
            
            _mainButton.onClick.AddListener(() => ShowView(ViewType.Main));
            _membersButton.onClick.AddListener(() => ShowView(ViewType.Members));
            _craftsButton.onClick.AddListener(() => ShowView(ViewType.Crafts));
            _historyButton.onClick.AddListener(() => ShowView(ViewType.History));
            _settingsButton.onClick.AddListener(() => ShowView(ViewType.Settings));
        }

        public override void Show()
        {
            ShowView(ViewType.Main);
            base.Show();
        }

        public override void Hide()
        {
            _currentView?.Hide();
            base.Hide();
        }

        private void ShowView(ViewType viewType)
        {
            if (_currentButton)
            {
                _currentButton.interactable = true;
            }
            
            _currentButton = viewType switch
            {
                ViewType.Main => _mainButton,
                ViewType.Members => _membersButton,
                ViewType.Crafts => _craftsButton,
                ViewType.History => _historyButton,
                ViewType.Settings => _settingsButton,
                _ => null
            };
            
            if (_currentButton)
            {
                _currentButton.interactable = false;
            }
            
            _currentView?.Hide();
            
            _currentView = viewType switch
            {
                ViewType.Main => _mainView,
                ViewType.Members => _membersView,
                ViewType.Crafts => _craftsView,
                ViewType.History => _historyView,
                ViewType.Settings => _settingsView,
                _ => null
            };

            _currentView?.Show();
        }
        
    }
}
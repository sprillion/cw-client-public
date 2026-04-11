using ui.popup;
using UnityEngine;
using UnityEngine.UI;

namespace ui.settings
{
    public class SettingsPopup : Popup
    {
        [SerializeField] private Button _soundButton;
        [SerializeField] private Button _graphicsButton;
        [SerializeField] private Button _controlsButton;
        [SerializeField] private Button _languageButton;
        [SerializeField] private Button _accountButton;

        [SerializeField] private SoundSettingsView _soundSettingsView;
        [SerializeField] private GraphicsSettingsView _graphicsSettingsView;
        [SerializeField] private ControlsSettingsView _controlsSettingsView;
        [SerializeField] private LanguageSettingsView _languageSettingsView;
        [SerializeField] private AccountSettingsView _accountSettingsView;

        private Popup _currentView;
        private Button _currentButton;

        public override void Initialize()
        {
            _soundSettingsView.Initialize();
            _graphicsSettingsView.Initialize();
            _controlsSettingsView.Initialize();
            _languageSettingsView.Initialize();
            _accountSettingsView.Initialize();

            _soundButton.onClick.AddListener(() => ShowSection(_soundSettingsView, _soundButton));
            _graphicsButton.onClick.AddListener(() => ShowSection(_graphicsSettingsView, _graphicsButton));
            _controlsButton.onClick.AddListener(() => ShowSection(_controlsSettingsView, _controlsButton));
            _languageButton.onClick.AddListener(() => ShowSection(_languageSettingsView, _languageButton));
            _accountButton.onClick.AddListener(() => ShowSection(_accountSettingsView, _accountButton));
        }

        public override void Show()
        {
            base.Show();
            ShowSection(_graphicsSettingsView, _graphicsButton);
        }

        public override void Hide()
        {
            base.Hide();
            _soundSettingsView.Hide();
            _graphicsSettingsView.Hide();
            _controlsSettingsView.Hide();
            _languageSettingsView.Hide();
            _accountSettingsView.Hide();
        }

        private void ShowSection(Popup view, Button button)
        {
            _currentView?.Hide();

            if (_currentButton)
                _currentButton.interactable = true;

            _currentView = view;
            _currentButton = button;

            _currentView.Show();

            if (_currentButton)
                _currentButton.interactable = false;
        }
    }
}

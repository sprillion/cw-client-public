using I2.Loc;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.settings
{
    public class LanguageSettingsView : Popup
    {
        [SerializeField] private Transform _buttonsContainer;
        [SerializeField] private Button _languageButtonPrefab;

        private Button _currentButton;

        public override void Initialize()
        {
            var languages = LocalizationManager.GetAllLanguages();
            foreach (var language in languages)
            {
                var btn = Instantiate(_languageButtonPrefab, _buttonsContainer);
                btn.GetComponentInChildren<TMPro.TMP_Text>().text = language;
                var captured = language;
                btn.onClick.AddListener(() => SelectLanguage(captured, btn));

                if (captured == LocalizationManager.CurrentLanguage)
                {
                    _currentButton = btn;
                    _currentButton.interactable = false;
                }
            }
        }

        private void SelectLanguage(string language, Button btn)
        {
            if (_currentButton)
                _currentButton.interactable = true;

            LocalizationManager.CurrentLanguage = language;

            _currentButton = btn;
            _currentButton.interactable = false;
        }
    }
}

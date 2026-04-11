using infrastructure.services.settings;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.settings
{
    public class GraphicsSettingsView : Popup
    {
        [SerializeField] private Button[] _presetButtons;
        [SerializeField] private Slider _renderDistanceSlider;
        [SerializeField] private Toggle _shadowsToggle;
        [SerializeField] private Button[] _shadowQualityButtons;  // 3 кнопки: Low / Medium / High

        private ISettingsService _settingsService;
        private Button _currentShadowButton;

        [Inject]
        public void Construct(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public override void Initialize()
        {
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                int preset = i;
                _presetButtons[i].onClick.AddListener(() => _settingsService.SetGraphicsPreset((GraphicsPreset)preset));
            }

            _renderDistanceSlider.wholeNumbers = true;
            _renderDistanceSlider.minValue = 3;
            _renderDistanceSlider.maxValue = 7;
            _renderDistanceSlider.onValueChanged.AddListener(v => _settingsService.SetRenderDistance((int)v));

            _shadowsToggle.onValueChanged.AddListener(SetShadowsEnabled);

            for (int i = 0; i < _shadowQualityButtons.Length; i++)
            {
                int quality = i;
                _shadowQualityButtons[i].onClick.AddListener(() => SelectShadowQuality(quality));
            }
        }

        public override void Show()
        {
            base.Show();
            var current = _settingsService.Current;
            _renderDistanceSlider.SetValueWithoutNotify(current.renderDistance);
            _shadowsToggle.SetIsOnWithoutNotify(current.shadowsEnabled);
            SetShadowsButtons(current.shadowsEnabled);
            SetShadowQualityButton(current.shadowQuality);
        }

        private void SelectShadowQuality(int quality)
        {
            _settingsService.SetShadowQuality(quality);
            SetShadowQualityButton(quality);
        }

        private void SetShadowQualityButton(int quality)
        {
            if (_currentShadowButton != null)
                _currentShadowButton.interactable = true;

            if (quality >= 0 && quality < _shadowQualityButtons.Length)
            {
                _currentShadowButton = _shadowQualityButtons[quality];
                _currentShadowButton.interactable = false;
            }
        }

        private void SetShadowsEnabled(bool value)
        {
            _settingsService.SetShadowsEnabled(value);
            SetShadowsButtons(value);
        }

        private void SetShadowsButtons(bool value)
        {
            // Если тени выключены — все кнопки качества недоступны.
            // Если включены — все доступны, кроме текущей активной.
            foreach (var btn in _shadowQualityButtons)
                btn.interactable = value;

            if (value)
                SetShadowQualityButton(_settingsService.Current.shadowQuality);
        }
    }
}

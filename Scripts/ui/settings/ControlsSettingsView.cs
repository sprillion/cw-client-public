using infrastructure.services.settings;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.settings
{
    public class ControlsSettingsView : Popup
    {
        [SerializeField] private Slider _sensitivityXSlider;
        [SerializeField] private Slider _sensitivityYSlider;

        private ISettingsService _settingsService;

        [Inject]
        public void Construct(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public override void Initialize()
        {
            _sensitivityXSlider.minValue = 0.2f;
            _sensitivityXSlider.maxValue = 3f;
            _sensitivityXSlider.wholeNumbers = false;
            _sensitivityXSlider.onValueChanged.AddListener(v => _settingsService.SetSensitivityX(v));

            _sensitivityYSlider.minValue = 0.2f;
            _sensitivityYSlider.maxValue = 3f;
            _sensitivityYSlider.wholeNumbers = false;
            _sensitivityYSlider.onValueChanged.AddListener(v => _settingsService.SetSensitivityY(v));
        }

        public override void Show()
        {
            base.Show();
            _sensitivityXSlider.SetValueWithoutNotify(_settingsService.Current.sensitivityX);
            _sensitivityYSlider.SetValueWithoutNotify(_settingsService.Current.sensitivityY);
        }
    }
}

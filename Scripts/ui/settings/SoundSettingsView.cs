using infrastructure.services.settings;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.settings
{
    public class SoundSettingsView : Popup
    {
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _musicSlider;

        private ISettingsService _settingsService;

        [Inject]
        public void Construct(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public override void Show()
        {
            base.Show();
            _sfxSlider.SetValueWithoutNotify(_settingsService.Current.sfxVolume);
            _musicSlider.SetValueWithoutNotify(_settingsService.Current.musicVolume);
        }

        public override void Initialize()
        {
            _sfxSlider.onValueChanged.AddListener(v => _settingsService.SetSfxVolume(v));
            _musicSlider.onValueChanged.AddListener(v => _settingsService.SetMusicVolume(v));
        }
    }
}

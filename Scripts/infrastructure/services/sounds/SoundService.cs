using System.Collections.Generic;
using System.Linq;
using infrastructure.factories;
using infrastructure.services.settings;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace infrastructure.services.sounds
{
    public class SoundService : MonoBehaviour, ISoundService
    {
        [SerializeField] private AudioMixer _mixer;

        private const string SfxParam = "SFXVolume";
        private const string MusicParam = "MusicVolume";

        private Dictionary<SoundType, SoundData> _soundDatas;
        private Audio _musicAudio;

        private ISettingsService _settingsService;

        [Inject]
        public void Construct(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private void Start()
        {
            _soundDatas = Resources.LoadAll<SoundData>("Sounds/").ToDictionary(s => s.SoundType);

            ApplySfxVolume(_settingsService.Current.sfxVolume);
            ApplyMusicVolume(_settingsService.Current.musicVolume);

            _settingsService.OnSfxVolumeChanged += ApplySfxVolume;
            _settingsService.OnMusicVolumeChanged += ApplyMusicVolume;
        }

        private void OnDestroy()
        {
            if (_settingsService == null) return;
            _settingsService.OnSfxVolumeChanged -= ApplySfxVolume;
            _settingsService.OnMusicVolumeChanged -= ApplyMusicVolume;
        }

        public Audio Play(SoundType soundType)
        {
            if (soundType == SoundType.None) return null;
            if (!_soundDatas.TryGetValue(soundType, out var data)) return null;

            var audio = Pool.Get<Audio>();
            audio.Play(data);
            return audio;
        }

        public Audio Play3D(SoundType soundType, Vector3 position)
        {
            if (soundType == SoundType.None) return null;
            if (!_soundDatas.TryGetValue(soundType, out var data)) return null;

            var audio = Pool.Get<Audio>();
            audio.Play(data, position);
            return audio;
        }

        public void PlayMusic(SoundType soundType)
        {
            StopMusic(0.3f);
            _musicAudio = Play(soundType);
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            if (_musicAudio == null) return;
            _musicAudio.Stop(fadeDuration);
            _musicAudio = null;
        }

        private void ApplySfxVolume(float value)
        {
            _mixer.SetFloat(SfxParam, ToDecibel(value));
        }

        private void ApplyMusicVolume(float value)
        {
            _mixer.SetFloat(MusicParam, ToDecibel(value));
        }

        private static float ToDecibel(float value)
        {
            return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        }
    }
}

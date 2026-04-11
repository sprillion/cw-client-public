using System;

namespace infrastructure.services.settings
{
    public interface ISettingsService
    {
        SettingsData Current { get; }
        event Action<int> OnRenderDistanceChanged;
        event Action<float, float> OnSensitivityChanged;
        event Action<float> OnSfxVolumeChanged;
        event Action<float> OnMusicVolumeChanged;
        void SetSfxVolume(float v);
        void SetMusicVolume(float v);
        void SetGraphicsPreset(GraphicsPreset p);
        void SetRenderDistance(int d);
        void SetShadowsEnabled(bool e);
        void SetShadowQuality(int q);   // 0/1/2
        void SetSensitivityX(float v);
        void SetSensitivityY(float v);
        void Save();
    }
}

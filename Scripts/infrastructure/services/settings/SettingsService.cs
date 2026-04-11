using infrastructure.services.saveLoad;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace infrastructure.services.settings
{
    public class SettingsService : ISettingsService
    {
        // shadowQuality: 0=Low, 1=Medium, 2=High
        private static readonly float[] ShadowDistances   = { 30f,  50f,  80f  };
        private static readonly int[]   ShadowCascades    = { 1,    2,    4    };
        private static readonly int[]   ShadowResolutions = { 128,  512, 1024 };

        public SettingsData Current { get; private set; }
        public event System.Action<int> OnRenderDistanceChanged;
        public event System.Action<float, float> OnSensitivityChanged;
        public event System.Action<float> OnSfxVolumeChanged;
        public event System.Action<float> OnMusicVolumeChanged;

        private readonly ISaveLoadService _saveLoadService;

        public SettingsService(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
            Load();
            // ApplyPerformanceDefaults();
            ApplyAll();
            ApplyFrameRate();
        }

        public void SetSfxVolume(float v)
        {
            Current.sfxVolume = v;
            OnSfxVolumeChanged?.Invoke(v);
            Save();
        }

        public void SetMusicVolume(float v)
        {
            Current.musicVolume = v;
            OnMusicVolumeChanged?.Invoke(v);
            Save();
        }

        public void SetGraphicsPreset(GraphicsPreset p)
        {
            Current.graphicsPreset = (int)p;
            ApplyGraphics(p);
            Save();
        }

        public void SetRenderDistance(int d)
        {
            Current.renderDistance = d;
            OnRenderDistanceChanged?.Invoke(d);
            Save();
        }

        public void SetShadowsEnabled(bool e)
        {
            Current.shadowsEnabled = e;
            ApplyShadows();
            Save();
        }

        public void SetShadowQuality(int q)
        {
            Current.shadowQuality = q;
            ApplyShadows();
            Save();
        }

        public void SetSensitivityX(float v)
        {
            Current.sensitivityX = v;
            OnSensitivityChanged?.Invoke(Current.sensitivityX, Current.sensitivityY);
            Save();
        }

        public void SetSensitivityY(float v)
        {
            Current.sensitivityY = v;
            OnSensitivityChanged?.Invoke(Current.sensitivityX, Current.sensitivityY);
            Save();
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(Current);
            _saveLoadService.SetJson("Settings", json);
        }

        private void Load()
        {
            if (_saveLoadService.HasJson("Settings"))
            {
                var json = _saveLoadService.GetJson("Settings");
                Current = JsonUtility.FromJson<SettingsData>(json) ?? new SettingsData();
            }
            else
            {
                Current = new SettingsData();
            }
        }

        private void ApplyAll()
        {
            ApplyGraphics((GraphicsPreset)Current.graphicsPreset);
            ApplyShadows();
            OnRenderDistanceChanged?.Invoke(Current.renderDistance);
        }

        private void ApplyGraphics(GraphicsPreset p)
        {
            var urp = Resources.Load<UniversalRenderPipelineAsset>($"Settings/URP-{p}");
            if (urp != null)
                GraphicsSettings.defaultRenderPipeline = urp;
        }

        private void ApplyShadows()
        {
            var urpAsset = (QualitySettings.renderPipeline ?? GraphicsSettings.defaultRenderPipeline)
                           as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogWarning("[Settings] ApplyShadows: URP asset not found");
                return;
            }

            if (!Current.shadowsEnabled)
            {
                urpAsset.shadowDistance = 0f;
                Debug.Log("[Settings] Shadows disabled");
                return;
            }

            int q = Current.shadowQuality;
            urpAsset.shadowDistance              = ShadowDistances[q];
            urpAsset.shadowCascadeCount          = ShadowCascades[q];
            urpAsset.mainLightShadowmapResolution = ShadowResolutions[q];

            // Мягкость теней управляется через основной направленный свет
            var sun = RenderSettings.sun;
            if (sun != null)
                sun.shadows = q > 0 ? LightShadows.Soft : LightShadows.Hard;

            Debug.Log($"[Settings] Shadow quality={q}, distance={urpAsset.shadowDistance}, cascades={urpAsset.shadowCascadeCount}, resolution={urpAsset.mainLightShadowmapResolution}");
        }

        // Однократно применяет настройки производительности, которые не нужны
        // в пользовательских пресетах, но дают прирост на всех платформах.
        private void ApplyPerformanceDefaults()
        {
            // Текстуры — анизотропия включена глобально движком, не принудительно
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;

            // Без сглаживания по умолчанию — дорого на мобильных/WebGL
            QualitySettings.antiAliasing = 0;

            // Soft particles требуют depth texture — отключаем
            QualitySettings.softParticles = false;

            // Reflection probes в реалтайме не нужны для воксельной игры
            QualitySettings.realtimeReflectionProbes = false;

            // Два веса костей достаточно для персонажей в этой игре
            QualitySettings.skinWeights = SkinWeights.TwoBones;

            // Физика: 6 итераций вместо 10 — достаточно для ECM2
            Physics.defaultSolverIterations = 6;
            Physics.defaultSolverVelocityIterations = 1;
        }

        // На мобильных фиксируем 60 fps. На WebGL не трогаем — браузер управляет сам.
        private void ApplyFrameRate()
        {
#if UNITY_EDITOR
            Application.targetFrameRate = -1;
            return;
#endif
            
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0; // vSync несовместим с targetFrameRate
#elif UNITY_WEBGL
            Application.targetFrameRate = -1;
#endif
        }
    }
}

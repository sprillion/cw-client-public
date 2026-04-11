using infrastructure.services.platform.core;
using infrastructure.services.platform.core.ads;
using infrastructure.services.platform.core.analytics;
using infrastructure.services.platform.core.auth;
using infrastructure.services.platform.core.cloudsave;
using infrastructure.services.platform.core.language;
using infrastructure.services.platform.core.leaderboard;
using infrastructure.services.platform.core.payment;
using infrastructure.services.platform.stubs;
#if YANDEX_GAMES
using infrastructure.services.platform.yandex;
#endif
#if VK_PLAY
using infrastructure.services.platform.vk;
#endif
#if RU_STORE
using infrastructure.services.platform.rustore;
#endif
#if GOOGLE_PLAY
using infrastructure.services.platform.google;
#endif
using UnityEngine;
using Zenject;

namespace infrastructure.services.platform
{
    public class PlatformInstaller : MonoInstaller
    {
        [SerializeField] private PlatformConfig _config;

        public override void InstallBindings()
        {
            var settings = _config.GetCurrent();

            Container
                .Bind<PlatformSettings>()
                .FromInstance(settings)
                .AsSingle();

            Container
                .Bind<PlatformConfig>()
                .FromInstance(_config)
                .AsSingle();

            BindProviders();

            Container
                .Bind<IPlatformService>()
                .To<PlatformService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<PlatformLanguageInitializer>().AsSingle().NonLazy();
        }

        private void BindProviders()
        {
#if UNITY_EDITOR
            BindEditorProviders();
#elif YANDEX_GAMES
            BindYandexProviders();
#elif VK_PLAY
            BindVKProviders();
#elif GOOGLE_PLAY
            BindGooglePlayProviders();
#elif RU_STORE
            BindRuStoreProviders();
#else
            Debug.LogError("[PlatformInstaller] No platform define found! Using stubs.");
            BindEditorProviders();
#endif
        }

        private void BindEditorProviders()
        {
            Container.Bind<IPlatformAuthProvider>().To<EditorAuthProvider>().AsSingle();
            Container.Bind<IPaymentService>().To<EditorPaymentProvider>().AsSingle();
            Container.Bind<IAdService>().To<EditorAdProvider>().AsSingle();
            Container.Bind<ILeaderboardService>().To<EditorLeaderboardProvider>().AsSingle();
            Container.Bind<IAnalyticsService>().To<EditorAnalyticsProvider>().AsSingle();
            Container.Bind<ILanguageService>().To<EditorLanguageProvider>().AsSingle();
            Container.Bind<ICloudSaveService>().To<EditorCloudSaveService>().AsSingle();
        }

        private void BindYandexProviders()
        {
#if YANDEX_GAMES
            Container.Bind<IPlatformAuthProvider>().To<YandexAuthProvider>().AsSingle();
            Container.Bind<IPaymentService>().To<YandexPaymentProvider>().AsSingle();
            Container.Bind<IAdService>().To<YandexAdProvider>().AsSingle();
            Container.Bind<ILeaderboardService>().To<YandexLeaderboardProvider>().AsSingle();
            Container.Bind<IAnalyticsService>().To<YandexAnalyticsProvider>().AsSingle();
            Container.Bind<ILanguageService>().To<YandexLanguageProvider>().AsSingle();
            Container.Bind<ICloudSaveService>().To<YandexCloudSaveProvider>().AsSingle();
#endif
        }

        private void BindVKProviders()
        {
#if VK_PLAY
            Container.Bind<IPlatformAuthProvider>()
                .To<VKAuthProvider>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("VKAuthProvider")
                .AsSingle();

            Container.Bind<IAdService>()
                .To<VKAdProvider>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("VKAdProvider")
                .AsSingle();

            Container.Bind<IPaymentService>()
                .To<VKPaymentProvider>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("VKPaymentProvider")
                .AsSingle();

            Container.Bind<ILeaderboardService>().To<VKLeaderboardProvider>().AsSingle();
            Container.Bind<IAnalyticsService>().To<VKAnalyticsProvider>().AsSingle();
            Container.Bind<ILanguageService>().To<VKLanguageProvider>().AsSingle();
            Container.Bind<ICloudSaveService>().To<EditorCloudSaveService>().AsSingle();
#endif
        }

        private void BindGooglePlayProviders()
        {
#if GOOGLE_PLAY
            Container.Bind<IPlatformAuthProvider>().To<GooglePlayAuthProvider>().AsSingle();
            // Внутренние провайдеры биндятся как конкретные типы — роутер получает их через DI
            Container.Bind<RobokassaPaymentProvider>().AsSingle();
            Container.Bind<GooglePlayBillingProvider>().AsSingle();
            Container.Bind<IPaymentService>().To<GooglePlayPaymentRouter>().AsSingle();
            Container.Bind<IAdService>().To<GooglePlayAdProvider>().AsSingle();
            Container.Bind<ILeaderboardService>().To<GooglePlayLeaderboardProvider>().AsSingle();
            Container.Bind<IAnalyticsService>().To<GooglePlayAnalyticsProvider>().AsSingle();
            Container.Bind<ILanguageService>().To<GooglePlayLanguageProvider>().AsSingle();
            Container.Bind<ICloudSaveService>().To<EditorCloudSaveService>().AsSingle();
#endif
        }

        private void BindRuStoreProviders()
        {
#if RU_STORE
            Container.Bind<IPlatformAuthProvider>().To<RuStoreAuthProvider>().AsSingle();
            Container.Bind<IPaymentService>().To<RobokassaPaymentProvider>().AsSingle();
            Container.Bind<IAdService>().To<RuStoreAdProvider>().AsSingle();
            Container.Bind<ILeaderboardService>().To<RuStoreLeaderboardProvider>().AsSingle();
            Container.Bind<IAnalyticsService>().To<RuStoreAnalyticsProvider>().AsSingle();
            Container.Bind<ILanguageService>().To<RuStoreLanguageProvider>().AsSingle();
            Container.Bind<ICloudSaveService>().To<EditorCloudSaveService>().AsSingle();
#endif
        }
    }
}

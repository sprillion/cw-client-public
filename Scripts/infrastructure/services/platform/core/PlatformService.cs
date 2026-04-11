using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.ads;
using infrastructure.services.platform.core.analytics;
using infrastructure.services.platform.core.auth;
using infrastructure.services.platform.core.cloudsave;
using infrastructure.services.platform.core.language;
using infrastructure.services.platform.core.leaderboard;
using infrastructure.services.platform.core.payment;
using UnityEngine;

namespace infrastructure.services.platform.core
{
    public class PlatformService : IPlatformService
    {
        public Platform CurrentPlatform => Settings.platform;
        public PlatformSettings Settings { get; }

        public IPlatformAuthProvider Auth { get; }
        public IPaymentService Payment { get; }
        public IAdService Ads { get; }
        public ILeaderboardService Leaderboard { get; }
        public IAnalyticsService Analytics { get; }
        public ILanguageService Language { get; }
        public ICloudSaveService CloudSave { get; }

        public PlatformService(
            PlatformSettings settings,
            IPlatformAuthProvider auth,
            IPaymentService payment,
            IAdService ads,
            ILeaderboardService leaderboard,
            IAnalyticsService analytics,
            ILanguageService language,
            ICloudSaveService cloudSave)
        {
            Settings = settings;
            Auth = auth;
            Payment = payment;
            Ads = ads;
            Leaderboard = leaderboard;
            Analytics = analytics;
            Language = language;
            CloudSave = cloudSave;
        }

        public async UniTask Initialize()
        {
            Debug.Log($"[PlatformService] Initializing platform: {CurrentPlatform}");
            await Payment.Initialize();
            await Ads.Initialize();
#if YANDEX_GAMES
            YandexGames.YandexGamesSDK.GameReady();
#endif
        }
    }
}

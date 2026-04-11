using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.ads;
using infrastructure.services.platform.core.analytics;
using infrastructure.services.platform.core.auth;
using infrastructure.services.platform.core.cloudsave;
using infrastructure.services.platform.core.language;
using infrastructure.services.platform.core.leaderboard;
using infrastructure.services.platform.core.payment;

namespace infrastructure.services.platform.core
{
    public interface IPlatformService
    {
        Platform CurrentPlatform { get; }
        PlatformSettings Settings { get; }

        IPlatformAuthProvider Auth { get; }
        IPaymentService Payment { get; }
        IAdService Ads { get; }
        ILeaderboardService Leaderboard { get; }
        IAnalyticsService Analytics { get; }
        ILanguageService Language { get; }
        ICloudSaveService CloudSave { get; }

        UniTask Initialize();
    }
}

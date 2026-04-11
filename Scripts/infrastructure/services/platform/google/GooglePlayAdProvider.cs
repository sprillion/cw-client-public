#if GOOGLE_PLAY
using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.ads;

namespace infrastructure.services.platform.google
{
    /// <summary>
    /// Заглушка рекламы для Google Play. Mediation SDK (AppLovin MAX / IronSource) — отдельный этап.
    /// </summary>
    public class GooglePlayAdProvider : IAdService
    {
        public event Action OnRewardedComplete;
        public event Action OnRewardedFailed;
        public event Action OnInterstitialClosed;

        public bool IsRewardedReady     => false;
        public bool IsInterstitialReady => false;

        public UniTask Initialize() => UniTask.CompletedTask;

        public void ShowRewarded()     => OnRewardedFailed?.Invoke();
        public void ShowInterstitial() { }
    }
}
#endif

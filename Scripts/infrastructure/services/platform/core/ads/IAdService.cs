using System;
using Cysharp.Threading.Tasks;

namespace infrastructure.services.platform.core.ads
{
    public interface IAdService
    {
        event Action OnRewardedComplete;
        event Action OnRewardedFailed;
        event Action OnInterstitialClosed;

        bool IsRewardedReady { get; }
        bool IsInterstitialReady { get; }

        UniTask Initialize();
        void ShowRewarded();
        void ShowInterstitial();
    }
}

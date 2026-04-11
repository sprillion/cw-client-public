#if YANDEX_GAMES
using System;
using YandexGames;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.ads;

namespace infrastructure.services.platform.yandex
{
    public class YandexAdProvider : IAdService
    {
        public event Action OnRewardedComplete;
        public event Action OnRewardedFailed;
        public event Action OnInterstitialClosed;

        public bool IsRewardedReady => YandexGamesSDK.IsInitialized;
        public bool IsInterstitialReady => YandexGamesSDK.IsInitialized;

        public UniTask Initialize() => UniTask.WaitUntil(() => YandexGamesSDK.IsInitialized);

        public void ShowRewarded()
        {
            VideoAd.Show(
                onOpenCallback: null,
                onRewardedCallback: () => OnRewardedComplete?.Invoke(),
                onCloseCallback: null,
                onErrorCallback: _ => OnRewardedFailed?.Invoke());
        }

        public void ShowInterstitial()
        {
            InterstitialAd.Show(
                onOpenCallback: null,
                onCloseCallback: _ => OnInterstitialClosed?.Invoke(),
                onErrorCallback: null,
                onOfflineCallback: null);
        }
    }
}
#endif

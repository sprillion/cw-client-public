#if VK_PLAY
using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.ads;
using UnityEngine;

namespace infrastructure.services.platform.vk
{
    // MonoBehaviour — нужен для приёма SendMessage-колбэков из JS.
    public class VKAdProvider : MonoBehaviour, IAdService
    {
        public event Action OnRewardedComplete;
        public event Action OnRewardedFailed;
        public event Action OnInterstitialClosed;

        public bool IsRewardedReady { get; private set; }
        public bool IsInterstitialReady => false; // VK Bridge не поддерживает interstitial

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void VKLoadRewarded(string gameObject);

        [DllImport("__Internal")]
        private static extern void VKShowRewarded(string gameObject);
#endif

        public UniTask Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            VKLoadRewarded(gameObject.name);
#endif
            return UniTask.CompletedTask;
        }

        public void ShowRewarded()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            VKShowRewarded(gameObject.name);
#else
            OnRewardedFailed?.Invoke();
#endif
        }

        public void ShowInterstitial()
        {
            Debug.LogWarning("[VKAd] Interstitial не поддерживается в VK Bridge.");
            OnInterstitialClosed?.Invoke();
        }

        // Колбэки из JS через UnitySendMessage:

        private void OnRewardedLoaded(string result)
        {
            IsRewardedReady = result == "1";
        }

        private void OnRewardEarned()
        {
            OnRewardedComplete?.Invoke();
        }

        private void OnRewardFailed()
        {
            IsRewardedReady = false;
            OnRewardedFailed?.Invoke();
#if UNITY_WEBGL && !UNITY_EDITOR
            VKLoadRewarded(gameObject.name); // перезагружаем рекламу
#endif
        }

        private void OnAdClosed()
        {
            // Rewarded ad закрыт (уже обработано в OnRewardEarned/OnRewardFailed)
        }
    }
}
#endif

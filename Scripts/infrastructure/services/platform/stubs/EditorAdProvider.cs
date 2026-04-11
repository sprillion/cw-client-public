using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.ads;
using UnityEngine;

namespace infrastructure.services.platform.stubs
{
    public class EditorAdProvider : IAdService
    {
        public event Action OnRewardedComplete;
        public event Action OnRewardedFailed;
        public event Action OnInterstitialClosed;

        public bool IsRewardedReady => true;
        public bool IsInterstitialReady => true;

        public async UniTask Initialize()
        {
            Debug.Log("[EditorAds] Initialize()");
            await UniTask.CompletedTask;
        }

        public void ShowRewarded()
        {
            Debug.Log("[EditorAds] ShowRewarded() — auto complete");
            OnRewardedComplete?.Invoke();
        }

        public void ShowInterstitial()
        {
            Debug.Log("[EditorAds] ShowInterstitial() — auto closed");
            OnInterstitialClosed?.Invoke();
        }
    }
}

using UnityEngine;

namespace infrastructure.services.platform.core
{
    [CreateAssetMenu(menuName = "Platform/Settings")]
    public class PlatformSettings : ScriptableObject
    {
        [Header("Платформа")]
        public Platform platform;

        [Header("Доступные методы авторизации")]
        public bool allowGuest = true;
        public bool allowGoogle;
        public bool allowVK;
        public bool allowYandex;

        [Header("Реклама")]
        public string interstitialAdId;
        public string rewardedAdId;
    }
}

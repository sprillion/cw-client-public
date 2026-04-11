using System.Collections.Generic;
using infrastructure.services.platform.core.payment;
using UnityEngine;

namespace infrastructure.services.platform.core
{
    /// <summary>
    /// Единый конфиг всех платформ. Назначается один раз в PlatformInstaller.
    /// Нужная платформа выбирается автоматически через Scripting Define Symbols:
    ///   UNITY_EDITOR            → editor
    ///   YANDEX_GAMES (WebGL)    → yandex
    ///   VK_PLAY      (WebGL)    → vk
    ///   GOOGLE_PLAY  (Android)  → googlePlay
    ///   RU_STORE     (Android)  → ruStore
    /// </summary>
    [CreateAssetMenu(menuName = "Platform/Config")]
    public class PlatformConfig : ScriptableObject
    {
        public PlatformSettings editor;
        public PlatformSettings yandex;
        public PlatformSettings vk;
        public PlatformSettings googlePlay;
        public PlatformSettings ruStore;

        [Header("Продукты (покупки)")]
        public PlatformProductConfig[] products;

        public PlatformSettings GetCurrent()
        {
#if UNITY_EDITOR
            return editor;
#elif YANDEX_GAMES
            return yandex;
#elif VK_PLAY
            return vk;
#elif GOOGLE_PLAY
            return googlePlay;
#elif RU_STORE
            return ruStore;
#else
            Debug.LogError("[PlatformConfig] Unknown platform! Add a Scripting Define Symbol.");
            return editor;
#endif
        }

        public PaymentProductConfig[] GetCurrentProducts()
        {
            var result = new List<PaymentProductConfig>();
            foreach (var p in products)
            {
#if UNITY_EDITOR
                if (p.availableEditor)
                    result.Add(new PaymentProductConfig { productId = p.productId, platformId = p.editorPlatformId, type = p.type });
#elif YANDEX_GAMES
                if (p.availableYandex)
                    result.Add(new PaymentProductConfig { productId = p.productId, platformId = p.yandexPlatformId, type = p.type });
#elif VK_PLAY
                if (p.availableVK)
                    result.Add(new PaymentProductConfig { productId = p.productId, platformId = p.vkPlatformId, type = p.type });
#elif GOOGLE_PLAY
                if (p.availableGooglePlay)
                    result.Add(new PaymentProductConfig { productId = p.productId, platformId = p.googlePlayPlatformId, type = p.type });
#elif RU_STORE
                if (p.availableRuStore)
                    result.Add(new PaymentProductConfig { productId = p.productId, platformId = p.ruStorePlatformId, type = p.type });
#endif
            }
            return result.ToArray();
        }
    }
}

#if VK_PLAY
using System.Collections.Generic;
using infrastructure.services.platform.core.analytics;
using UnityEngine;

namespace infrastructure.services.platform.vk
{
    public class VKAnalyticsProvider : IAnalyticsService
    {
        public void TrackEvent(string name, Dictionary<string, object> parameters = null)
            => Debug.Log($"[YandexAnalytics] TrackEvent: {name}");

        public void TrackPurchase(string productId, decimal price, string currency)
            => Debug.Log($"[YandexAnalytics] TrackPurchase: {productId} {price} {currency}");

        public void TrackAdRevenue(string adType, double revenue, string currency)
            => Debug.Log($"[YandexAnalytics] TrackAdRevenue: {adType} {revenue} {currency}");
        
    }
}
#endif

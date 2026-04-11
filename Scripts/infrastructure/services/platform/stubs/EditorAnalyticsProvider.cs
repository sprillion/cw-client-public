using System.Collections.Generic;
using infrastructure.services.platform.core.analytics;
using UnityEngine;

namespace infrastructure.services.platform.stubs
{
    public class EditorAnalyticsProvider : IAnalyticsService
    {
        public void TrackEvent(string name, Dictionary<string, object> parameters = null)
        {
            Debug.Log($"[EditorAnalytics] TrackEvent: {name}");
        }

        public void TrackPurchase(string productId, decimal price, string currency)
        {
            Debug.Log($"[EditorAnalytics] TrackPurchase: {productId} {price} {currency}");
        }

        public void TrackAdRevenue(string adType, double revenue, string currency)
        {
            Debug.Log($"[EditorAnalytics] TrackAdRevenue: {adType} {revenue} {currency}");
        }
    }
}

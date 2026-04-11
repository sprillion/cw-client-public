using System.Collections.Generic;

namespace infrastructure.services.platform.core.analytics
{
    public interface IAnalyticsService
    {
        void TrackEvent(string name, Dictionary<string, object> parameters = null);
        void TrackPurchase(string productId, decimal price, string currency);
        void TrackAdRevenue(string adType, double revenue, string currency);
    }
}

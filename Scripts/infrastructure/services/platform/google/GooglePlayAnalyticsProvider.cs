#if GOOGLE_PLAY
using System.Collections.Generic;
using infrastructure.services.platform.core.analytics;

namespace infrastructure.services.platform.google
{
    /// <summary>
    /// Заглушка аналитики для Google Play. AppMetrica SDK — отдельный этап.
    /// </summary>
    public class GooglePlayAnalyticsProvider : IAnalyticsService
    {
        public void TrackEvent(string name, Dictionary<string, object> parameters = null) { }
        public void TrackPurchase(string productId, decimal price, string currency) { }
        public void TrackAdRevenue(string adType, double revenue, string currency) { }
    }
}
#endif

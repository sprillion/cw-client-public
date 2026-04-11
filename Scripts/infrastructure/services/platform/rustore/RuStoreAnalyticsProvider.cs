#if RU_STORE
using System.Collections.Generic;
using infrastructure.services.platform.core.analytics;

namespace infrastructure.services.platform.rustore
{
    /// <summary>
    /// Заглушка аналитики для RuStore. AppMetrica SDK — отдельный этап.
    /// </summary>
    public class RuStoreAnalyticsProvider : IAnalyticsService
    {
        public void TrackEvent(string name, Dictionary<string, object> parameters = null) { }
        public void TrackPurchase(string productId, decimal price, string currency) { }
        public void TrackAdRevenue(string adType, double revenue, string currency) { }
    }
}
#endif

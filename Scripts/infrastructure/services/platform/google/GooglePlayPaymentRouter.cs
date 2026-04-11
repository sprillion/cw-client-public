#if GOOGLE_PLAY
using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.language;
using infrastructure.services.platform.core.payment;
using network;
using Zenject;

namespace infrastructure.services.platform.google
{
    /// <summary>
    /// Маршрутизатор платёжных провайдеров для Google Play.
    /// Начальная логика: русский язык → Робокасса, остальные → Google Play Billing.
    /// UseRobokassa() / UseGooglePlayBilling() позволяют переключить провайдер в runtime
    /// (по ответу сервера, данным региона и т.д.).
    /// </summary>
    public class GooglePlayPaymentRouter : IPaymentService
    {
        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<PaymentProduct[]> OnCatalogLoaded;
        public bool IsInitialized => _active?.IsInitialized ?? false;

        private readonly RobokassaPaymentProvider _robokassa;
        private readonly GooglePlayBillingProvider _billing;
        private readonly ILanguageService _language;
        private IPaymentService _active;

        [Inject]
        public GooglePlayPaymentRouter(
            RobokassaPaymentProvider robokassa,
            GooglePlayBillingProvider billing,
            ILanguageService language)
        {
            _robokassa = robokassa;
            _billing   = billing;
            _language  = language;

            _robokassa.OnPurchaseSuccess += id => OnPurchaseSuccess?.Invoke(id);
            _robokassa.OnPurchaseFailed  += id => OnPurchaseFailed?.Invoke(id);
            _robokassa.OnCatalogLoaded   += p  => OnCatalogLoaded?.Invoke(p);
            _billing.OnPurchaseSuccess   += id => OnPurchaseSuccess?.Invoke(id);
            _billing.OnPurchaseFailed    += id => OnPurchaseFailed?.Invoke(id);
            _billing.OnCatalogLoaded     += p  => OnCatalogLoaded?.Invoke(p);
        }

        public async UniTask Initialize()
        {
            _active = SelectProvider();
            await _active.Initialize();
        }

        public void LoadCatalog()              => _active.LoadCatalog();
        public void Purchase(string productId)  => _active.Purchase(productId);
        public void ReceiveMessage(Message m)   => _active.ReceiveMessage(m);

        /// <summary>Переключение на Робокассу (например, по ответу сервера с регионом РФ).</summary>
        public void UseRobokassa()         => _active = _robokassa;

        /// <summary>Переключение на Google Play Billing (не российские пользователи).</summary>
        public void UseGooglePlayBilling() => _active = _billing;

        private IPaymentService SelectProvider() =>
            _language.CurrentLanguage == "ru" ? (IPaymentService)_robokassa : _billing;
    }
}
#endif

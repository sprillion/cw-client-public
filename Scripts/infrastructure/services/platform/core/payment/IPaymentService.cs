using System;
using Cysharp.Threading.Tasks;
using network;

namespace infrastructure.services.platform.core.payment
{
    public interface IPaymentService : IReceiver
    {
        event Action<string> OnPurchaseSuccess;
        event Action<string> OnPurchaseFailed;
        event Action<PaymentProduct[]> OnCatalogLoaded;

        bool IsInitialized { get; }

        UniTask Initialize();
        void LoadCatalog();
        void Purchase(string productId);
    }
}

using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core;
using infrastructure.services.platform.core.payment;
using network;
using UnityEngine;

namespace infrastructure.services.platform.stubs
{
    public class EditorPaymentProvider : IPaymentService
    {
        private INetworkManager _networkManager;
        private PaymentProductConfig[] _products;

        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<PaymentProduct[]> OnCatalogLoaded;

        public bool IsInitialized { get; private set; }

        [Zenject.Inject]
        public void Construct(INetworkManager networkManager, PlatformConfig config)
        {
            _networkManager = networkManager;
            _products = config.GetCurrentProducts();
        }

        public async UniTask Initialize()
        {
            Debug.Log("[EditorPayment] Initialize()");
            IsInitialized = true;
            await UniTask.CompletedTask;
        }

        public void LoadCatalog()
        {
            var products = new PaymentProduct[_products.Length];
            for (int i = 0; i < _products.Length; i++)
            {
                var c = _products[i];
                products[i] = new PaymentProduct
                {
                    ProductId  = c.productId,
                    PlatformId = c.platformId,
                    Title      = c.productId,
                    Type       = c.type
                };
            }

            OnCatalogLoaded?.Invoke(products);
        }

        public void Purchase(string productId)
        {
            Debug.Log($"[EditorPayment] Purchase({productId}) — auto success");
            OnPurchaseSuccess?.Invoke(productId);
        }

        public void ReceiveMessage(Message message) { }
    }
}

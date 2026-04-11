#if YANDEX_GAMES
using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core;
using infrastructure.services.platform.core.payment;
using network;
using UnityEngine;
using YandexGames;

namespace infrastructure.services.platform.yandex
{
    public class YandexPaymentProvider : IPaymentService
    {
        private enum FromClientMessage : byte { InitiatePurchase = 1 }
        private enum FromServerMessage : byte { PurchaseResult = 1 }

        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<PaymentProduct[]> OnCatalogLoaded;
        public bool IsInitialized { get; private set; }

        private readonly INetworkManager _networkManager;
        private readonly PaymentProductConfig[] _products;
        private string _pendingConsumeToken;

        public YandexPaymentProvider(INetworkManager networkManager, PlatformConfig config)
        {
            _networkManager = networkManager;
            _products = config.GetCurrentProducts();
        }

        public async UniTask Initialize()
        {
            await UniTask.WaitUntil(() => Billing.IsReady);
            IsInitialized = true;
        }

        public void LoadCatalog()
        {
            Billing.GetProductCatalog(
                onSuccessCallback: r => OnCatalogLoaded?.Invoke(MapCatalog(r.products)),
                onErrorCallback: err => Debug.LogWarning($"[YandexPayment] Catalog error: {err}"));
        }

        public void Purchase(string productId)
        {
            var config = FindConfig(productId);
            if (config == null) { OnPurchaseFailed?.Invoke(productId); return; }

            Billing.PurchaseProduct(
                productId: config.platformId,
                onSuccessCallback: r => OnPurchased(productId, config.platformId, r),
                onErrorCallback: _ => OnPurchaseFailed?.Invoke(productId),
                developerPayload: "");
        }

        private void OnPurchased(string productId, string platformId, PurchaseProductResponse r)
        {
            _pendingConsumeToken = r.purchaseData.purchaseToken;
            var message = new Message(MessageType.Payment)
                .AddByte((byte)FromClientMessage.InitiatePurchase)
                .AddString(productId)
                .AddString(platformId)
                .AddString(r.purchaseData.purchaseToken)
                .AddString(r.signature);
            _networkManager.SendMessage(message);
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            if (type != FromServerMessage.PurchaseResult) return;

            var success = message.GetBool();
            var productId = message.GetString();

            if (success)
            {
                if (_pendingConsumeToken != null)
                    Billing.ConsumeProduct(_pendingConsumeToken, null, null);
                _pendingConsumeToken = null;
                OnPurchaseSuccess?.Invoke(productId);
            }
            else
            {
                _pendingConsumeToken = null;
                OnPurchaseFailed?.Invoke(productId);
            }
        }

        private PaymentProductConfig FindConfig(string productId)
        {
            foreach (var c in _products)
                if (c.productId == productId) return c;
            return null;
        }

        private PaymentProduct[] MapCatalog(CatalogProduct[] products)
        {
            var result = new PaymentProduct[products.Length];
            for (int i = 0; i < products.Length; i++)
                result[i] = new PaymentProduct
                {
                    ProductId = products[i].id,
                    PlatformId = products[i].id,
                    Title = products[i].title,
                    Description = products[i].description,
                    Price = products[i].price,
                    Type = ProductType.Consumable
                };
            return result;
        }
    }
}
#endif

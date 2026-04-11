#if GOOGLE_PLAY
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core;
using infrastructure.services.platform.core.payment;
using network;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Zenject;

namespace infrastructure.services.platform.google
{
    /// <summary>
    /// Флоу оплаты через Google Play Billing (не российские пользователи):
    /// 1. Initialize() — регистрируем продукты в Unity IAP
    /// 2. Purchase(productId) — _controller.InitiatePurchase(platformId)
    /// 3. ProcessPurchase — отправляем receipt на сервер: MessageType.Payment / InitiatePurchase
    ///    Возвращаем Pending — подтверждаем покупку только после ответа сервера
    /// 4. ReceiveMessage(PurchaseResult) — ConfirmPendingPurchase + fire событие
    /// </summary>
    public class GooglePlayBillingProvider : IPaymentService, IDetailedStoreListener
    {
        private enum FromClientMessage : byte { InitiatePurchase = 1 }
        private enum FromServerMessage : byte { PurchaseResult = 1 }

        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<PaymentProduct[]> OnCatalogLoaded;
        public bool IsInitialized { get; private set; }

        private readonly INetworkManager _networkManager;
        private readonly PaymentProductConfig[] _products;
        private IStoreController _controller;
        private UniTaskCompletionSource<bool> _initTcs;
        // игровой productId → IAP Product, ожидающий подтверждения сервера
        private readonly Dictionary<string, Product> _pendingProducts = new Dictionary<string, Product>();

        [Inject]
        public GooglePlayBillingProvider(INetworkManager networkManager, PlatformConfig config)
        {
            _networkManager = networkManager;
            _products = config.GetCurrentProducts();
        }

        public async UniTask Initialize()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var c in _products)
                builder.AddProduct(c.platformId, ProductType.Consumable);

            _initTcs = new UniTaskCompletionSource<bool>();
            UnityPurchasing.Initialize(this, builder);
            await _initTcs.Task;
        }

        // IDetailedStoreListener ──────────────────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            IsInitialized = true;

            var products = new PaymentProduct[_products.Length];
            for (int i = 0; i < _products.Length; i++)
            {
                var c = _products[i];
                var iapProduct = _controller.products.WithID(c.platformId);
                products[i] = new PaymentProduct
                {
                    ProductId  = c.productId,
                    PlatformId = c.platformId,
                    Title      = iapProduct?.metadata.localizedTitle ?? c.productId,
                    Type       = c.type
                };
            }
            OnCatalogLoaded?.Invoke(products);
            _initTcs.TrySetResult(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogWarning($"[GooglePlayBilling] Init failed: {error}");
            IsInitialized = false;
            _initTcs.TrySetResult(false);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
            => OnInitializeFailed(error);

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var platformId = args.purchasedProduct.definition.id;
            var config = FindConfig(platformId);
            if (config == null) return PurchaseProcessingResult.Complete;

            // Сохраняем — подтвердим только после ответа сервера
            _pendingProducts[config.productId] = args.purchasedProduct;

            // Отправляем JSON receipt на сервер; сервер извлечёт purchaseToken и packageName
            _networkManager.SendMessage(
                new Message(MessageType.Payment)
                    .AddByte((byte)FromClientMessage.InitiatePurchase)
                    .AddString(config.productId)
                    .AddString(platformId)
                    .AddString(args.purchasedProduct.receipt));

            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var config = FindConfig(product.definition.id);
            OnPurchaseFailed?.Invoke(config?.productId ?? product.definition.id);
        }

        // ─────────────────────────────────────────────────────────────────────

        public void LoadCatalog()
        {
            // Каталог формируется в OnInitialized после успешной инициализации IAP
        }

        public void Purchase(string productId)
        {
            var config = FindConfig(productId);
            if (config == null) { OnPurchaseFailed?.Invoke(productId); return; }
            _controller.InitiatePurchase(config.platformId);
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            if (type != FromServerMessage.PurchaseResult) return;

            var success   = message.GetBool();
            var productId = message.GetString();

            if (_pendingProducts.TryGetValue(productId, out var product))
            {
                _controller.ConfirmPendingPurchase(product);
                _pendingProducts.Remove(productId);
            }

            if (success) OnPurchaseSuccess?.Invoke(productId);
            else         OnPurchaseFailed?.Invoke(productId);
        }

        private PaymentProductConfig FindConfig(string id)
        {
            foreach (var c in _products)
                if (c.productId == id || c.platformId == id) return c;
            return null;
        }
    }
}
#endif

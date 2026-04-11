#if VK_PLAY
using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core;
using infrastructure.services.platform.core.payment;
using network;
using UnityEngine;

namespace infrastructure.services.platform.vk
{
    // MonoBehaviour — нужен для приёма SendMessage-колбэков из JS.
    public class VKPaymentProvider : MonoBehaviour, IPaymentService
    {
        private enum FromClientMessage : byte { InitiatePurchase = 1 }
        private enum FromServerMessage : byte { PurchaseResult = 1 }

        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<PaymentProduct[]> OnCatalogLoaded;
        public bool IsInitialized { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void VKPurchaseProduct(string gameObject, string productId);
#endif

        private INetworkManager _networkManager;
        private PaymentProductConfig[] _products;
        private string _pendingProductId;
        private string _pendingPlatformId;

        [Zenject.Inject]
        public void Construct(INetworkManager networkManager, PlatformConfig config)
        {
            _networkManager = networkManager;
            _products = config.GetCurrentProducts();
        }

        public UniTask Initialize()
        {
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public void LoadCatalog()
        {
            // VK Bridge не предоставляет API каталога — используем конфиг
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
            var config = FindConfig(productId);
            if (config == null) { OnPurchaseFailed?.Invoke(productId); return; }

            _pendingProductId  = productId;
            _pendingPlatformId = config.platformId;

#if UNITY_WEBGL && !UNITY_EDITOR
            VKPurchaseProduct(gameObject.name, config.platformId);
#else
            OnPurchaseFailed?.Invoke(productId);
#endif
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            if (type != FromServerMessage.PurchaseResult) return;

            var success   = message.GetBool();
            var productId = message.GetString();

            if (success)
                OnPurchaseSuccess?.Invoke(productId);
            else
                OnPurchaseFailed?.Invoke(productId);

            _pendingProductId = null;
        }

        // Колбэки из JS через UnitySendMessage:

        private void OnVKPurchaseSuccess(string orderId)
        {
            if (_pendingProductId == null) return;
            var message = new Message(MessageType.Payment)
                .AddByte((byte)FromClientMessage.InitiatePurchase)
                .AddString(_pendingProductId)
                .AddString(_pendingPlatformId)
                .AddString(orderId);
            _networkManager.SendMessage(message);
        }

        private void OnVKPurchaseFailed()
        {
            OnPurchaseFailed?.Invoke(_pendingProductId ?? "");
            _pendingProductId = null;
        }

        private PaymentProductConfig FindConfig(string productId)
        {
            foreach (var c in _products)
                if (c.productId == productId) return c;
            return null;
        }
    }
}
#endif

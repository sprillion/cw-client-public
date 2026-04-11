#if RU_STORE
using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core;
using infrastructure.services.platform.core.payment;
using network;
using UnityEngine;
using Zenject;

namespace infrastructure.services.platform.rustore
{
    /// <summary>
    /// Флоу оплаты через Робокассу:
    /// 1. Purchase(productId)  → сервер: RequestInvoice
    /// 2. Сервер               → клиент: InvoiceUrl → открываем в браузере
    /// 3. Робокасса webhook    → сервер → клиент: PurchaseResult
    /// 4. CheckInvoice()       → сервер: CheckInvoice (по кнопке «я уже оплатил»)
    /// </summary>
    public class RobokassaPaymentProvider : IPaymentService
    {
        private enum FromClientMessage : byte
        {
            RequestInvoice = 0,
            CheckInvoice   = 2,
        }

        private enum FromServerMessage : byte
        {
            InvoiceUrl    = 0,
            PurchaseResult = 1,
        }

        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<PaymentProduct[]> OnCatalogLoaded;
        public bool IsInitialized { get; private set; }

        private readonly INetworkManager _networkManager;
        private readonly PaymentProductConfig[] _products;

        private string _pendingProductId;
        private string _pendingInvoiceId;

        [Inject]
        public RobokassaPaymentProvider(INetworkManager networkManager, PlatformConfig config)
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
            _pendingProductId = productId;
            _networkManager.SendMessage(
                new Message(MessageType.Payment)
                    .AddByte((byte)FromClientMessage.RequestInvoice)
                    .AddString(productId));
        }

        /// <summary>Запросить статус инвойса вручную (кнопка «я уже оплатил»).</summary>
        public void CheckInvoice()
        {
            if (_pendingInvoiceId == null) return;
            _networkManager.SendMessage(
                new Message(MessageType.Payment)
                    .AddByte((byte)FromClientMessage.CheckInvoice)
                    .AddString(_pendingInvoiceId));
        }

        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();
            switch (type)
            {
                case FromServerMessage.InvoiceUrl:
                    message.GetString(); // productId — уже сохранён в _pendingProductId
                    var url = message.GetString();
                    _pendingInvoiceId = message.GetString();
                    Application.OpenURL(url);
                    break;

                case FromServerMessage.PurchaseResult:
                    var success   = message.GetBool();
                    var productId = message.GetString();
                    if (success) OnPurchaseSuccess?.Invoke(productId);
                    else         OnPurchaseFailed?.Invoke(productId);
                    _pendingProductId = null;
                    _pendingInvoiceId = null;
                    break;
            }
        }
    }
}
#endif

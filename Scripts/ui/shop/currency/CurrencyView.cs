using System.Collections.Generic;
using infrastructure.factories;
using infrastructure.services.platform.core;
using infrastructure.services.platform.core.payment;
using ui.popup;
using UnityEngine;
using Zenject;

namespace ui.shop.currency
{
    public class CurrencyView : Popup
    {
        [SerializeField] private Transform _elementsParent;
        [SerializeField] private LoadingPanel _loadingPanel;

        private IPlatformService _platformService;
        private CurrencyProductCatalog _productCatalog;
        private PaymentProduct[] _catalog;
        private readonly List<CurrencyElement> _elements = new();

        [Inject]
        public void Construct(IPlatformService platformService)
        {
            _platformService = platformService;
            _platformService.Payment.OnCatalogLoaded += OnCatalogLoaded;
            _platformService.Payment.OnPurchaseSuccess += OnPurchaseSuccess;
            _platformService.Payment.OnPurchaseFailed += OnPurchaseFailed;
        }

        public override void Initialize()
        {
            Pool.CreatePool<CurrencyElement>(8);
            _productCatalog = GameResources.Data.Catalogs.currency_product_catalog<CurrencyProductCatalog>();
            CurrencyElement.OnBuyClicked += OnElementBuyClicked;
        }

        public override void Show()
        {
            base.Show();

            if (_catalog == null)
            {
                _loadingPanel.Show();
                _platformService.Payment.LoadCatalog();
            }
            else
            {
                ShowElements();
            }
        }

        public override void Hide()
        {
            ReleaseElements();
            base.Hide();
        }

        private void ShowElements()
        {
            _loadingPanel.Hide();
            ReleaseElements();

            foreach (var product in _catalog)
            {
                var element = Pool.Get<CurrencyElement>();
                element.transform.SetParent(_elementsParent, false);
                _elements.Add(element);
                var sprite = _productCatalog.GetSprite(product.ProductId);
                element.SetData(product, sprite);
            }
        }

        private void ReleaseElements()
        {
            foreach (var el in _elements)
                el.Release();
            _elements.Clear();
        }

        private void OnCatalogLoaded(PaymentProduct[] products)
        {
            _catalog = products;
            if (IsActive)
                ShowElements();
        }

        private void OnPurchaseSuccess(string productId)
        {
            FindElement(productId)?.SetIdle();
        }

        private void OnPurchaseFailed(string productId)
        {
            FindElement(productId)?.SetIdle();
        }

        private void OnElementBuyClicked(CurrencyElement element)
        {
            // Reserved for future use (e.g. disable other buttons during purchase)
        }

        private CurrencyElement FindElement(string productId)
        {
            foreach (var el in _elements)
            {
                if (el.ProductId == productId)
                    return el;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (_platformService != null)
            {
                _platformService.Payment.OnCatalogLoaded -= OnCatalogLoaded;
                _platformService.Payment.OnPurchaseSuccess -= OnPurchaseSuccess;
                _platformService.Payment.OnPurchaseFailed -= OnPurchaseFailed;
            }

            CurrencyElement.OnBuyClicked -= OnElementBuyClicked;
        }
    }
}

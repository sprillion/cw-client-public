using System;
using infrastructure.services.platform.core;
using infrastructure.services.platform.core.payment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.shop.currency
{
    public class CurrencyElement : PooledObject
    {
        [SerializeField] private Image _productIcon;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private LoadingPanel _loadingPanel;

        private IPlatformService _platformService;

        public static event Action<CurrencyElement> OnBuyClicked;

        public string ProductId { get; private set; }

        [Inject]
        public void Construct(IPlatformService platformService)
        {
            _platformService = platformService;
        }

        private void Start()
        {
            _buyButton.onClick.AddListener(Buy);
        }

        public override void OnGetted()
        {
            _loadingPanel.Hide();
            _buyButton.gameObject.SetActive(true);
            _buyButton.interactable = true;
            base.OnGetted();
        }

        public void SetData(PaymentProduct product, Sprite icon)
        {
            ProductId = product.ProductId;
            _productIcon.sprite = icon;
            // _titleText.text = string.IsNullOrEmpty(product.Title) ? product.ProductId : product.Title;
            _titleText.text = $"Shop/{product.ProductId}".Loc();

            var hasPrice = !string.IsNullOrEmpty(product.Price);
            _priceText.gameObject.SetActive(hasPrice);
            if (hasPrice)
                _priceText.text = product.Price;
        }

        public void SetPurchasing()
        {
            _buyButton.interactable = false;
            _loadingPanel.Show();
        }

        public void SetIdle()
        {
            _buyButton.interactable = true;
            _loadingPanel.Hide();
        }

        private void Buy()
        {
            SetPurchasing();
            OnBuyClicked?.Invoke(this);
            _platformService.Payment.Purchase(ProductId);
        }
    }
}

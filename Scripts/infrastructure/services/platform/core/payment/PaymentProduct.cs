using System;
using UnityEngine;

namespace infrastructure.services.platform.core.payment
{
    public enum ProductType
    {
        Consumable,
        NonConsumable,
        Subscription,
    }

    [Serializable]
    public class PaymentProductConfig
    {
        public string productId;
        public string platformId;
        public ProductType type;
    }

    [Serializable]
    public class PlatformProductConfig
    {
        public string productId;
        public ProductType type;

        [Header("Editor")]
        public bool availableEditor;
        public string editorPlatformId;

        [Header("Yandex")]
        public bool availableYandex;
        public string yandexPlatformId;

        [Header("VK")]
        public bool availableVK;
        public string vkPlatformId;

        [Header("Google Play")]
        public bool availableGooglePlay;
        public string googlePlayPlatformId;

        [Header("RuStore")]
        public bool availableRuStore;
        public string ruStorePlatformId;
    }

    public class PaymentProduct
    {
        public string ProductId { get; set; }
        public string PlatformId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public ProductType Type { get; set; }
    }
}

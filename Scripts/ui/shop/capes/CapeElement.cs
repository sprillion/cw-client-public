using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.shop;
using infrastructure.services.shop.capes;
using TMPro;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.shop.capes
{
    public class CapeElement : PooledObject
    {
        [SerializeField] private Image _capeIcon;
        [SerializeField] private Image _emblemIcon;
        [SerializeField] private Button _previewButton;
        [SerializeField] private Button _putOnButton;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _equipedButton;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private GameObject _notAvailable;
        [SerializeField] private Image _rareImage;
        [SerializeField] private GameObject _selectedObject;
        [SerializeField] private LoadingPanel _loadingPanel;

        private CapeData _capeData;

        private bool _available;
        private bool _equiped;

        private IShopService _shopService;
        private ColorCatalog _rareColorCatalog;

        public static event Action<CapeElement> OnPreviewClick;

        private static bool _equipCooldown;
        
        public character.CapeData Data { get; private set; }

        [Inject]
        public void Construct(IShopService shopService)
        {
            _shopService = shopService;
            _rareColorCatalog = GameResources.Data.Catalogs.rare_color_catalog<ColorCatalog>();
        }

        private void Start()
        {
            _buyButton.onClick.AddListener(BuyCape);
            _putOnButton.onClick.AddListener(EquipCape);
            _previewButton.onClick.AddListener(() => OnPreviewClick?.Invoke(this));
            _equipedButton.onClick.AddListener(UnequipCape);
        }

        public override void OnGetted()
        {
            _loadingPanel.Show();
            _capeIcon.gameObject.SetActive(false);
            _emblemIcon.gameObject.SetActive(false);
            base.OnGetted();
        }

        public override void Release()
        {
            _available = false;
            _equiped = false;
            _selectedObject.SetActive(false);
            base.Release();
        }

        public void SetData(character.CapeData data, CapeData capeData)
        {
            Data = data;
            _capeData = capeData;

            // _capeIcon.sprite = data.AvatarIcon;
            _capeIcon.sprite = data.CapeSprite;
            _emblemIcon.sprite = data.EmblemSprite;
            _capeIcon.color = data.CapeColor;
            _emblemIcon.color = data.EmblemColor;
            
            var icon = capeData.CurrencyType switch
            {
                CurrencyType.Gold => IconType.Gold,
                CurrencyType.Diamonds => IconType.Diamond
            };
            _priceText.text = $"{capeData.Price}{icon.ToIcon()}";

            _rareImage.color = _rareColorCatalog.GetColor(_capeData.RareType);
            _notAvailable.SetActive(!capeData.CanBuy);
            _buyButton.gameObject.SetActive(capeData.CanBuy);
            _equipedButton.gameObject.SetActive(false);
            _putOnButton.gameObject.SetActive(false);
            _capeIcon.gameObject.SetActive(true);
            _emblemIcon.gameObject.SetActive(data.EmblemTexture != null);
            _loadingPanel.Hide();
        }

        public void SetAvailable()
        {
            _available = true;
            _putOnButton.gameObject.SetActive(!_equiped);
            _notAvailable.SetActive(false);
            _buyButton.gameObject.SetActive(false);
            _equipedButton.gameObject.SetActive(_equiped);
        }

        public void SetEquiped()
        {
            _equiped = true;
            _available = true;
            _equipedButton.gameObject.SetActive(true);

            _notAvailable.SetActive(false);
            _buyButton.gameObject.SetActive(false);
            _putOnButton.gameObject.SetActive(false);
        }

        public void SetUnequiped()
        {
            _equiped = false;
            _equipedButton.gameObject.SetActive(false);

            _notAvailable.SetActive(!_available && !_capeData.CanBuy);
            _buyButton.gameObject.SetActive(!_available && _capeData.CanBuy);
            _putOnButton.gameObject.SetActive(_available);
        }
        
        public void SetEquipPreviewCape()
        {
            _selectedObject.SetActive(true);
        }

        public void SetUnequipPreviewCape()
        {
            _selectedObject.SetActive(false);
        }

        private void BuyCape()
        {
            _buyButton.gameObject.SetActive(!_shopService.BuyCape(Data.Id));
        }

        private void EquipCape()
        {
            if (_equipCooldown) return;
            if (!_shopService.PutOnCape(Data.Id)) return;
            OnPreviewClick?.Invoke(this);
            StartEquipCooldown().Forget();
        }

        private void UnequipCape()
        {
            _shopService.PutOnCape(-1);
        }

        private static async UniTaskVoid StartEquipCooldown()
        {
            _equipCooldown = true;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.75f));
            _equipCooldown = false;
        }
    }
}

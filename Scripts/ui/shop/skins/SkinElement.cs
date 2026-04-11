using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.shop;
using infrastructure.services.shop.skins;
using TMPro;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.shop.skins
{
    public class SkinElement : PooledObject
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _previewButton;
        [SerializeField] private Button _putOnButton;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private GameObject _on;
        [SerializeField] private GameObject _notAvailable;
        [SerializeField] private GameObject _selectedObject;
        [SerializeField] private Image _rareImage;
        [SerializeField] private LoadingPanel _loadingPanel;
        
        private SkinData _skinData;

        private bool _available;
        private bool _equiped;

        private IShopService _shopService;
        private ColorCatalog _rareColorCatalog;

        public static event Action<SkinElement> OnPreviewClick;

        private static bool _equipCooldown;
        
        public character.SkinData Data { get; private set; }

        [Inject]
        public void Construct(IShopService shopService)
        {
            _shopService = shopService;
            _rareColorCatalog = GameResources.Data.Catalogs.rare_color_catalog<ColorCatalog>();
        }
        
        private void Start()
        {
            _buyButton.onClick.AddListener(BuySkin);
            _putOnButton.onClick.AddListener(EquipSkin);
            _previewButton.onClick.AddListener(() => OnPreviewClick?.Invoke(this));
        }
        
        public override void OnGetted()
        {
            _loadingPanel.Show();
            _icon.gameObject.SetActive(false);
            base.OnGetted();
        }

        public override void Release()
        {
            _available = false;
            _equiped = false;
            _selectedObject.SetActive(false);
            base.Release();
        }

        public void SetData(character.SkinData data, SkinData skinData)
        {
            Data = data;
            _skinData = skinData;
            
            _icon.sprite = data.AvatarIcon;
            var icon = skinData.CurrencyType switch
            {
                CurrencyType.Gold => IconType.Gold, 
                CurrencyType.Diamonds => IconType.Diamond
            };
            _priceText.text = $"{skinData.Price}{icon.ToIcon()}";

            _rareImage.color = _rareColorCatalog.GetColor(_skinData.RareType);
            _notAvailable.SetActive(!skinData.CanBuy);
            _buyButton.gameObject.SetActive(skinData.CanBuy);
            _on.SetActive(false);
            _putOnButton.gameObject.SetActive(false);
            _icon.gameObject.SetActive(true);
            _loadingPanel.Hide();
        }

        public void SetAvailable()
        {
            _available = true;
            _putOnButton.gameObject.SetActive(!_equiped);
            _notAvailable.SetActive(false);
            _buyButton.gameObject.SetActive(false);
            _on.SetActive(_equiped);
        }

        public void SetEquiped()
        {
            _equiped = true;
            _available = true;
            _on.SetActive(true);
            
            _notAvailable.SetActive(false);
            _buyButton.gameObject.SetActive(false);
            _putOnButton.gameObject.SetActive(false);
        }

        public void SetUnequiped()
        {
            _equiped = false;
            _on.SetActive(false);
            
            _notAvailable.SetActive(!_available && !_skinData.CanBuy);
            _buyButton.gameObject.SetActive(!_available && _skinData.CanBuy);
            _putOnButton.gameObject.SetActive(_available);
        }
        
        public void SetEquipPreviewSkin()
        {
            _selectedObject.SetActive(true);
        }

        public void SetUnequipPreviewSkin()
        {
            _selectedObject.SetActive(false);
        }

        private void BuySkin()
        {
            _buyButton.gameObject.SetActive(!_shopService.BuySkin(Data.Id));
        }

        private void EquipSkin()
        {
            if (_equipCooldown) return;
            if (!_shopService.PutOnSkin(Data.Id)) return;
            OnPreviewClick?.Invoke(this);
            StartEquipCooldown().Forget();
        }

        private static async UniTaskVoid StartEquipCooldown()
        {
            _equipCooldown = true;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.75f));
            _equipCooldown = false;
        }
    }
}
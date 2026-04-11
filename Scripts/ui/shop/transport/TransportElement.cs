using infrastructure.factories;
using infrastructure.services.transport;
using TMPro;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.shop.transport
{
    public class TransportElement : PooledObject
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _rareImage;
        [SerializeField] private GameObject _notAvailable;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _spawnButton;
        [SerializeField] private Button _despawnButton;

        private TransportData _data;
        private bool _owned;
        private bool _spawned;

        private ITransportService _transportService;
        private ColorCatalog _rareColorCatalog;

        [Inject]
        public void Construct(ITransportService transportService)
        {
            _transportService = transportService;
            _rareColorCatalog = GameResources.Data.Catalogs.rare_color_catalog<ColorCatalog>();
        }

        private void Start()
        {
            _buyButton.onClick.AddListener(BuyTransport);
            _spawnButton.onClick.AddListener(() => _transportService.SpawnTransport(_data.Id));
            _despawnButton.onClick.AddListener(() => _transportService.DespawnTransport());
        }

        public void SetData(TransportData data)
        {
            _data = data;
            _owned = false;
            _spawned = false;

            _icon.sprite = null;

            var icon = data.CurrencyType switch
            {
                CurrencyType.Gold     => IconType.Gold,
                CurrencyType.Diamonds => IconType.Diamond
            };
            _priceText.text = $"{data.Price}{icon.ToIcon()}";

            _rareImage.color = _rareColorCatalog.GetColor(data.RareType);

            _notAvailable.SetActive(!data.CanBuy);
            _buyButton.gameObject.SetActive(data.CanBuy);
            _spawnButton.gameObject.SetActive(false);
            _despawnButton.gameObject.SetActive(false);
        }

        public void SetAssetData(TransportAssetData assetData)
        {
            _icon.sprite = assetData.PreviewIcon;
        }

        public void SetOwned()
        {
            _owned = true;
            _notAvailable.SetActive(false);
            _buyButton.gameObject.SetActive(false);
            _spawnButton.gameObject.SetActive(!_spawned);
            _despawnButton.gameObject.SetActive(_spawned);
        }

        public void SetSpawned()
        {
            _spawned = true;
            _spawnButton.gameObject.SetActive(false);
            _despawnButton.gameObject.SetActive(true);
        }

        public void SetDespawned()
        {
            _spawned = false;
            _despawnButton.gameObject.SetActive(false);
            _spawnButton.gameObject.SetActive(_owned);
        }

        public override void Release()
        {
            _owned = false;
            _spawned = false;
            _spawnButton.gameObject.SetActive(false);
            _despawnButton.gameObject.SetActive(false);
            base.Release();
        }

        private void BuyTransport()
        {
            _buyButton.gameObject.SetActive(!_transportService.BuyTransport(_data.Id));
        }
    }
}

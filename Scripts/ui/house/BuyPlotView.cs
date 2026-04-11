using infrastructure.services.house;
using TMPro;
using ui.popup;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.house
{
    public class BuyPlotView : Popup
    {
        [SerializeField] private TMP_Text _price;

        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _closeBGButton;

        private IHouseService _houseService;

        [Inject]
        public void Construct(IHouseService houseService)
        {
            _houseService = houseService;
            _houseService.OnBuyPlotResult += HandleBuyResult;
        }

        public override void Initialize()
        {
            _buyButton.onClick.AddListener(_houseService.BuyPlot);
            _closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            var icon = _houseService.PlotCurrencyType switch
            {
                CurrencyType.Gold => IconType.Gold,
                CurrencyType.Diamonds => IconType.Diamond,
                _ => IconType.Gold
            };

            _price.text = $"{_houseService.PlotPrice}{icon.ToIcon()}";
            base.Show();
        }

        private void OnEnable()
        {
            _closeBGButton.gameObject.SetActive(true);
            _closeBGButton.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            _closeBGButton.gameObject.SetActive(false);
            _closeBGButton.onClick.RemoveListener(Back);
        }

        private void HandleBuyResult(bool success)
        {
            if (success)
                Hide();
        }
    }
}

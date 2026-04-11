using ui.popup;
using ui.shop.capes;
using ui.shop.cases;
using ui.shop.currency;
using ui.shop.other;
using ui.shop.skins;
using ui.shop.transport;
using ui.shop.vip;
using UnityEngine;
using UnityEngine.UI;

namespace ui.shop
{
    public class ShopView : Popup
    {
        [SerializeField] private Button _skinsButton;
        [SerializeField] private Button _capesButton;
        [SerializeField] private Button _transportButton;
        [SerializeField] private Button _casesButton;
        [SerializeField] private Button _vipButton;
        [SerializeField] private Button _currencyButton;
        [SerializeField] private Button _otherButton;

        [SerializeField] private Button _hideButton;

        [SerializeField] private SkinsView _skinsView;
        [SerializeField] private CapesView _capesView;
        [SerializeField] private TransportsView _transportsView;
        [SerializeField] private CasesView _casesView;
        [SerializeField] private VipView _vipView;
        [SerializeField] private CurrencyView _currencyView;
        [SerializeField] private OtherView _otherView;

        private Popup _currentPopup;
        private Button _currentButton;

        public override void Initialize()
        {
            _skinsView.Initialize();
            _capesView.Initialize();
            _transportsView.Initialize();
            // _casesView.Initialize();
            // _vipView.Initialize();
            _currencyView.Initialize();
            // _otherView.Initialize();

            _skinsButton.onClick.AddListener(() => ShowSection(SectionType.Skins));
            _capesButton.onClick.AddListener(() => ShowSection(SectionType.Capes));
            _transportButton.onClick.AddListener(() => ShowSection(SectionType.Transport));
            _casesButton.onClick.AddListener(() => ShowSection(SectionType.Cases));
            _vipButton.onClick.AddListener(() => ShowSection(SectionType.Vip));
            _currencyButton.onClick.AddListener(() => ShowSection(SectionType.Currency));
            _otherButton.onClick.AddListener(() => ShowSection(SectionType.Other));

            _hideButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            base.Show();
            ShowSection(SectionType.Skins);
        }

        public override void Hide()
        {
            _currentPopup?.Hide();
            base.Hide();
        }

        private void ShowSection(SectionType sectionType)
        {
            _currentPopup?.Hide();

            if (_currentButton)
            {
                _currentButton.interactable = true;
            }

            _currentPopup = sectionType switch
            {
                SectionType.Skins => _skinsView,
                SectionType.Capes => _capesView,
                SectionType.Transport => _transportsView,
                SectionType.Cases => _casesView,
                SectionType.Vip => _vipView,
                SectionType.Currency => _currencyView,
                SectionType.Other => _otherView,
                _ => null
            };

            _currentButton = sectionType switch
            {
                SectionType.Skins => _skinsButton,
                SectionType.Capes => _capesButton,
                SectionType.Transport => _transportButton,
                SectionType.Cases => _casesButton,
                SectionType.Vip => _vipButton,
                SectionType.Currency => _currencyButton,
                SectionType.Other => _otherButton,
                _ => null
            };

            _currentPopup?.Show();

            if (_currentButton)
            {
                _currentButton.interactable = false;
            }
        }
    }
}

using I2.Loc;
using infrastructure.services.house;
using TMPro;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.house
{
    public class UpgradeHouseView : Popup
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _maxLevelText;
        [SerializeField] private Button _closeButton;

        [SerializeField] private UpgradeHouseElement _upgradeHouseElement;

        private IHouseService _houseService;
        private HousePlaceType _currentType;

        [Inject]
        public void Construct(IHouseService houseService)
        {
            _houseService = houseService;
            _closeButton.onClick.AddListener(Back);
        }

        public override void Show(Popup backPopup, params object[] args)
        {
            SetInfo(args);
            base.Show(backPopup, args);
        }

        public override void Show(params object[] args)
        {
            SetInfo(args);
            base.Show(args);
        }

        public override void Hide()
        {
            _houseService.OnHouseReceived -= Refresh;
            _houseService.OnUpgradeResult -= OnUpgradeResult;
            base.Hide();
        }

        public override void AddToStack()
        {
        }

        public override void RemoveFromStack()
        {
        }
        
        private void SetInfo(params object[] args)
        {
            _currentType = (HousePlaceType)args[0];
            _houseService.OnHouseReceived += Refresh;
            _houseService.OnUpgradeResult += OnUpgradeResult;
            Refresh();
        }

        private void OnUpgradeResult(HousePlaceType _, bool success)
        {
            if (success)
                Back();
        }

        private void Refresh()
        {
            var info = _houseService.GetHousePlaceInfo(_currentType);
            int currentLevel = info?.Level ?? 0;
            var isMaxLevel = currentLevel >= (info?.MaxLevel ?? 0);

            if (currentLevel > 0)
            {
                _titleText.text = $"{$"House/{_currentType}".Loc()} {currentLevel} {"Game/Lvl".Loc()}";      
            }
            else
            {
                _titleText.text = $"House/{_currentType}".Loc();      
            }

            _maxLevelText.gameObject.SetActive(isMaxLevel);
            _upgradeHouseElement.gameObject.SetActive(!isMaxLevel);
            
            var craftData = _houseService.GetHouseUpgradeData(_currentType, currentLevel);
            _upgradeHouseElement.SetData(_currentType, craftData, currentLevel + 1);
        }
    }
}

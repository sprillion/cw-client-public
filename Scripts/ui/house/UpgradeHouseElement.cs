using System.Linq;
using infrastructure.services.house;
using network;
using TMPro;
using tools;
using ui.craft;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.house
{
    public class UpgradeHouseElement : PooledObject
    {
        [SerializeField] private CraftSlot[] _neededSlots;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _takeButton;

        private HousePlaceType _currentType;
        private HousePlaceCraftData _currentCraftData;
        private IHouseService _houseService;

        private bool _isCurrentUpgrade;
        private bool _isCompleteUpgrade;

        private float _currentDuration;

        [Inject]
        public void Construct(IHouseService houseService)
        {
            _houseService = houseService;

            _startButton.onClick.AddListener(ClickStart);
            _takeButton.onClick.AddListener(ClickTake);
        }

        private void Update()
        {
            SetCurrentTime();
        }

        public override void Release()
        {
            foreach (var neededSlot in _neededSlots)
            {
                neededSlot.Clear();
            }

            base.Release();
        }

        public void SetData(HousePlaceType type, HousePlaceCraftData data, int nextLevel)
        {
            _currentType = type;
            _currentCraftData = data;
            
            _levelText.text = $"<size=60>{nextLevel}</size>{"Game/Lvl".Loc()}";

            if (data != null)
            {
                ActivateNeededSlots(data.NeededItems.Count);
                CreateNeededItems();
                SetDuration();
            }

            SetButtons();
        }

        private void ActivateNeededSlots(int neededItemsCount)
        {
            for (var i = 0; i < _neededSlots.Length; i++)
            {
                _neededSlots[i].gameObject.SetActive(i < neededItemsCount);
            }
        }

        private void CreateNeededItems()
        {
            for (var i = 0; i < _currentCraftData.NeededItems.Count; i++)
            {
                _neededSlots[i].SetItem(_currentCraftData.NeededItems[i], false);
            }
        }

        private void SetButtons()
        {
            _isCurrentUpgrade = _houseService.IsCurrentUpgrade(_currentType);
            _isCompleteUpgrade = _houseService.IsCompleteUpgrade(_currentType);
            var isMaxLevel = _houseService.IsMaxLevel(_currentType);

            _startButton.gameObject.SetActive(!_isCurrentUpgrade && !isMaxLevel);
            _takeButton.gameObject.SetActive(_isCurrentUpgrade && !isMaxLevel);

            if (!_isCurrentUpgrade)
                _startButton.interactable = _neededSlots.Where(s => s.gameObject.activeSelf).All(s => s.IsReady);
            else
                _takeButton.interactable = _isCompleteUpgrade;
        }

        private void SetDuration()
        {
            if (_isCurrentUpgrade)
                _currentDuration = (float)(_houseService.GetFinishTimeUpgrade(_currentType) - NetworkManager.ServerNow).TotalSeconds;
            else
                _currentDuration = _currentCraftData.Duration;
            
            _timeText.text = _isCompleteUpgrade ? "House/Ready".Loc() : _currentDuration.FormatTime();
        }

        private void SetCurrentTime()
        {
            if (_isCompleteUpgrade) return;
            if (!_isCurrentUpgrade) return;

            _currentDuration -= Time.deltaTime;
            if (_currentDuration < 0)
            {
                SetButtons();
                SetDuration();
                return;
            }

            _timeText.text = _currentDuration.FormatTime();
        }

        private void ClickStart()
        {
            if (_isCurrentUpgrade || _isCompleteUpgrade) return;
            _houseService.StartUpgrade(_currentType);
        }

        private void ClickTake()
        {
            if (!_isCompleteUpgrade) return;
            _houseService.ApplyUpgrade(_currentType);
        }
    }
}

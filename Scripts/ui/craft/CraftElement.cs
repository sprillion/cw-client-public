using System.Linq;
using factories.inventory;
using I2.Loc;
using infrastructure.services.craft;
using network;
using TMPro;
using tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.craft
{
    public class CraftElement : PooledObject
    {
        [SerializeField] private CraftSlot[] _neededSlots;
        [SerializeField] private CraftSlot[] _resultSlots;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _takeButton;

        private CraftDataJson _currentData;
        private ICraftService _craftService;
        private IInventoryFactory _inventoryFactory;

        private bool _isCurrentCraft;
        private bool _isCompleteCraft;

        private float _currentDuration;
        

        [Inject]
        public void Construct(ICraftService craftService)
        {
            _craftService = craftService;

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

            foreach (var resultSlot in _resultSlots)
            {
                resultSlot.Clear();
            }

            base.Release();
        }

        public void SetData(CraftDataJson craftData)
        {
            _currentData = craftData;

            ActivateNeededSlots(craftData.NeededItems.Count);
            ActivateResultSlots(craftData.ResultItems.Count);
            
            CreateNeededItems();
            CreateResultItems();
            
            SetButtons();
            SetDuration();
        }

        private void ActivateNeededSlots(int neededItemsCount)
        {
            for (var i = 0; i < _neededSlots.Length; i++)
            {
                _neededSlots[i].gameObject.SetActive(i < neededItemsCount);
            }
        }

        private void ActivateResultSlots(int resultItemsCount)
        {
            for (var i = 0; i < _resultSlots.Length; i++)
            {
                _resultSlots[i].gameObject.SetActive(i < resultItemsCount);
            }
        }

        private void CreateNeededItems()
        {
            for (var i = 0; i < _currentData.NeededItems.Count; i++)
            {
                _neededSlots[i].SetItem(_currentData.NeededItems[i], false);
            }
        }

        private void CreateResultItems()
        {
            for (var i = 0; i < _currentData.ResultItems.Count; i++)
            {
                _resultSlots[i].SetItem(_currentData.ResultItems[i], true);
            }
        }

        private void SetButtons()
        {
            _isCurrentCraft = _craftService.IsCurrentCraft(_currentData.Id);
            _isCompleteCraft = _craftService.IsCompleteCraft(_currentData.Id);
            
            _startButton.gameObject.SetActive(!_isCurrentCraft);
            _takeButton.gameObject.SetActive(_isCurrentCraft);

            if (!_isCurrentCraft)
            {
                _startButton.interactable = _neededSlots.Where(s => s.gameObject.activeSelf).All(s => s.IsReady);
            }
            else
            {
                _takeButton.interactable = _isCompleteCraft;
            }
        }

        private void SetDuration()
        {
            if (_isCurrentCraft)
            {
                _currentDuration = (float)(_craftService.GetFinishTimeCraft(_currentData.Id) - NetworkManager.ServerNow).TotalSeconds;
            }
            else
            {
                _currentDuration = _currentData.Duration;
            }

            if (_isCompleteCraft)
            {
                _timeText.text = LocalizationManager.GetTranslation("House/Ready");
            }
            else
            {
                _timeText.text = _currentDuration.FormatTime();
            }
        }

        private void SetCurrentTime()
        {
            if (_isCompleteCraft) return;
            if (!_isCurrentCraft) return;

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
            if (_isCurrentCraft || _isCompleteCraft) return;
            _craftService.StartCraft(_currentData.Id);
        }

        private void ClickTake()
        {
            if (!_isCompleteCraft) return;
            _craftService.TakeCraftResult(_currentData.Id);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.factories;
using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using infrastructure.services.lumber;
using network;
using TMPro;
using ui.inventory;
using ui.inventory.equipSlot;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.lumber
{
    public class LumberView : Popup
    {
        [SerializeField] private Sprite[] _destroyIcons;

        [SerializeField] private Image _destroyImage;
        [SerializeField] private TMP_Text _energy;
        [SerializeField] private TMP_Text _energyTimer;
        [SerializeField] private TMP_Text _cooldownTimer;
        [SerializeField] private GameObject _dropsPanel;
        [SerializeField] private Button _hitButton;
        [SerializeField] private LoadingPanel _loadingPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _closeBGButton;

        private ILumberService _lumberService;
        private float _cooldownDeadline;

        private readonly List<UiItem> _currentDroppedItems = new List<UiItem>();

        [Inject]
        public void Construct(ILumberService lumberService, IInventoryService inventoryService)
        {
            _lumberService = lumberService;

            _lumberService.OnInfoUpdated += SetInfo;
            _lumberService.OnHitResult += UpdateView;
            _lumberService.OnCooldown += OnCooldownReceived;
        }

        public override void Initialize()
        {
            _hitButton.onClick.AddListener(Hit);
            _closeButton.onClick.AddListener(Back);
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

        public override void Show()
        {
            // bool hasAxe = _inventoryService.Items.Any(i => i.EquipSlotType == EquipSlotType.Axe);
            // if (!hasAxe)
            //     return;

            _dropsPanel.SetActive(false);
            _lumberService.GetInfo(_lumberService.CurrentTreeId);
            _loadingPanel.Show();
            base.Show();
        }

        private void Update()
        {
            UpdateEnergyTimer();
            UpdateCooldownDisplay();
        }

        private void SetInfo()
        {
            UpdateDestroyBlock();
            UpdateEnergyValue();
            UpdateCooldownDeadline();
            _loadingPanel.Hide();
        }

        private void UpdateView()
        {
            UpdateCooldownDeadline();
            UpdateDestroyBlock();
            UpdateDrops();
            UpdateEnergyValue();
        }

        private void OnCooldownReceived()
        {
            UpdateCooldownDeadline();
            UpdateDestroyBlock();
        }

        private void UpdateCooldownDeadline()
        {
            _cooldownDeadline = Time.time + _lumberService.CooldownRemaining;
            UpdateHitButtonState();
        }

        private void UpdateCooldownDisplay()
        {
            float remaining = _cooldownDeadline - Time.time;
            if (remaining <= 0)
            {
                _cooldownTimer.gameObject.SetActive(false);
                UpdateHitButtonState();
                return;
            }

            _cooldownTimer.gameObject.SetActive(true);
            var ts = TimeSpan.FromSeconds(remaining);
            _cooldownTimer.text = $"{ts.Minutes:00}:{ts.Seconds:00}";
            _hitButton.interactable = false;
        }

        private void UpdateHitButtonState()
        {
            _hitButton.interactable = _cooldownDeadline - Time.time <= 0 && _lumberService.CurrentEnergy > 0;
        }

        private void UpdateEnergyTimer()
        {
            if (_lumberService.UpdateLocalEnergy())
            {
                UpdateEnergyValue();
            }

            if (_lumberService.CurrentEnergy >= _lumberService.MaxEnergy) return;

            var time = TimeSpan.FromSeconds(_lumberService.RecoveryEnergyDuration) -
                       (NetworkManager.ServerNow - _lumberService.EnergyUpdateTime);

            _energyTimer.text = $"({time.Minutes:00}:{time.Seconds:00})";
        }

        private void UpdateEnergyValue()
        {
            _energy.text = $"{_lumberService.CurrentEnergy}/{_lumberService.MaxEnergy}";
            _energyTimer.gameObject.SetActive(_lumberService.CurrentEnergy < _lumberService.MaxEnergy);
            UpdateHitButtonState();
        }

        private void Hit()
        {
            _lumberService.Hit(_lumberService.CurrentTreeId);
            HitDelay().Forget();
        }

        private async UniTaskVoid HitDelay()
        {
            _hitButton.interactable = false;
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            UpdateHitButtonState();
        }

        private void UpdateDestroyBlock()
        {
            if (_lumberService.MaxHealth == 0 || _lumberService.CooldownRemaining > 0)
            {
                _destroyImage.sprite = _destroyIcons.Last();
                return;
            }

            float hpPercent = (float)_lumberService.CurrentHealth / _lumberService.MaxHealth;

            int index = Mathf.FloorToInt((1f - hpPercent) * _destroyIcons.Length);
            index = Mathf.Clamp(index, 0, _destroyIcons.Length - 1);

            _destroyImage.sprite = _destroyIcons[index];
        }

        private void UpdateDrops()
        {
            _currentDroppedItems.ForEach(item => item.Release());
            _currentDroppedItems.Clear();
            
            if (_lumberService.LastDrops.Count == 0)
            {
                _dropsPanel.SetActive(false);
                return;
            }

            _dropsPanel.SetActive(true);
            foreach (var (itemId, count) in _lumberService.LastDrops)
            {
                if (count == 0) continue;
                
                var item = Pool.Get<UiItem>();
                item.SetParent(_dropsPanel.transform);
                item.Initialize(new Item()
                {
                    Id = itemId,
                    Count = count
                }, null, false);
                _currentDroppedItems.Add(item);
            }
        }
    }
}

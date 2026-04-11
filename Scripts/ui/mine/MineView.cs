using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.mine;
using network;
using TMPro;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.mine
{
    public class MineView : Popup
    {
        [SerializeField] private Sprite[] _resourceIcons;
        [SerializeField] private Sprite[] _destroyIcons;
        
        [SerializeField] private TMP_Text _energy;
        [SerializeField] private TMP_Text _energyTimer;
        [SerializeField] private TMP_Text _stoneChance;
        [SerializeField] private TMP_Text _coalChance;
        [SerializeField] private TMP_Text _ironChance;
        [SerializeField] private TMP_Text _goldChance;
        [SerializeField] private TMP_Text _diamondChance;

        [SerializeField] private Image _rewardImage;
        [SerializeField] private Image _destroyImage;

        [SerializeField] private Button _blockButton;
        [SerializeField] private Animation _destroyAnimation;

        [SerializeField] private LoadingPanel _loadingPanel;
        
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _closeBGButton;

        private IMineService _mineService;
        
        [Inject]
        public void Construct(IMineService mineService)
        {
            _mineService = mineService;
            
            _mineService.OnInfoUpdated += SetInfo;
            _mineService.OnHitResult += UpdateInfo;
            _mineService.OnReward += SetReward;
        }

        public override void Initialize()
        {
            _blockButton.onClick.AddListener(Hit);
            _closeButton.onClick.AddListener(Back);
        }

        private void Update()
        {
            UpdateEnergyTimer();
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
            GetInfo();
            base.Show();
        }

        private void SetInfo()
        {
            UpdateInfo();
            
            _stoneChance.text = $"{_mineService.GetChancePercent(MineResourceType.Stone):0.#}%";
            _coalChance.text = $"{_mineService.GetChancePercent(MineResourceType.Coal):0.#}%";
            _ironChance.text = $"{_mineService.GetChancePercent(MineResourceType.Iron):0.#}%";
            _goldChance.text = $"{_mineService.GetChancePercent(MineResourceType.Gold):0.#}%";
            _diamondChance.text = $"{_mineService.GetChancePercent(MineResourceType.Diamond):0.#}%";
            
            _loadingPanel.Hide();
        }

        private void UpdateInfo()
        {
            UpdateEnergyValue();
            UpdateDestroyBlock();
        }

        private void SetReward(MineResourceType resourceType)
        {
            _rewardImage.sprite = _resourceIcons[(int)resourceType];
            UpdateDestroyBlock();
            ShowReward().Forget();
        }

        private void Hit()
        {
            _mineService.Hit();
            
            HitDelay().Forget();
        }

        private void GetInfo()
        {
            _mineService.GetInfo();
            _loadingPanel.Show();
        }

        private async UniTaskVoid ShowReward()
        {
            _rewardImage.gameObject.SetActive(true);
            _destroyAnimation.gameObject.SetActive(true);
            _blockButton.gameObject.SetActive(false);
            _destroyAnimation.Play();
            
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            
            _blockButton.gameObject.SetActive(true);
            _rewardImage.gameObject.SetActive(false);
            _destroyAnimation.gameObject.SetActive(false);
        }

        private async UniTaskVoid HitDelay()
        {
            _blockButton.interactable = false;
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _blockButton.interactable = true;
        }

        private void UpdateDestroyBlock()
        {
            if (_mineService.BlockHealth == 100)
            {
                _destroyImage.gameObject.SetActive(false);
                return;
            }
            
            _destroyImage.gameObject.SetActive(true);
            
            float hpPercent = (float)_mineService.BlockHealth / 100;

            int index = Mathf.FloorToInt((1f - hpPercent) * _destroyIcons.Length);
            index = Mathf.Clamp(index, 0, _destroyIcons.Length - 1);

            _destroyImage.sprite = _destroyIcons[index];
        }

        private void UpdateEnergyTimer()
        {
            if (_mineService.UpdateLocalEnergy())
            {
                UpdateEnergyValue();
            }
            
            if (_mineService.CurrentEnergy >= _mineService.MaxEnergy) return;
            
            var time = TimeSpan.FromSeconds(_mineService.RecoveryEnergyDuration) -
                       (NetworkManager.ServerNow - _mineService.EnergyUpdateTime);
            
            _energyTimer.text = $"({time.Minutes:00}:{time.Seconds:00})";
        }

        private void UpdateEnergyValue()
        {
            _energy.text = $"{_mineService.CurrentEnergy}/{_mineService.MaxEnergy}";
            _energyTimer.gameObject.SetActive(_mineService.CurrentEnergy < _mineService.MaxEnergy);
        }
    }
}
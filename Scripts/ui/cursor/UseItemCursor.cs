using DG.Tweening;
using infrastructure.services.input;
using infrastructure.services.inventory.items;
using infrastructure.services.players;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.cursor
{
    public class UseItemCursor : MonoBehaviour
    {
        [SerializeField] private Image _image;

        private IInputService _inputService;
        private ICharacterService _characterService;
        
        private Tween _tween;

        [Inject]
        public void Construct(IInputService inputService, ICharacterService characterService)
        {
            _inputService = inputService;
            _characterService = characterService;
        }
        
        private void OnEnable()
        {
            _inputService.OnAttackEvent += Launch;
            _inputService.OnStopAttackEvent += StopFill;
            _image.fillAmount = 0;
        }

        private void OnDisable()
        {
            _inputService.OnAttackEvent -= Launch;
            _inputService.OnStopAttackEvent -= StopFill;
        }

        private void Launch()
        {
            if (!_inputService.CursorIsLocked) return;
            if (_characterService.CurrentCharacter.HandItemsController.CurrentItem == null) return;
            if (_characterService.CurrentCharacter.HandItemsController.CurrentItem.Data.ItemType != ItemType.Food) return;
            if (_characterService.CurrentCharacter.HandItemsController.CurrentItem.IsCooldown) return;
            
            _image.fillAmount = 0;
            _tween = _image.DOFillAmount(1, 1f).SetEase(Ease.Linear);
        }

        private void StopFill()
        {
            _tween?.Kill();
            _image.fillAmount = 0;
        }
    }
}
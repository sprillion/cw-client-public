using DG.Tweening;
using infrastructure.services.players;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.death
{
    public class DeathView : Popup
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _freeRebornButton;
        
        private ICharacterService _characterService;

        [Inject]
        public void Construct(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        public override void Initialize()
        {
            _characterService.OnCurrentCharacterDead += Show;
            _characterService.OnCurrentCharacterRevival += Hide;
            
            _freeRebornButton.onClick.AddListener(FreeReborn);

            if (_characterService.CurrentCharacter.IsDead)
            {
                Show();
            }
        }

        private void OnDestroy()
        {
            _characterService.OnCurrentCharacterDead -= Show;
            _freeRebornButton.onClick.RemoveListener(FreeReborn);
        }

        public override void Show()
        {
            _freeRebornButton.interactable = true;
            _canvasGroup.interactable = false;
            _canvasGroup.alpha = 0;
            base.Show();
            _canvasGroup.DOFade(1, 2f).OnComplete(() => _canvasGroup.interactable = true);
        }

        private void FreeReborn()
        {
            _characterService.RevivalRequest();
            _freeRebornButton.interactable = false;
        }
    }
}
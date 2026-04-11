using infrastructure.services.map;
using infrastructure.services.players;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.admin
{
    public class AdminPanelView : Popup
    {
        [SerializeField] private Button _noclipButton;
        [SerializeField] private Image  _noclipHighlight;

        private ICharacterService _characterService;
        private IMapService _mapService;

        [Inject]
        public void Construct(ICharacterService characterService, IMapService mapService)
        {
            _characterService = characterService;
            _mapService = mapService;
        }

        public override void Initialize()
        {
            _noclipButton.onClick.AddListener(ToggleNoclip);
            UpdateNoclipVisual();
        }

        // Bypass PopupService so the panel doesn't lock cursor/input.
        public override void Show() => gameObject.SetActive(true);
        public override void Hide() => gameObject.SetActive(false);

        private void ToggleNoclip()
        {
            var movement = _characterService.CurrentCharacter?.CharacterMovement;
            if (movement == null) return;

            var enabling = !movement.IsNoclipEnabled;
            movement.SetNoclip(enabling);

            if (enabling)
            {
                _characterService.StopSendPosition();
                _mapService.SetViewerOverride(movement.NoclipGhost);
            }
            else
            {
                _characterService.ResumeSendPosition();
                _mapService.ClearViewerOverride();
            }

            UpdateNoclipVisual();
        }

        private void UpdateNoclipVisual()
        {
            if (_noclipHighlight == null) return;
            var movement = _characterService.CurrentCharacter?.CharacterMovement;
            _noclipHighlight.enabled = movement != null && movement.IsNoclipEnabled;
        }

        // TO ADD FUTURE FEATURES:
        // 1. Add [SerializeField] Button for the new feature
        // 2. AddListener in Initialize()
        // 3. Write a private toggle/action method using injected services
        // 4. Wire the button in the AdminPanel prefab in the editor
    }
}

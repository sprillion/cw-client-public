using System.Collections.Generic;
using infrastructure.services.input;
using ui.popup;

namespace infrastructure.services.ui.popup
{
    public class PopupService : IPopupService
    {
        private readonly HashSet<Popup> _openedPopups = new HashSet<Popup>();

        private readonly IInputService _inputService;
        
        public PopupService(IInputService inputService)
        {
            _inputService = inputService;

            _inputService.OnBackEvent += Back;
        }
        
        public void AddPopup(Popup popup)
        {
            _openedPopups.Add(popup);
            
            _inputService.UnlockCursor(true);
            _inputService.DisableFullInput();
        }

        public void RemovePopup(Popup popup)
        {
            _openedPopups.Remove(popup);

            if (_openedPopups.Count == 0)
            {
                _inputService.LockCursor(true);
                _inputService.EnableFullInput();
            }
        }

        private void Back()
        {
            //TODO: использовать ESC для возвращения в попапах
            //_openedPopups.LastOrDefault()?.Back();
        }
    }
}
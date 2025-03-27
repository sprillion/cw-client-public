using System.Collections.Generic;
using System.Linq;
using infrastructure.services.input;
using ui.popup;

namespace infrastructure.services.ui.popup
{
    public class PopupService : IPopupService
    {
        private readonly List<Popup> _openedPopups = new List<Popup>();

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
        }

        public void RemovePopup(Popup popup)
        {
            _openedPopups.Remove(popup);

            if (_openedPopups.Count == 0)
            {
                _inputService.LockCursor(true);
            }
        }

        private void Back()
        {
            _openedPopups.LastOrDefault()?.Back();
        }
    }
}
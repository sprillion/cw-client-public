using ui.popup;

namespace infrastructure.services.ui.popup
{
    public interface IPopupService
    {
        void AddPopup(Popup popup);
        void RemovePopup(Popup popup);
    }
}
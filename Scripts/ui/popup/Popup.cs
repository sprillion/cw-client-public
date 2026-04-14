using infrastructure.services.ui.popup;
using UnityEngine;
using Zenject;

namespace ui.popup
{
    public class Popup : MonoBehaviour
    {
        private Popup _backPopup;

        private IPopupService _popupService;
        
        public bool IsActive => gameObject.activeSelf;

        [Inject]
        public void BaseConstruct(IPopupService popupService)
        {
            _popupService = popupService;
        }
        
        public virtual void Initialize() { }

        public virtual void Show()
        {
            AddToStack();
            gameObject.SetActive(true);
        }

        public virtual void Show(Popup backPopup)
        {
            _backPopup = backPopup;
            _backPopup?.Hide();
            Show();
        }

        public virtual void Show(params object[] args)
        {
            Show();
        }
        
        public virtual void Show(Popup backPopup, params object[] args)
        {
            Show(backPopup);
        }

        public virtual void Hide()
        {
            RemoveFromStack();
            gameObject.SetActive(false);
        }

        public virtual void Back()
        {
            if (_backPopup)
            {
                _backPopup.Show();
                _backPopup = null;
            }
            Hide();
        }

        public virtual void AddToStack()
        {
            if (!IsActive)
            {
                _popupService.AddPopup(this);
            }
        }
        
        public virtual void RemoveFromStack()
        {
            if (IsActive)
            {
                _popupService.RemovePopup(this);
            }
        }
    }
}
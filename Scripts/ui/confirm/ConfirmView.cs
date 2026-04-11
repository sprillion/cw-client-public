using System;
using TMPro;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;

namespace ui.confirm
{
    public class ConfirmView : Popup
    {
        [SerializeField] private TMP_Text _confirmText;
        
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private Action _currentAction;

        public override void Initialize()
        {
            _confirmButton.onClick.AddListener(Confirm);
            _cancelButton.onClick.AddListener(Back);
        }
        
        public override void Hide()
        {
            _currentAction = null;
            base.Hide();
        }

        public void SetConfirmInfo(string confirmText, Action action)
        {
            _confirmText.text = confirmText;
            _currentAction = action;
        }

        private void Confirm()
        {
            _currentAction?.Invoke();
            Back();
        }
    }
}
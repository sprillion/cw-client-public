using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ui.tools
{
    public class KeyboardButton : MonoBehaviour
    {
        [SerializeField] private Key _keyCode = Key.None;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;
        
        private bool _keyPressed;

        private void Awake()
        {
            if (_text == null) return;
            _text.text = GetKetString();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
        
            bool keyDown = Keyboard.current[_keyCode].wasPressedThisFrame;
        
            if (keyDown && !_keyPressed)
            {
                _keyPressed = true;
                if (_button.interactable)
                {
                    _button.onClick?.Invoke();
                }
            }
            else if (!keyDown)
            {
                _keyPressed = false;
            }
        }

        private string GetKetString()
        {
            return _keyCode switch
            {
                Key.Digit1 => "1",
                Key.Digit2 => "2",
                Key.Digit3 => "3",
                Key.Digit4 => "4",
                _ => _keyCode.ToString()
            };
        }
    }
}
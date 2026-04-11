using infrastructure.services.input;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.tools
{
    public class DisableCursorButton : MonoBehaviour
    {
        private Button _button;

        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (!_inputService.IsMobile)
            {
                _button.onClick.AddListener(() => _inputService.LockCursor());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
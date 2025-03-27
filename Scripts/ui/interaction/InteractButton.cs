using infrastructure.services.ui;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.interaction
{
    public class InteractButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        
        private IUiService _uiService;

        [Inject]
        public void Construct(IUiService uiService)
        {
            _uiService = uiService;
            
            _button.onClick.AddListener(Interact);
        }

        private void Interact()
        {
            if (_uiService.Inventory.IsActive) return;
            _uiService.Interaction.Interact();
        }

    }
}
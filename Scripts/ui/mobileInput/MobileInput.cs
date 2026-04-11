using infrastructure.services.input;
using UnityEngine;
using Zenject;

namespace ui.mobileInput
{
    public class MobileInput : MonoBehaviour
    {
        [Inject]
        public void Construct(IInputService inputService)
        {
            if (!inputService.IsMobile)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
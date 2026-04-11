using System;
using infrastructure.services.input;
using UnityEngine;
using Zenject;

namespace ui.tools
{
    public class Joystick : MonoBehaviour
    {
        [SerializeField] private FixedJoystick _fixedJoystick;
        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Update()
        {
            _inputService.OnMove(_fixedJoystick.Direction);
        }
    }
}
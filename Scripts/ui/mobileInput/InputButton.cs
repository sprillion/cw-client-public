using System;
using infrastructure.services.input;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace ui.tools
{
    public class InputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private ActionType _actionType;

        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }   
        
        public void OnPointerDown(PointerEventData eventData)
        {
            switch (_actionType)
            {
                case ActionType.Jump:
                    _inputService.OnJump();
                    break;
                case ActionType.Attack:
                    _inputService.OnAttack();
                    break;
                case ActionType.Interract:
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            switch (_actionType)
            {
                case ActionType.Jump:
                    _inputService.OnStopJump();
                    break;
                case ActionType.Attack:
                    break;
                case ActionType.Interract:
                    break;
            }
        }
    }
}
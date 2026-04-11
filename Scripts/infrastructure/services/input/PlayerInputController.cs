using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace infrastructure.services.input
{
    public class PlayerInputController : MonoBehaviour, IPlayerInputController
    {
        [SerializeField] private PlayerInput _playerInput;
        
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnAttack;
        public event Action OnStopAttack;
        public event Action OnInteract;
        public event Action OnJump;
        public event Action OnStopJump;
        public event Action<float> OnZoom;
        public event Action OnChangeCursor;
        public event Action OnBack;
        

        public void Move(InputAction.CallbackContext context)
        {
            OnMove?.Invoke(context.ReadValue<Vector2>());
        }
        
        public void Look(InputAction.CallbackContext context)
        {
            OnLook?.Invoke(context.ReadValue<Vector2>());
        }
        
        public void Attack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAttack?.Invoke();
            }
            else if (context.canceled)
            {
                OnStopAttack?.Invoke();
            }
        }
        
        public void Interact(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnInteract?.Invoke();
            }
        }
        
        public void Jump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnJump?.Invoke();
            }
            else if (context.canceled)
            {
                OnStopJump?.Invoke();
            }
        }

        public void CursorLock(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnChangeCursor?.Invoke();
            }
        }

        public void Zoom(InputAction.CallbackContext context)
        {
            OnZoom?.Invoke(context.ReadValue<float>());
        }
        
        public void Back(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnBack?.Invoke();
            }
        }
    }
}
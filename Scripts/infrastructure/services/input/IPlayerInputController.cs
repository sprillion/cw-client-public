using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace infrastructure.services.input
{
    public interface IPlayerInputController
    {
        event Action<Vector2> OnMove;
        event Action<Vector2> OnLook;
        event Action OnAttack;
        event Action OnStopAttack;
        event Action OnInteract;
        event Action OnJump;
        event Action OnStopJump;
        event Action<float> OnZoom;
        event Action OnChangeCursor;
        event Action OnBack;

        void Attack(InputAction.CallbackContext context);
        void Interact(InputAction.CallbackContext context);
        void Jump(InputAction.CallbackContext context);
    }
}
using System;
using UnityEngine;

namespace infrastructure.services.input
{
    public interface IPlayerInputController
    {
        event Action<Vector2> OnMove;
        event Action<Vector2> OnLook;
        event Action OnAttack;
        event Action OnInteract;
        event Action OnJump;
        event Action<float> OnZoom;
        event Action OnChangeCursor;
        event Action OnBack;
    }
}
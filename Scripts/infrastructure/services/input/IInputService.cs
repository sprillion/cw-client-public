
using System;
using UnityEngine;

namespace infrastructure.services.input
{
    public interface IInputService
    {
        bool CursorIsLocked { get; }
        Vector2 Move { get; }
        event Action OnAttackEvent;
        event Action OnJumpEvent;
        event Action OnChangeCursorEvent;
        event Action<float> OnZoomEvent;
        event Action OnBackEvent;
        void SetAngle(float angle);
        void LockCursor(bool strong = false);
        void UnlockCursor(bool strong = false);
    }
}
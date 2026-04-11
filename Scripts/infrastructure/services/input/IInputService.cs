
using System;
using UnityEngine;

namespace infrastructure.services.input
{
    public interface IInputService
    {
        bool FullInputEnabled { get; }
        bool CursorIsLocked { get; }
        bool IsMobile { get; }
        Vector2 Move { get; }
        event Action OnAttackEvent;
        event Action OnStopAttackEvent;
        event Action OnJumpEvent;
        event Action OnStopJumpEvent;
        event Action OnChangeCursorEvent;
        event Action<float> OnZoomEvent;
        event Action OnBackEvent;
        event Action OnInteractEvent;
        void SetAngle(float angle);
        void LockCursor(bool strong = false);
        void UnlockCursor(bool strong = false);
        void DisableFullInput();
        void EnableFullInput();
        void OnMove(Vector2 vector2);
        void OnAttack();
        void OnStopAttack();
        void OnJump();
        void OnStopJump();
    }
}
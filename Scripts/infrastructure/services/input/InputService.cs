using System;
using UnityEngine;

namespace infrastructure.services.input
{
    public class InputService : IInputService
    {
        private readonly IPlayerInputController _playerInputController;
        
        private float _angle;
        private bool _angleChanged;

        private bool _cursorUnlockedStrong;
        
        public Vector2 Move { get; private set; }

        public bool FullInputEnabled { get; private set; } = true;
        public bool CursorIsLocked { get; private set; }
        public bool IsMobile { get; private set; } = Application.isMobilePlatform;
        // public bool IsMobile { get; private set; } = true;
        public event Action OnAttackEvent;
        public event Action OnStopAttackEvent;
        public event Action OnJumpEvent;
        public event Action OnStopJumpEvent;
        public event Action OnChangeCursorEvent;
        public event Action<float> OnZoomEvent;
        public event Action OnBackEvent;
        public event Action OnInteractEvent;

        public InputService(IPlayerInputController playerInputController)
        {
            _playerInputController = playerInputController;
            
            _playerInputController.OnMove += OnMove;
            _playerInputController.OnAttack += OnAttack;
            _playerInputController.OnStopAttack += OnStopAttack;
            _playerInputController.OnJump += OnJump;
            _playerInputController.OnStopJump += OnStopJump;
            _playerInputController.OnInteract += OnInteract;
            _playerInputController.OnChangeCursor += OnChangeCursor;
            _playerInputController.OnZoom += OnZoom;
            _playerInputController.OnBack += OnBack;
        }

        public void SetAngle(float angle)
        {
            if (Mathf.Approximately(_angle, angle)) return;
            _angle = angle;
        }

        public void LockCursor(bool strong = false)
        {
            if (IsMobile) return;
            if (_cursorUnlockedStrong && !strong) return;
            _cursorUnlockedStrong = false;
            
            Cursor.lockState = CursorLockMode.Locked;
            CursorIsLocked = true;
            OnChangeCursorEvent?.Invoke();
        }
        
        public void UnlockCursor(bool strong = false)
        {
            if (_cursorUnlockedStrong) return;
            _cursorUnlockedStrong = strong;
            
            Cursor.lockState = CursorLockMode.None;
            CursorIsLocked = false;
            OnChangeCursorEvent?.Invoke();
        }

        public void DisableFullInput()
        {
            FullInputEnabled = false;
        }

        public void EnableFullInput()
        {
            FullInputEnabled = true;
        }

        public void OnMove(Vector2 vector2)
        {
            Move = vector2;
        }

        public void OnAttack()
        {
            OnAttackEvent?.Invoke();
        }
        
        public void OnStopAttack()
        {
            OnStopAttackEvent?.Invoke();
        }
        
        public void OnJump()
        {
            OnJumpEvent?.Invoke();
        }
        public void OnStopJump()
        {
            OnStopJumpEvent?.Invoke();
        }

        private void OnInteract()
        {
            OnInteractEvent?.Invoke();
        }
        
        private void OnChangeCursor()
        {
            if (IsMobile) return;
            if (_cursorUnlockedStrong) return;
            Cursor.lockState = CursorIsLocked ? CursorLockMode.None : CursorLockMode.Locked;
            CursorIsLocked = !CursorIsLocked;
            OnChangeCursorEvent?.Invoke();
        }
        
        private void OnZoom(float axis)
        {
            axis *= Time.deltaTime;
#if UNITY_WEBGL && !UNITY_EDITOR
            axis *= 0.05f;
#endif
#if UNITY_EDITOR
            axis *= 10f;
#endif
            OnZoomEvent?.Invoke(axis);
        }

        private void OnBack()
        {
            OnBackEvent?.Invoke();
        }
    }
}
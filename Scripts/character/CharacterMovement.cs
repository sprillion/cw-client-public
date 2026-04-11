using System;
using Cysharp.Threading.Tasks;
using ECM2;
using infrastructure.services.input;
using infrastructure.services.sounds;
using network;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace character
{
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private ECM2.Character _character;
        [SerializeField] private CharacterAnimator _characterAnimator;
        [SerializeField] private CharacterCamera _characterCamera;
        
        [SerializeField] private float _noclipSpeed = 15f;
        private const float NoclipLookSensitivity = 0.15f;
        private const float NoclipSpeedScrollMultiplier = 5f;

        private IInputService _inputService;
        private INetworkManager _networkManager;
        private ISoundService _soundService;
        private Transform _cameraTransform;
        private bool _wasGrounded;

        private Transform _noclipGhost;
        private float _noclipYaw;
        private float _noclipPitch;

        public bool IsNoclipEnabled { get; private set; }
        public Transform NoclipGhost => _noclipGhost;

        public event Action Landed;
        public CharacterAnimator CharacterAnimator => _characterAnimator;

        
        [Inject]
        public void Construct(IInputService inputService, INetworkManager networkManager, ISoundService soundService)
        {
            _inputService = inputService;
            _networkManager = networkManager;
            _soundService = soundService;
            _cameraTransform = Camera.main.transform;

            EnableMovement();
        }

        private void Update()
        {
            CheckLanding();
            if (!_inputService.FullInputEnabled) return;
            if (IsNoclipEnabled)
                NoclipLook();
            else
                Rotation();
        }

        private void NoclipLook()
        {
            if (!_inputService.CursorIsLocked && !_inputService.IsMobile) return;
            var mouseDelta = Mouse.current?.delta.ReadValue() ?? Vector2.zero;
            _noclipYaw   += mouseDelta.x * NoclipLookSensitivity;
            _noclipPitch -= mouseDelta.y * NoclipLookSensitivity;
            _noclipPitch  = Mathf.Clamp(_noclipPitch, -89f, 89f);
            _noclipGhost.rotation = Quaternion.Euler(_noclipPitch, _noclipYaw, 0f);
            var cam = _cameraTransform;
            cam.SetPositionAndRotation(_noclipGhost.position, _noclipGhost.rotation);
        }

        private void CheckLanding()
        {
            bool isGrounded = _character.IsGrounded();
            if (isGrounded && !_wasGrounded)
                Landed?.Invoke();
            _wasGrounded = isGrounded;
        }
        
        public void EnableMovement()
        {
            enabled = true;
            _networkManager.Update += Tick;
            _inputService.OnJumpEvent += Jump;
            _inputService.OnStopJumpEvent += _character.StopJumping;
        }

        public void DisableMovement()
        {
            enabled = false;
            _networkManager.Update -= Tick;
            _inputService.OnJumpEvent -= Jump;
            _inputService.OnStopJumpEvent -= _character.StopJumping;
        }

        public void SetNoclip(bool enabled)
        {
            IsNoclipEnabled = enabled;
            if (enabled)
            {
                if (_noclipGhost == null)
                    _noclipGhost = new GameObject("NoclipGhost").transform;

                // Start ghost at character position; initialise look angles from current yaw
                _noclipGhost.position = transform.position;
                _noclipYaw   = transform.eulerAngles.y;
                _noclipPitch = 0f;
                _noclipGhost.rotation = Quaternion.Euler(_noclipPitch, _noclipYaw, 0f);

                // Disable Cinemachine; we drive Camera.main directly each frame
                _characterCamera.EnableNoclip();

                // Scroll wheel adjusts fly speed instead of camera zoom
                _inputService.OnZoomEvent += AdjustNoclipSpeed;
            }
            else
            {
                _inputService.OnZoomEvent -= AdjustNoclipSpeed;
                if (_noclipGhost != null)
                    _character.SetPosition(_noclipGhost.position);
                _characterCamera.DisableNoclip();
            }
        }

        private void AdjustNoclipSpeed(float delta)
        {
            _noclipSpeed = Mathf.Clamp(_noclipSpeed + delta * NoclipSpeedScrollMultiplier, 1f, 100f);
        }

        public void SetPosition(Vector3 position)
        {
            _character.SetPosition(position);
            _characterCamera.SnapToTarget();
        }

        public void SetPositionNoSnap(Vector3 position, Quaternion rotation)
        {
            _character.SetPosition(position);
            _character.SetRotation(rotation);
        }

        public void EnableCamera()  => _characterCamera.EnableCamera();
        public void DisableCamera() => _characterCamera.DisableCamera();
        
        private void Tick()
        {
            Move();
        }
        
        private void Move()
        {
            Vector3 movementDirection;
            if (_inputService.Move == default || (!_inputService.CursorIsLocked && !_inputService.IsMobile) ||
                !_inputService.FullInputEnabled)
            {
                movementDirection = Vector3.zero;
            }
            else if (IsNoclipEnabled)
            {
                // Move the ghost freely through the world; character stays in place
                var flyDir = (_cameraTransform.forward * _inputService.Move.y
                            + _cameraTransform.right   * _inputService.Move.x).normalized;
                _noclipGhost.position += flyDir * (_noclipSpeed * Time.deltaTime);
                _character.SetMovementDirection(Vector3.zero);
                return;
            }
            else
            {
                movementDirection = (transform.forward * _inputService.Move.y + transform.right * _inputService.Move.x).normalized;
            }
            _character.SetMovementDirection(movementDirection);
        }
        
        private void Rotation()
        {
            if (_inputService.Move == default) return;
            if (!_inputService.CursorIsLocked && !_inputService.IsMobile) return;

            var cameraDirection = _cameraTransform.forward;
            cameraDirection.y = 0;
            
            if (cameraDirection == Vector3.zero) return;

            var targetRotation = Quaternion.LookRotation(cameraDirection);
            _character.SetRotation(targetRotation);

            _inputService.SetAngle(transform.eulerAngles.y);
        }
        
        private void Jump()
        {
            if (!_inputService.CursorIsLocked && !_inputService.IsMobile) return;
            if (!_inputService.FullInputEnabled) return;
            if (!_character.IsGrounded()) return;
            _character.Jump();
            _soundService.Play(SoundType.Jump);
        }
    }
}
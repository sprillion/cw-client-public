using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.input;
using network;
using UnityEngine;
using Zenject;

namespace character
{
    public class CharacterMovement : MonoBehaviour
    {
        private const float MoveSpeed = 4f;
        private const float JumpHeight = 500f;
        private const float GroundCheckRadius = 0.3f;
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private CharacterAnimator _characterAnimator;
        
        private Transform _cameraTransform;
        private float _turnSmoothVelocity;
        private bool _moved;
        private bool _jumped;
        private Vector3 _velocity;
        
        private IInputService _inputService;
        private INetworkManager _networkManager;

        public bool IsGrounded { get; private set; }
        
        [Inject]
        public void Construct(IInputService inputService, INetworkManager networkManager)
        {
            _inputService = inputService;
            _networkManager = networkManager;
            _cameraTransform = Camera.main.transform;
            
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            
            _inputService.OnJumpEvent += Jump;
            _networkManager.Update += Tick;
        }

        private void OnDestroy()
        {
            _inputService.OnJumpEvent -= Jump;
            _networkManager.Update -= Tick;
        }

        public void SetPosition(Vector3 position)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            transform.position = position;

            EnableKinematic().Forget();
        }

        private async UniTaskVoid EnableKinematic()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Time.fixedDeltaTime * 2));
            _rigidbody.isKinematic = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Tick()
        {
            CheckGround();
            Moving();
        }

        private void Update()
        {
            Rotation();
        }

        private void LateUpdate()
        {
            _jumped = false;
        }

        private void Rotation()
        {
            if (_inputService.Move == default) return;
            if (!_inputService.CursorIsLocked) return;

            var cameraDirection = _cameraTransform.forward;
            cameraDirection.y = 0;
            
            if (cameraDirection == Vector3.zero) return;

            var targetRotation = Quaternion.LookRotation(cameraDirection);
            _rigidbody.MoveRotation(targetRotation);

            _inputService.SetAngle(transform.eulerAngles.y);
        }
        
        private void Moving()
        {
            if (_rigidbody.isKinematic) return;
            if (_inputService.Move == default || !_inputService.CursorIsLocked)
            {
                _velocity = default;
            }
            else
            {
                _velocity = (transform.forward * _inputService.Move.y + transform.right * _inputService.Move.x).normalized;
            }
            
            _rigidbody.linearVelocity = new Vector3(_velocity.x * MoveSpeed, _rigidbody.linearVelocity.y, _velocity.z * MoveSpeed);
        }
        
        private void Jump()
        {
            if (!_inputService.CursorIsLocked) return;
            if (_jumped) return;
            if (!IsGrounded) return;
            _rigidbody.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
            _jumped = true;
        }

        private void CheckGround()
        {
            IsGrounded = Physics.CheckSphere(
                transform.position + Vector3.up * 0.2f, 
                GroundCheckRadius, 
                ~LayerMask.GetMask("Player"), 
                QueryTriggerInteraction.Ignore
            );
        }
    }
}
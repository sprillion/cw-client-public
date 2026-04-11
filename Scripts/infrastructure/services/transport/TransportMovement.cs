using character;
using Cysharp.Threading.Tasks;
using infrastructure.services.input;
using infrastructure.services.players;
using network;
using tools;
using UnityEngine;
using Zenject;

namespace infrastructure.services.transport
{
    public class TransportMovement : MonoBehaviour
    {
        [SerializeField] private ECM2.Character _character;

        private IInputService _inputService;
        private INetworkManager _networkManager;
        private Transform _cameraTransform;
        private CharacterMovement _rider;
        private Vector3? _autopilotDirection;

        [Inject]
        public void Construct(IInputService inputService, INetworkManager networkManager)
        {
            _inputService = inputService;
            _networkManager = networkManager;
            _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (_autopilotDirection.HasValue)
                AutopilotRotation();
            else
                Rotation();
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
            _autopilotDirection = null;
            _networkManager.Update -= Tick;
            _inputService.OnJumpEvent -= Jump;
            _inputService.OnStopJumpEvent -= _character.StopJumping;
            StopWithDelay().Forget();
        }

        public void StartAutopilot(Vector3 direction) => _autopilotDirection = direction;
        public void StopAutopilot()                   => _autopilotDirection = null;
        
        public void SetPosition(Vector3 position)
        {
            _character.SetPosition(position);
        }

        public void SetSpeed(float speed) => _character.maxWalkSpeed = speed;

        public void SetRider(CharacterMovement rider) => _rider = rider;
        public void ClearRider()                       => _rider = null;

        private void Tick()
        {
            Move();
            _rider?.SetPositionNoSnap(transform.position, transform.rotation);
            SendPosition();
        }

        private void Move()
        {
            if (_autopilotDirection.HasValue)
            {
                _character.SetMovementDirection(_autopilotDirection.Value);
                return;
            }

            Vector3 movementDirection;
            if (_inputService.Move == default || (!_inputService.CursorIsLocked && !_inputService.IsMobile) ||
                !_inputService.FullInputEnabled)
            {
                movementDirection = Vector3.zero;
            }
            else
            {
                movementDirection = (transform.forward * _inputService.Move.y +
                                     transform.right * _inputService.Move.x).normalized;
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

            _character.SetRotation(Quaternion.LookRotation(cameraDirection));
            _inputService.SetAngle(transform.eulerAngles.y);
        }

        private void AutopilotRotation()
        {
            if (_autopilotDirection.Value == Vector3.zero) return;
            _character.SetRotation(Quaternion.LookRotation(_autopilotDirection.Value));
            _inputService.SetAngle(transform.eulerAngles.y);
        }
        
        private void Jump()
        {
            if (!_inputService.CursorIsLocked && !_inputService.IsMobile) return;
            if (!_inputService.FullInputEnabled) return;
            if (!_character.IsGrounded()) return;
            _character.Jump();
        }

        private void SendPosition()
        {
            var message = new Message(MessageType.Character)
                .AddByte(CharacterService.FromClientMessage.Move.ToByte())
                .AddVector3(transform.position)
                .AddFloat(transform.rotation.eulerAngles.y);
            _networkManager.SendMessage(message);
        }

        private async UniTaskVoid StopWithDelay()
        {
            await UniTask.DelayFrame(1);
            _character.SetMovementDirection(Vector3.zero);
        }
    }
}

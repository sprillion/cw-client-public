using System;
using infrastructure.services.players;
using infrastructure.services.transport;
using network;
using tools;
using UnityEngine;

namespace infrastructure.services.navigation
{
    public class NavigationService : INavigationService
    {
        // Set to true to allow navigation without being mounted on transport.
        // When enabled, also implement the CharacterMovement driving path below.
        private const bool AllowWithoutTransport = false;

        private const float ReachThreshold = 0.75f;

        private enum FromClientMessage : byte
        {
            RequestPath = 0,
        }

        private enum FromServerMessage : byte
        {
            PathResult = 0,
            CooldownRejected = 1,
            NotOnTransport = 2,
        }

        private readonly INetworkManager _networkManager;
        private readonly ICharacterService _characterService;
        private readonly ITransportService _transportService;

        private Vector3[] _corners;
        private int _currentIndex;
        private bool _isActive;

        public bool IsActive => _isActive;

        public event Action OnNavigationStarted;
        public event Action OnNavigationCompleted;
        public event Action OnNavigationCancelled;
        public event Action<float> OnCooldownRejected;
        public event Action OnNotOnTransport;
        public event Action OnPathPartial;
        public event Action OnPathInvalid;

        public NavigationService(INetworkManager networkManager, ICharacterService characterService,
            ITransportService transportService)
        {
            _networkManager = networkManager;
            _characterService = characterService;
            _transportService = transportService;

            _networkManager.Update += Tick;
            _transportService.OnLocalDismounted += CancelNavigation;
        }

        public void RequestPath(Vector3 target)
        {
            if (!AllowWithoutTransport && !_transportService.IsLocalMounted)
            {
                OnNotOnTransport?.Invoke();
                return;
            }

            if (_isActive)
                CancelNavigation();

            var message = new Message(MessageType.Navigation)
                .AddByte(FromClientMessage.RequestPath.ToByte())
                .AddVector3(target);
            _networkManager.SendMessage(message);
        }

        public void ReceiveMessage(Message message)
        {
            var sub = (FromServerMessage)message.GetByte();
            switch (sub)
            {
                case FromServerMessage.PathResult:
                    HandlePathResult(message);
                    break;
                case FromServerMessage.CooldownRejected:
                    HandleCooldownRejected(message);
                    break;
                case FromServerMessage.NotOnTransport:
                    OnNotOnTransport?.Invoke();
                    break;
            }
        }

        public void CancelNavigation()
        {
            if (!_isActive) return;
            StopMovement();
            _isActive = false;
            OnNavigationCancelled?.Invoke();
        }

        private void HandlePathResult(Message message)
        {
            var status = message.GetByte();
            var count = message.GetUShort();

            _corners = new Vector3[count];
            for (int i = 0; i < count; i++)
                _corners[i] = message.GetVector3();

            if (count == 0 || status == 2)
            {
                OnPathInvalid?.Invoke();
                return;
            }

            if (status == 1)
                OnPathPartial?.Invoke();

            _currentIndex = 0;
            _isActive = true;
            OnNavigationStarted?.Invoke();
        }

        private void HandleCooldownRejected(Message message)
        {
            var remaining = message.GetFloat();
            OnCooldownRejected?.Invoke(remaining);
        }

        private void Tick()
        {
            if (!_isActive) return;
            if (_characterService.CurrentCharacter == null)
            {
                CancelNavigation();
                return;
            }

            var pos = GetMovablePosition();
            var target = _corners[_currentIndex];
            var diff = new Vector3(target.x - pos.x, 0f, target.z - pos.z);

            if (diff.sqrMagnitude < 0.001f)
            {
                AdvanceWaypoint();
                return;
            }

            var dir = diff.normalized;

            if (_transportService.IsLocalMounted)
                _transportService.SetAutopilotDirection(dir);
            else if (AllowWithoutTransport)
            {
                // TODO: drive CharacterMovement when foot navigation is enabled
            }

            if (Vector3.Distance(pos, target) < ReachThreshold)
                AdvanceWaypoint();
        }

        private Vector3 GetMovablePosition()
        {
            return _characterService.CurrentCharacter.transform.position;
        }

        private void AdvanceWaypoint()
        {
            _currentIndex++;
            if (_currentIndex >= _corners.Length)
                CompleteNavigation();
        }

        private void CompleteNavigation()
        {
            StopMovement();
            _isActive = false;
            OnNavigationCompleted?.Invoke();
        }

        private void StopMovement()
        {
            if (_transportService.IsLocalMounted)
                _transportService.StopAutopilot();
        }
    }
}
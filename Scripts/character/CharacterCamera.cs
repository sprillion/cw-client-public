using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.input;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace character
{
    public class CharacterCamera : MonoBehaviour
    {
        private const float ZoomSpeed = 500f;
        
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;
        [SerializeField] private CinemachineRotationComposer _rotationComposer;
        [SerializeField] private CinemachineInputAxisController _inputAxisController;
        
        private IInputService _inputService;
        private float _targetValue = 1;
        
        
        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
            _inputService.OnZoomEvent += Zoom;
            _inputService.OnChangeCursorEvent += OnChangeCursor;
        }

        private void Update()
        {
            _orbitalFollow.RadialAxis.Value = Mathf.Lerp(_orbitalFollow.RadialAxis.Value, _targetValue,  ZoomSpeed * Time.deltaTime);
            if (Math.Abs(_orbitalFollow.RadialAxis.Value - _targetValue) < 0.1f) return;
            _orbitalFollow.RadialAxis.Validate();
        }
        
        private void Zoom(float axis)
        {
            _targetValue = _orbitalFollow.RadialAxis.ClampValue(_orbitalFollow.RadialAxis.Value + axis);
        }
        
        private void OnChangeCursor()
        {
            _orbitalFollow.enabled = _inputService.CursorIsLocked;
            _inputAxisController.enabled = _inputService.CursorIsLocked;
        }
    }
}
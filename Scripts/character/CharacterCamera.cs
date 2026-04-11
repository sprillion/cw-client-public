using infrastructure.services.input;
using infrastructure.services.settings;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace character
{
    public class CharacterCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;
        [SerializeField] private CinemachineRotationComposer _rotationComposer;
        [SerializeField] private CinemachineInputAxisController _inputAxisController;
        [SerializeField] private MobileTouchInputAxisController _mobileInputAxisController;

        private IInputService _inputService;
        private ISettingsService _settingsService;

        // Base values from inspector — sensitivity settings act as multipliers
        private float[] _baseGains;
        private float[] _baseMobileGains;

        public float TargetValue { get; private set; } = 1;

        [Inject]
        public void Construct(IInputService inputService, ISettingsService settingsService)
        {
            _inputService = inputService;
            _settingsService = settingsService;
            _inputService.OnZoomEvent += Zoom;
            _inputService.OnChangeCursorEvent += OnChangeCursor;
            _settingsService.OnSensitivityChanged += ApplySensitivity;
        }

        private void Start()
        {
            _inputAxisController.enabled = !_inputService.IsMobile;
            _mobileInputAxisController.enabled = _inputService.IsMobile;

            // Snapshot inspector-configured base gains
            _baseGains = new float[_inputAxisController.Controllers.Count];
            for (int i = 0; i < _baseGains.Length; i++)
                _baseGains[i] = _inputAxisController.Controllers[i].Input.Gain;

            _baseMobileGains = new float[_mobileInputAxisController.Controllers.Count];
            for (int i = 0; i < _baseMobileGains.Length; i++)
                _baseMobileGains[i] = _mobileInputAxisController.Controllers[i].Input.sensitivity;

            ApplySensitivity(_settingsService.Current.sensitivityX, _settingsService.Current.sensitivityY);
        }

        private void OnDestroy()
        {
            if (_settingsService != null)
                _settingsService.OnSensitivityChanged -= ApplySensitivity;
        }

        // index 0 = Horizontal, index 1 = Vertical (matches Cinemachine axis order)
        private void ApplySensitivity(float x, float y)
        {
            if (_baseGains != null)
            {
                if (_baseGains.Length > 0) _inputAxisController.Controllers[0].Input.Gain = _baseGains[0] * x;
                if (_baseGains.Length > 1) _inputAxisController.Controllers[1].Input.Gain = _baseGains[1] * y;
            }

            if (_baseMobileGains != null)
            {
                if (_baseMobileGains.Length > 0) _mobileInputAxisController.Controllers[0].Input.sensitivity = _baseMobileGains[0] * x;
                if (_baseMobileGains.Length > 1) _mobileInputAxisController.Controllers[1].Input.sensitivity = _baseMobileGains[1] * y;
            }
        }

        public Transform FollowTarget => _cinemachineCamera.Follow;

        public void SetFollowTarget(Transform target)
        {
            _cinemachineCamera.Follow = target;
        }

        public void EnableNoclip()
        {
            // Stop Cinemachine from driving the camera and consuming scroll zoom
            _inputService.OnZoomEvent -= Zoom;
            _inputAxisController.enabled = false;
            _mobileInputAxisController.enabled = false;
            _cinemachineCamera.enabled = false;
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null) brain.enabled = false;
        }

        public void DisableNoclip()
        {
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null) brain.enabled = true;
            _cinemachineCamera.enabled = true;
            _inputAxisController.enabled = !_inputService.IsMobile;
            _mobileInputAxisController.enabled = _inputService.IsMobile;
            _inputService.OnZoomEvent += Zoom;
            SnapToTarget();
        }

        public void SnapToTarget()
        {
            if (_cinemachineCamera != null)
                _cinemachineCamera.PreviousStateIsValid = false;
        }

        public void EnableCamera()  => _cinemachineCamera.enabled = true;
        public void DisableCamera() => _cinemachineCamera.enabled = false;

        private void Zoom(float axis)
        {
            TargetValue = Mathf.Clamp(TargetValue + axis, 0.5f, 2f);
        }

        private void OnChangeCursor()
        {
            _orbitalFollow.enabled = _inputService.CursorIsLocked;
            _inputAxisController.enabled = _inputService.CursorIsLocked;
        }
    }
}
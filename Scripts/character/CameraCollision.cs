using Unity.Cinemachine;
using UnityEngine;

namespace character
{
    public class CameraCollision : MonoBehaviour
    {
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;
        [SerializeField] private CharacterCamera _characterCamera;
        [SerializeField] private float _rayDistance = 10f;
        [SerializeField] private float _minDistance = 0.1f;
        [SerializeField] private LayerMask _collisionLayer;
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 5f;
    
        private Vector3 _cameraPosition;
        
        private float _currentRadialValue;

        private void LateUpdate()
        {
            float targetRadialValue;
            
            if (Physics.Raycast(_target.position, transform.position - _target.position, out var hit, _rayDistance, _collisionLayer))
            {
                var currentDistance = Vector3.Distance(_target.position, hit.point);
                var distance = Mathf.InverseLerp(_minDistance, _rayDistance, currentDistance);
                currentDistance = Mathf.Lerp(_minDistance, _characterCamera.TargetValue, distance);
                targetRadialValue = currentDistance;
            }
            else
            {
                targetRadialValue = _characterCamera.TargetValue;
            }
            
            _currentRadialValue = Mathf.Lerp(
                _currentRadialValue, 
                targetRadialValue, 
                Time.deltaTime * _smoothSpeed
            );
    
            _orbitalFollow.RadialAxis.Value = _currentRadialValue;
            _orbitalFollow.RadialAxis.Validate();
        }

    }
}
using System.Collections.Generic;
using network;
using UnityEngine;

namespace character
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private CharacterAnimator _animator;
        [SerializeField] private bool _attachToGround;

        private const int interpolationDelayTicks = 5;
        private const int maxBufferSize = 32;
        private const float teleportDistance = 10f;
        private const float tickDuration = 0.025f;

        private readonly List<EnemySnapshot> _buffer = new();

        private bool _initialized;
        private float _renderTick = -1f;
        private float _groundY;

        private void Update()
        {
            if (!_initialized || _buffer.Count < 2 || _renderTick < 0) return;

            float dt = Mathf.Min(Time.deltaTime, tickDuration * 3f);
            _renderTick += dt / tickDuration;

            if (_renderTick > _buffer[^1].Tick)
                _renderTick = _buffer[^1].Tick;

            EnemySnapshot from = default;
            EnemySnapshot to = default;
            bool found = false;

            for (int i = 0; i < _buffer.Count - 1; i++)
            {
                if (_buffer[i].Tick <= _renderTick && _buffer[i + 1].Tick >= _renderTick)
                {
                    from = _buffer[i];
                    to = _buffer[i + 1];
                    found = true;

                    if (i > 0)
                        _buffer.RemoveRange(0, i);
                    break;
                }
            }

            if (!found) return;

            int tickDiff = to.Tick - from.Tick;
            float t = tickDiff > 0 ? (_renderTick - from.Tick) / tickDiff : 1f;
            t = Mathf.Clamp01(t);

            Vector3 targetPosition = Vector3.Lerp(from.Position, to.Position, t);

            if (Vector3.Distance(transform.position, targetPosition) > teleportDistance)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.Euler(0, Mathf.LerpAngle(from.RotationY, to.RotationY, t), 0);
                return;
            }
            float targetRotation = Mathf.LerpAngle(from.RotationY, to.RotationY, t);

            if (_attachToGround)
            {
                var ray = new Ray(targetPosition + Vector3.up * 1.5f, Vector3.down);
                if (Physics.Raycast(ray, out var hit, 3f, LayerMask.GetMask("Map")))
                    _groundY = Mathf.Lerp(_groundY, hit.point.y, Time.deltaTime * 15f);
                targetPosition.y = _groundY;
            }

            transform.position = targetPosition;
            transform.rotation = Quaternion.Euler(0, targetRotation, 0);

            Vector3 velocity = (to.Position - from.Position) / (tickDiff * tickDuration);
            _animator?.SetDirection(velocity);
        }
        
        public void SetAnimator(CharacterAnimator animator)
        {
            _animator = animator;
        }
        
        public void ApplySnapshot(EnemySnapshot snapshot)
        {
            if (_buffer.Count > 0 && snapshot.Tick <= _buffer[^1].Tick)
                return;

            _buffer.Add(snapshot);

            if (_buffer.Count > maxBufferSize)
            {
                _buffer.RemoveAt(0);
                if (_renderTick < _buffer[0].Tick)
                    _renderTick = _buffer[0].Tick;
            }

            if (!_initialized)
            {
                transform.position = snapshot.Position;
                transform.rotation = Quaternion.Euler(0, snapshot.RotationY, 0);
                _groundY = snapshot.Position.y;
                _initialized = true;
            }

            if (_renderTick < 0 && _buffer.Count >= 2)
                _renderTick = snapshot.Tick - interpolationDelayTicks;
        }

        public void Clear()
        {
            _buffer.Clear();
            _initialized = false;
            _renderTick = -1f;
            _groundY = 0f;
        }
    }
}
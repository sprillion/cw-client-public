using System.Collections.Generic;
using character;
using network;
using UnityEngine;
using Zenject;

namespace infrastructure.services.mobs
{
    public class MobController : MonoBehaviour
    {
        [SerializeField] private MobAnimator _mobAnimator;

        private INetworkManager _networkManager;

        private readonly Queue<EnemyPosition> _mobPositions = new Queue<EnemyPosition>();

        private EnemyPosition _currentMobPosition;

        private Vector3 _velocity;
        private float _rotateVelocity;

        private bool _isDead;
        public MobAnimator MobAnimator => _mobAnimator;
        

        [Inject]
        public void Construct(INetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void Initialize()
        {
            _mobAnimator.Initialize();
            _isDead = false;
            _mobPositions.Clear();
        }
        
        public void SetMobAnimator(MobAnimator mobAnimator)
        {
            _mobAnimator = mobAnimator;
        }

        private void OnEnable()
        {
            if (_networkManager == null) return;
            _networkManager.Update += GetNewPosition;
        }

        private void OnDisable()
        {
            if (_networkManager == null) return;
            _networkManager.Update -= GetNewPosition;
        }

        private void Update()
        {
            if (_isDead) return;
             
            transform.position = Vector3.SmoothDamp(transform.position, _currentMobPosition.Position, ref _velocity,
                Time.fixedDeltaTime);
            
            transform.eulerAngles = new Vector3(0,
                Mathf.SmoothDampAngle(transform.eulerAngles.y, _currentMobPosition.Rotation, ref _rotateVelocity,
                    Time.fixedDeltaTime), 0);
        }

        public void SetPosition(Vector3 position, float rotation, bool teleport)
        {
            if (teleport)
            {
                transform.position = position;
                transform.eulerAngles = new Vector3(0, rotation, 0);
                _currentMobPosition = new EnemyPosition(position, rotation);
                return;
            }
            
            _mobPositions.Enqueue(new EnemyPosition(position, rotation));
            if (_mobPositions.Count > 4)
            {
                _mobPositions.Dequeue();
            }
        }

        public void Death()
        {
            _mobAnimator.Death();
            _isDead = true;
        }

        private void GetNewPosition()
        {
            if (!_mobPositions.TryDequeue(out _currentMobPosition)) return;
            _mobAnimator.SetDirection(_velocity);
        }
    }
}
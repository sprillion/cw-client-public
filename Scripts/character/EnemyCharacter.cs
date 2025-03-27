using System.Collections.Generic;
using network;
using UnityEngine;
using Zenject;

namespace character
{
    public class EnemyCharacter : PooledObject
    {
        [SerializeField] private CharacterAnimator _characterAnimator;
        [SerializeField] private CharacterSkin _characterSkin;

        private INetworkManager _networkManager;

        private readonly Queue<EnemyPosition> _enemyPositions = new Queue<EnemyPosition>();

        private EnemyPosition _currentEnemyPosition;
        private EnemyPosition _lastEnemyPosition;
        
        private float _elapsedTime;
        
        [Inject]
        public void Construct(INetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.Update += GetNewPosition;
        }

        private void OnDestroy()
        {
            _networkManager.Update -= GetNewPosition;
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(_lastEnemyPosition.Position, _currentEnemyPosition.Position, _elapsedTime / Time.fixedDeltaTime);
            transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(_lastEnemyPosition.Rotation, _currentEnemyPosition.Rotation, _elapsedTime / Time.fixedDeltaTime), 0);
            
            _elapsedTime += Time.deltaTime;
        }

        public void SetPosition(Vector3 position, float rotation)
        {
            _enemyPositions.Enqueue(new EnemyPosition(position, rotation));
            if (_enemyPositions.Count > 4)
            {
                _enemyPositions.Dequeue();
            }
        }

        private void GetNewPosition()
        {
            _lastEnemyPosition = _currentEnemyPosition;
            if (!_enemyPositions.TryDequeue(out _currentEnemyPosition)) return;
            _characterAnimator.SetDirection(_currentEnemyPosition.Position - _lastEnemyPosition.Position);
            _elapsedTime = 0;
        }

    }

    public struct EnemyPosition
    {
        public Vector3 Position;
        public float Rotation;

        public EnemyPosition(Vector3 position, float rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
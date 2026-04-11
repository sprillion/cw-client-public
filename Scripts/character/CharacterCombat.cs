using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.input;
using infrastructure.services.mobs;
using infrastructure.services.players;
using network;
using UnityEngine;
using Zenject;

namespace character
{
    public class CharacterCombat : MonoBehaviour
    {
        private const float AttackBaseDuration = 1f;
        private const float AttackHitMoment = 0.75f;
        private const float AttackHitRadius = 1f;
        private const float AttackHitMaxDistance = 0.5f;

        [SerializeField] private CharacterAnimator _characterAnimator;
        [SerializeField] private PotionThrowController _potionThrowController;
        [SerializeField] private Transform _hitPosition;

        private IInputService _inputService;
        private INetworkManager _networkManager;

        private readonly RaycastHit[] _mobsToHit = new RaycastHit[3];

        private int _mobsCount;
        private bool _isAttack;

        private float _attackSpeed = 1;

        [Inject]
        public void Construct(IInputService inputService, INetworkManager networkManager)
        {
            _inputService = inputService;
            _networkManager = networkManager;
        }

        private void OnEnable()
        {
            _inputService.OnAttackEvent += LaunchAttack;
        }

        private void OnDisable()
        {
            _inputService.OnAttackEvent -= LaunchAttack;
        }

        private void LaunchAttack()
        {
            if (_isAttack) return;
            if (!_inputService.CursorIsLocked && !_inputService.IsMobile) return;
            if (_potionThrowController.gameObject.activeSelf && !_potionThrowController.CurrentItem.IsCooldown) return;
            _characterAnimator.Attack();
            Attack().Forget();
        }

        private async UniTaskVoid Attack()
        {
            _isAttack = true;
            await UniTask.Delay(TimeSpan.FromSeconds(AttackBaseDuration * _attackSpeed * AttackHitMoment));
            CheckMobs();
            SendHitMessage();
            await UniTask.Delay(TimeSpan.FromSeconds(AttackBaseDuration * _attackSpeed * (1 - AttackHitMoment)));
            _isAttack = false;
        }

        private void CheckMobs()
        {
            _mobsCount = Physics.SphereCastNonAlloc(_hitPosition.position, AttackHitRadius, _hitPosition.forward,
                _mobsToHit, AttackHitMaxDistance, LayerMask.GetMask("Mob"));
        }

        private void SendHitMessage()
        {
            if (_mobsCount == 0) return;
            var message = new Message(MessageType.Character);
            message.AddByte((byte)CharacterService.FromClientMessage.Hit);
            message.AddInt(_mobsCount);
            
            for (int i = 0; i < _mobsCount; i++)
            {
                if (!_mobsToHit[i].transform.gameObject.TryGetComponent(out Mob mob))
                {
                    Debug.LogError("Ошибка атаки моба");
                    return;
                }
                message.AddUShort(mob.Id);
            }
            
            _networkManager.SendMessage(message);
        }
    }
}
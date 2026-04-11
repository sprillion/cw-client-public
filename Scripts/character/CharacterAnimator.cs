using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace character
{
    public class CharacterAnimator : MonoBehaviour
    {
        private const float AttackAnimationBaseDuration = 1.2f;

        protected static readonly int DirectionX = Animator.StringToHash("DirectionX");
        protected static readonly int DirectionZ = Animator.StringToHash("DirectionZ");
        protected static readonly int MeleeAttack = Animator.StringToHash("MeleeAttack");
        protected static readonly int MeleeAttackSpeed = Animator.StringToHash("MeleeAttackSpeed");
        protected static readonly int IsDead = Animator.StringToHash("IsDead");
        protected static readonly int Throwing = Animator.StringToHash("Throwing");
        protected static readonly int ThrowAnim = Animator.StringToHash("Throw");
        protected static readonly int WithSword = Animator.StringToHash("WithSword");
        protected static readonly int OnGetSword = Animator.StringToHash("OnGetSword");
        protected static readonly int SwordAttackCount = Animator.StringToHash("SwordAttackCount");

        [SerializeField] protected Animator _animator;

        [SerializeField] private GameObject _sword;
        private CancellationTokenSource _cancelAttack;
        private Tween _attackLayerChange;

        private Vector3 _lastPosition;
        private Vector3 _moveDirection;
        private Vector3 _velocity;

        private bool _isThrowing;
        private bool _withSword;
        private int _swordAttackCount;

        public event Action<bool>    OnThrowingChanged;
        public event Action<Vector3> OnMoveDirectionUpdated;

        public virtual void Initialize()
        {
            _animator.SetBool(IsDead, false);
        }

        public void SetAnimatorController(RuntimeAnimatorController animatorController)
        {
            _animator.runtimeAnimatorController = animatorController;
        }

        protected virtual void Update()
        {
            SetDirection();
        }

        public virtual void SetDirection(Vector3 direction = default)
        {
            if (direction == default)
            {
                direction = transform.parent.position - _lastPosition;
                _lastPosition = transform.parent.position;
            }

            direction.y = 0;
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector3.zero;
            else
                direction.Normalize();

            var forward = transform.parent.forward;
            forward.y = 0;
            forward.Normalize();

            var moveDirection = new Vector3(-Vector3.Cross(direction, forward).y, 0, Vector3.Dot(direction, forward))
                .normalized;

            _moveDirection = Vector3.SmoothDamp(_moveDirection, moveDirection, ref _velocity, 0.1f);

            if (_moveDirection.x is < 0.025f and > -0.025f)
            {
                _moveDirection.x = 0f;
            }

            if (_moveDirection.z is < 0.025f and > -0.025f)
            {
                _moveDirection.z = 0f;
            }

            _animator.SetFloat(DirectionX, _moveDirection.x);
            _animator.SetFloat(DirectionZ, _moveDirection.z);

            OnMoveDirectionUpdated?.Invoke(_moveDirection);
        }

        public virtual void Attack()
        {
            if (_isThrowing) return;

            _attackLayerChange?.Kill();
            _cancelAttack?.Cancel();
            _cancelAttack = new CancellationTokenSource();
            AttackLayer().Forget();
            _animator.SetTrigger(MeleeAttack);
            if (_withSword)
            {
                _swordAttackCount++;
                if (_swordAttackCount > 2)
                {
                    _swordAttackCount = 0;
                }

                _animator.SetFloat(SwordAttackCount, _swordAttackCount);
            }
        }

        public virtual void Death()
        {
            _animator.SetBool(IsDead, true);
        }

        public virtual void Revivel()
        {
            _animator.SetBool(IsDead, false);
        }

        public void SetAttackSpeed(float speed)
        {
            _animator.SetFloat(MeleeAttack, speed);
        }

        public void SetSword(bool withSword)
        {
            _withSword = withSword;
            _animator.SetBool(WithSword, withSword);
            if (withSword)
            {
                EnableAttackLayer();
                _animator.SetTrigger(OnGetSword);
            }
            else
            {
                DisableAttackLayer();
            }
        }

        public void SetThrowing(bool isThrowing)
        {
            if (_isThrowing == isThrowing) return;
            _isThrowing = isThrowing;

            OnThrowingChanged?.Invoke(_isThrowing);

            if (_isThrowing)
            {
                EnableAttackLayer();
            }

            _animator.SetBool(Throwing, _isThrowing);
        }

        public void Throw()
        {
            _animator.SetTrigger(ThrowAnim);
            SetThrowing(false);
            DisableAttackLayerWithDelay(0.5f).Forget();
        }

        public void DisableAttackLayer()
        {
            _attackLayerChange?.Kill();
            _attackLayerChange = DOVirtual.Float(1, 0, 1f, (value) => { _animator.SetLayerWeight(1, value); });
        }

        public void ShowSword()
        {
            _sword.SetActive(true);
        }

        public void HideSword()
        {
            _sword.SetActive(false);
        }

        private async UniTaskVoid AttackLayer()
        {
            EnableAttackLayer();
            if (_withSword) return;
            await UniTask.Delay(TimeSpan.FromSeconds(AttackAnimationBaseDuration),
                cancellationToken: _cancelAttack.Token);
            DisableAttackLayer();
        }

        private async UniTaskVoid DisableAttackLayerWithDelay(float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            DisableAttackLayer();
        }

        private void EnableAttackLayer()
        {
            _attackLayerChange?.Kill();
            _animator.SetLayerWeight(1, 1);
        }
    }
}
using character;
using UnityEngine;

namespace infrastructure.services.mobs
{
    public class MobAnimator : CharacterAnimator
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        protected override void Update()
        {
        }

        public override void SetDirection(Vector3 direction = default)
        {
            direction.y = 0;
            _animator.SetFloat(MoveSpeed, direction.magnitude);
        }

        public override void Attack()
        {
            _animator.SetTrigger(MeleeAttack);
        }
    }
}
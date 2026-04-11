using character;
using UnityEngine;

namespace infrastructure.services.mobs
{
    public class MobController : MonoBehaviour
    {
        [SerializeField] private MobAnimator _mobAnimator;
        [SerializeField] private EnemyController _enemyController;

        private bool _isDead;
        public MobAnimator MobAnimator => _mobAnimator;
        public EnemyController EnemyController => _enemyController;

        public void Initialize()
        {
            _mobAnimator.Initialize();
            _isDead = false;
        }

        
        public void SetMobAnimator(MobAnimator mobAnimator)
        {
            _mobAnimator = mobAnimator;
            _enemyController.SetAnimator(mobAnimator);
        }

        public void Death()
        {
            _mobAnimator.Death();
            _isDead = true;
        }

    }
}
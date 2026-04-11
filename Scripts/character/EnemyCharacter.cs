using UnityEngine;
using Zenject;

namespace character
{
    public class EnemyCharacter : PooledObject
    {
        [SerializeField] private CharacterAnimator _characterAnimator;
        [SerializeField] private CharacterSkin _characterSkin;
        [SerializeField] private HandItemsController _handItemsController;
        [SerializeField] private EnemyController _enemyController;

        public CharacterSkin CharacterSkin => _characterSkin;
        public HandItemsController HandItemsController => _handItemsController;
        public EnemyController EnemyController => _enemyController;

        public void SetVisible(bool visible)
        {
            _characterSkin.gameObject.SetActive(visible);
            _handItemsController.gameObject.SetActive(visible);
        }


        [Inject]
        public void Construct()
        {
            _handItemsController.Initialize();
        }

        public override void Release()
        {
            _enemyController.Clear();
            base.Release();
        }
    }
}
using character;
using character.handItems;
using infrastructure.services.inventory.items;
using UnityEngine;
using Zenject;

namespace factories.characters
{
    public class CharacterFactory : ICharacterFactory
    {
        private const string PlayerPrefabPath = "Prefabs/Player/Player";
        private const string EnemyPrefabPath = "Prefabs/Player/Enemy";

        private readonly DiContainer _container;

        private readonly Character _characterPrefab;
        private readonly EnemyCharacter _enemyPrefab;
        private readonly Transform _charactersParent;

        private readonly ObjectPool _enemiesPool;
        private readonly ObjectPool _potionsPool;

        public CharacterFactory(DiContainer container)
        {
            _container = container;
            _characterPrefab = Resources.Load<Character>(PlayerPrefabPath);
            _enemyPrefab = Resources.Load<EnemyCharacter>(EnemyPrefabPath);
            _charactersParent = new GameObject("Characters").transform;
            var potionsParent = new GameObject("Potions").transform;

            _enemiesPool = new ObjectPool(_enemyPrefab, 0, _charactersParent, _container);

            _potionsPool = new ObjectPool(Resources.Load<Potion>("Prefabs/Player/HandItems/Potion"), 0, potionsParent,
                _container);
        }

        public Character CreateCharacter()
        {
            return _container.InstantiatePrefabResourceForComponent<Character>(PlayerPrefabPath, _charactersParent);
        }

        public EnemyCharacter CreateEnemy(string nickname)
        {
            var enemy = (EnemyCharacter)_enemiesPool.GetObject();
            enemy.name = nickname;
            return enemy;
        }

        public CharacterStats CreateCharacterStats()
        {
            return _container.Instantiate<CharacterStats>();
        }

        public Potion GetPotion(Item item)
        {
            var potion = _potionsPool.GetObject<Potion>();
            potion.CurrentItem = item;
            return potion;
        }
    }
}
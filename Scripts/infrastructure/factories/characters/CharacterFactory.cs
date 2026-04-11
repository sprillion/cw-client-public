using character;
using infrastructure.factories;
using UnityEngine;
using Zenject;

namespace factories.characters
{
    public class CharacterFactory : ICharacterFactory
    {
        private const string PlayerPrefabPath = "Prefabs/Player/Player";

        private readonly DiContainer _container;

        private readonly Character _characterPrefab;
        private readonly Transform _charactersParent;

        public CharacterFactory(DiContainer container)
        {
            _container = container;
            _charactersParent = new GameObject("Characters").transform;
        }

        public Character CreateCharacter()
        {
            return _container.InstantiatePrefabResourceForComponent<Character>(PlayerPrefabPath, _charactersParent);
        }

        public EnemyCharacter CreateEnemy(string nickname)
        {
            var enemy = Pool.Get<EnemyCharacter>();
            enemy.name = nickname;
            enemy.SetParentPreserveScale(_charactersParent);
            return enemy;
        }

        public CharacterStats CreateCharacterStats()
        {
            return _container.Instantiate<CharacterStats>();
        }
    }
}
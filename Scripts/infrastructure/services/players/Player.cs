using character;
using UnityEngine;

namespace infrastructure.services.users
{
    public class Player
    {
        public int Id { get; private set; }
        public string Nickname { get; private set; }

        public Vector3 StartPosition { get; private set; }

        public Character Character { get; private set; }
        public EnemyCharacter EnemyCharacter { get; private set; }

        public Player(int id, string nickname, EnemyCharacter enemyCharacter)
        {
            Id = id;
            Nickname = nickname;
            EnemyCharacter = enemyCharacter;
        }
        
        public Player(int id, string nickname, Vector3 startPosition)
        {
            Id = id;
            Nickname = nickname;
            StartPosition = startPosition;
        }

        public void SetCharacter(Character character)
        {
            Character = character;
        }
    }
}